using Hmz.Core._3D.Geometry;
using Hmz.Core.Renderer;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using System.Numerics;
using AssimpMesh = Silk.NET.Assimp.Mesh;
using AssimpTexture = Silk.NET.Assimp.Texture;
using AssimpAnimation = Silk.NET.Assimp.Animation;
using EngineMesh = Hmz.Core._3D.Geometry.Mesh;

namespace Hmz.Core.Content;

// Imports a glTF/OBJ/etc. scene via Assimp: walks the node hierarchy, builds GPU buffers per
// mesh, resolves each material's texture (embedded or external), and — when present — pulls
// out the skeleton (bones + node hierarchy) and animation clips for GPU skinning.
static class ModelLoader
{
  const int MaxBoneWeightsPerVertex = 4;
  const int VertexFloats = 3 /* position */ + 2 /* texcoord */ + MaxBoneWeightsPerVertex /* bone ids */ + MaxBoneWeightsPerVertex /* bone weights */;

  public static unsafe Model Load(string path, string fullPath)
  {
    using Silk.NET.Assimp.Assimp assimp = Silk.NET.Assimp.Assimp.GetApi();

    string modelDir = Path.GetDirectoryName(fullPath) ?? "";

    Scene* scene = assimp.ImportFile(fullPath,
      (uint)(PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.GenerateNormals | PostProcessSteps.LimitBoneWeights));

    if (scene == null || scene->MFlags == (uint)SceneFlags.Incomplete || scene->MRootNode == null)
      throw new Exception($"Assimp failed to load '{path}': {assimp.GetErrorStringS()}");

    List<EngineMesh> meshes = [];
    Dictionary<uint, Texture2D> textureCache = [];
    Dictionary<string, int> boneNameToIndex = [];
    List<Matrix4x4> boneOffsets = [];

    ProcessNode(scene->MRootNode, scene, Matrix4x4.Identity, assimp, modelDir, textureCache, meshes, boneNameToIndex, boneOffsets);

    SkeletonNode skeleton = BuildSkeleton(scene->MRootNode);
    Dictionary<string, AnimationClip> animations = BuildAnimations(scene);

    assimp.ReleaseImport(scene);

    return new Model
    {
      Path = path,
      Meshes = meshes,
      Skeleton = skeleton,
      BoneNameToIndex = boneNameToIndex,
      BoneOffsets = [.. boneOffsets],
      Animations = animations,
    };
  }

  // Node.MTransformation is typed as System.Numerics.Matrix4x4 but its memory layout is
  // Assimp's row-major, column-vector convention (v' = M * v). Transposing it produces a
  // matrix that matches the row-vector, translation-last convention used everywhere else
  // in this engine's model matrices (v' = v * M, uploaded to the shader untransposed).
  static Matrix4x4 ToNumerics(Matrix4x4 m) => new(
    m.M11, m.M21, m.M31, m.M41,
    m.M12, m.M22, m.M32, m.M42,
    m.M13, m.M23, m.M33, m.M43,
    m.M14, m.M24, m.M34, m.M44
  );

  static unsafe void ProcessNode(Node* node, Scene* scene, Matrix4x4 parentTransform, Silk.NET.Assimp.Assimp assimp,
    string modelDir, Dictionary<uint, Texture2D> textureCache, List<EngineMesh> meshes,
    Dictionary<string, int> boneNameToIndex, List<Matrix4x4> boneOffsets)
  {
    Matrix4x4 globalTransform = ToNumerics(node->MTransformation) * parentTransform;

    for (uint i = 0; i < node->MNumMeshes; i++)
    {
      AssimpMesh* mesh = scene->MMeshes[node->MMeshes[i]];
      meshes.Add(ProcessMesh(mesh, scene, globalTransform, assimp, modelDir, textureCache, boneNameToIndex, boneOffsets));
    }

    for (uint i = 0; i < node->MNumChildren; i++)
      ProcessNode(node->MChildren[i], scene, globalTransform, assimp, modelDir, textureCache, meshes, boneNameToIndex, boneOffsets);
  }

  static unsafe EngineMesh ProcessMesh(AssimpMesh* mesh, Scene* scene, Matrix4x4 nodeTransform, Silk.NET.Assimp.Assimp assimp,
    string modelDir, Dictionary<uint, Texture2D> textureCache, Dictionary<string, int> boneNameToIndex, List<Matrix4x4> boneOffsets)
  {
    int vertexCount = (int)mesh->MNumVertices;

    // Up to MaxBoneWeightsPerVertex (bone index, weight) pairs per vertex; left at 0/0 for
    // unskinned vertices, which is harmless since the shader only applies skinning when the
    // mesh-level uSkinned uniform is set.
    float[] boneIds = new float[vertexCount * MaxBoneWeightsPerVertex];
    float[] boneWeights = new float[vertexCount * MaxBoneWeightsPerVertex];
    int[] boneSlotsUsed = new int[vertexCount];

    for (uint b = 0; b < mesh->MNumBones; b++)
    {
      Bone* bone = mesh->MBones[b];
      string boneName = bone->MName.AsString;

      if (!boneNameToIndex.TryGetValue(boneName, out int boneIndex))
      {
        boneIndex = boneOffsets.Count;
        boneNameToIndex[boneName] = boneIndex;
        boneOffsets.Add(ToNumerics(bone->MOffsetMatrix));
      }

      for (uint w = 0; w < bone->MNumWeights; w++)
      {
        VertexWeight weight = bone->MWeights[w];
        int vertexId = (int)weight.MVertexId;
        int slot = boneSlotsUsed[vertexId];
        if (slot >= MaxBoneWeightsPerVertex) continue;

        boneIds[vertexId * MaxBoneWeightsPerVertex + slot] = boneIndex;
        boneWeights[vertexId * MaxBoneWeightsPerVertex + slot] = weight.MWeight;
        boneSlotsUsed[vertexId]++;
      }
    }

    bool isSkinned = mesh->MNumBones > 0;

    // Interleaved position + texcoord + bone ids + bone weights, matching the vertex layout
    // every other 3D draw call in Graphics uses (plus the skinning attributes).
    float[] vertices = new float[vertexCount * VertexFloats];
    for (int i = 0; i < vertexCount; i++)
    {
      Vector3 position = mesh->MVertices[i];
      Vector2 texCoord = mesh->MTextureCoords[0] != null
        ? new Vector2(mesh->MTextureCoords[0][i].X, mesh->MTextureCoords[0][i].Y)
        : Vector2.Zero;

      int baseIndex = i * VertexFloats;
      vertices[baseIndex + 0] = position.X;
      vertices[baseIndex + 1] = position.Y;
      vertices[baseIndex + 2] = position.Z;
      vertices[baseIndex + 3] = texCoord.X;
      vertices[baseIndex + 4] = texCoord.Y;
      for (int k = 0; k < MaxBoneWeightsPerVertex; k++)
      {
        vertices[baseIndex + 5 + k] = boneIds[i * MaxBoneWeightsPerVertex + k];
        vertices[baseIndex + 5 + MaxBoneWeightsPerVertex + k] = boneWeights[i * MaxBoneWeightsPerVertex + k];
      }
    }

    List<uint> indices = [];
    for (uint i = 0; i < mesh->MNumFaces; i++)
    {
      Face face = mesh->MFaces[i];
      for (uint j = 0; j < face.MNumIndices; j++)
        indices.Add(face.MIndices[j]);
    }

    Texture2D? texture = mesh->MMaterialIndex < scene->MNumMaterials
      ? LoadMaterialTexture(scene->MMaterials[mesh->MMaterialIndex], scene, assimp, modelDir, textureCache)
      : null;

    uint vao = Engine.GL.GenVertexArray();
    Engine.GL.BindVertexArray(vao);

    uint vbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
    Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.StaticDraw);

    uint ebo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
    Engine.GL.BufferData(BufferTargetARB.ElementArrayBuffer, [.. indices], BufferUsageARB.StaticDraw);

    unsafe
    {
      uint stride = VertexFloats * sizeof(float);
      Engine.GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
      Engine.GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
      Engine.GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, (void*)(5 * sizeof(float)));
      Engine.GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, stride, (void*)(9 * sizeof(float)));
    }
    Engine.GL.EnableVertexAttribArray(0);
    Engine.GL.EnableVertexAttribArray(1);
    Engine.GL.EnableVertexAttribArray(2);
    Engine.GL.EnableVertexAttribArray(3);

    return new EngineMesh
    {
      Vao = vao,
      Vbo = vbo,
      Ebo = ebo,
      IndexCount = (uint)indices.Count,
      NodeTransform = nodeTransform,
      Texture = texture,
      IsSkinned = isSkinned,
    };
  }

  static unsafe SkeletonNode BuildSkeleton(Node* node)
  {
    SkeletonNode result = new()
    {
      Name = node->MName.AsString,
      LocalTransform = ToNumerics(node->MTransformation),
    };

    for (uint i = 0; i < node->MNumChildren; i++)
      result.Children.Add(BuildSkeleton(node->MChildren[i]));

    return result;
  }

  static unsafe Dictionary<string, AnimationClip> BuildAnimations(Scene* scene)
  {
    Dictionary<string, AnimationClip> animations = [];

    for (uint i = 0; i < scene->MNumAnimations; i++)
    {
      AssimpAnimation* anim = scene->MAnimations[i];
      double ticksPerSecond = anim->MTicksPerSecond != 0 ? anim->MTicksPerSecond : 25.0;

      AnimationClip clip = new()
      {
        Name = anim->MName.AsString,
        DurationTicks = (float)anim->MDuration,
        TicksPerSecond = (float)ticksPerSecond,
      };

      for (uint c = 0; c < anim->MNumChannels; c++)
      {
        NodeAnim* channel = anim->MChannels[c];
        AnimationChannel data = new();

        for (uint k = 0; k < channel->MNumPositionKeys; k++)
        {
          VectorKey key = channel->MPositionKeys[k];
          data.Positions.Add(((float)key.MTime, key.MValue));
        }
        for (uint k = 0; k < channel->MNumRotationKeys; k++)
        {
          QuatKey key = channel->MRotationKeys[k];
          data.Rotations.Add(((float)key.MTime, new Quaternion(key.MValue.X, key.MValue.Y, key.MValue.Z, key.MValue.W)));
        }
        for (uint k = 0; k < channel->MNumScalingKeys; k++)
        {
          VectorKey key = channel->MScalingKeys[k];
          data.Scales.Add(((float)key.MTime, key.MValue));
        }

        clip.Channels[channel->MNodeName.AsString] = data;
      }

      string name = string.IsNullOrEmpty(clip.Name) ? $"animation{i}" : clip.Name;
      animations[name] = clip;
    }

    return animations;
  }

  static unsafe Texture2D? LoadMaterialTexture(Material* material, Scene* scene, Silk.NET.Assimp.Assimp assimp,
    string modelDir, Dictionary<uint, Texture2D> textureCache)
  {
    if (assimp.GetMaterialTextureCount(material, TextureType.BaseColor) == 0 &&
        assimp.GetMaterialTextureCount(material, TextureType.Diffuse) == 0)
      return null;

    TextureType type = assimp.GetMaterialTextureCount(material, TextureType.BaseColor) > 0
      ? TextureType.BaseColor
      : TextureType.Diffuse;

    AssimpString path;
    assimp.GetMaterialTexture(material, type, 0, &path, null, null, null, null, null, null);
    string texturePath = path.AsString;

    if (texturePath.StartsWith('*'))
    {
      uint index = uint.Parse(texturePath.AsSpan(1));
      if (textureCache.TryGetValue(index, out Texture2D? cached)) return cached;

      AssimpTexture* embedded = scene->MTextures[index];
      // mHeight == 0 means the texture is still compressed (PNG/JPEG); mWidth is then the
      // raw byte size of that buffer, not a texel count.
      byte[] data = new byte[embedded->MWidth];
      new Span<byte>((byte*)embedded->PcData, (int)embedded->MWidth).CopyTo(data);

      Texture2D texture = TextureLoader.Upload(data);
      textureCache[index] = texture;
      return texture;
    }

    return TextureLoader.Upload(System.IO.File.ReadAllBytes(Path.Combine(modelDir, texturePath)));
  }
}
