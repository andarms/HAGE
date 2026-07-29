using Hmz.Core.Renderer._2D;
using Hmz.Core.Renderer._3D;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using StbImageSharp;
using System.Numerics;
using AssimpMesh = Silk.NET.Assimp.Mesh;
using AssimpTexture = Silk.NET.Assimp.Texture;
using EngineMesh = Hmz.Core.Renderer._3D.Mesh;

namespace Hmz.Core.Content;

public sealed class ContentManager
{
  public string ContentRoot { get; init; } = "assets/";
  public string Resolve(string path) => Path.Combine(AppContext.BaseDirectory, ContentRoot, path);
  private readonly Dictionary<string, object> cache = [];

  // Decode the image on the CPU, upload it to OpenGL, and expose only the engine texture type.
  public Texture2D LoadTexture(string path) => UploadTexture(System.IO.File.ReadAllBytes(Resolve(path)));

  Texture2D UploadTexture(byte[] bytes)
  {
    ImageResult image = ImageResult.FromMemory(bytes, ColorComponents.RedGreenBlueAlpha);

    uint handle = Engine.GL.GenTexture();
    Engine.GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
    Engine.GL.BindTexture(TextureTarget.Texture2D, handle);
    Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
    Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
    Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
    Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
    unsafe
    {
      // The decoded pixels are needed only while OpenGL copies them into the texture.
      fixed (byte* ptr = image.Data)
        Engine.GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba,
          (uint)image.Width, (uint)image.Height, 0,
          PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
    }
    Engine.GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
    // Configure texture sampling and generate mipmaps before releasing the CPU image.

    return new Texture2D(handle, image.Width, image.Height);
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

  public unsafe Model LoadModel(string path)
  {
    using Silk.NET.Assimp.Assimp assimp = Silk.NET.Assimp.Assimp.GetApi();

    string fullPath = Resolve(path);
    string modelDir = Path.GetDirectoryName(fullPath) ?? "";

    Scene* scene = assimp.ImportFile(fullPath,
      (uint)(PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.GenerateNormals));

    if (scene == null || scene->MFlags == (uint)SceneFlags.Incomplete || scene->MRootNode == null)
      throw new Exception($"Assimp failed to load '{path}': {assimp.GetErrorStringS()}");

    List<EngineMesh> meshes = [];
    Dictionary<uint, Texture2D> textureCache = [];

    ProcessNode(scene->MRootNode, scene, Matrix4x4.Identity, assimp, modelDir, textureCache, meshes);

    assimp.ReleaseImport(scene);

    return new Model { Path = path, Meshes = meshes };
  }

  unsafe void ProcessNode(Node* node, Scene* scene, Matrix4x4 parentTransform, Silk.NET.Assimp.Assimp assimp,
    string modelDir, Dictionary<uint, Texture2D> textureCache, List<EngineMesh> meshes)
  {
    Matrix4x4 globalTransform = ToNumerics(node->MTransformation) * parentTransform;

    for (uint i = 0; i < node->MNumMeshes; i++)
    {
      AssimpMesh* mesh = scene->MMeshes[node->MMeshes[i]];
      meshes.Add(ProcessMesh(mesh, scene, globalTransform, assimp, modelDir, textureCache));
    }

    for (uint i = 0; i < node->MNumChildren; i++)
      ProcessNode(node->MChildren[i], scene, globalTransform, assimp, modelDir, textureCache, meshes);
  }

  unsafe EngineMesh ProcessMesh(AssimpMesh* mesh, Scene* scene, Matrix4x4 nodeTransform, Silk.NET.Assimp.Assimp assimp,
    string modelDir, Dictionary<uint, Texture2D> textureCache)
  {
    // Interleaved position + texcoord, matching the vertex layout every other draw call in Graphics uses.
    float[] vertices = new float[mesh->MNumVertices * 5];
    for (uint i = 0; i < mesh->MNumVertices; i++)
    {
      Vector3 position = mesh->MVertices[i];
      Vector2 texCoord = mesh->MTextureCoords[0] != null
        ? new Vector2(mesh->MTextureCoords[0][i].X, mesh->MTextureCoords[0][i].Y)
        : Vector2.Zero;

      vertices[i * 5 + 0] = position.X;
      vertices[i * 5 + 1] = position.Y;
      vertices[i * 5 + 2] = position.Z;
      vertices[i * 5 + 3] = texCoord.X;
      vertices[i * 5 + 4] = texCoord.Y;
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
      Engine.GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);
      Engine.GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
    }
    Engine.GL.EnableVertexAttribArray(0);
    Engine.GL.EnableVertexAttribArray(1);

    return new EngineMesh
    {
      Vao = vao,
      Vbo = vbo,
      Ebo = ebo,
      IndexCount = (uint)indices.Count,
      NodeTransform = nodeTransform,
      Texture = texture,
    };
  }

  unsafe Texture2D? LoadMaterialTexture(Material* material, Scene* scene, Silk.NET.Assimp.Assimp assimp,
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

      Texture2D texture = UploadTexture(data);
      textureCache[index] = texture;
      return texture;
    }

    return UploadTexture(System.IO.File.ReadAllBytes(Path.Combine(modelDir, texturePath)));
  }

  public Shader LoadShader(string path)
  {
    throw new NotImplementedException("Shader loading is not implemented yet.");
  }

  public bool TryGet<T>(string path, out T? asset) where T : class
  {
    if (cache.TryGetValue(path, out var obj) && obj is T typedAsset)
    {
      asset = typedAsset;
      return true;
    }
    asset = null;
    return false;
  }

  public void Unload(string path)
  {
    if (cache.TryGetValue(path, out var asset))
    {
      if (asset is IDisposable disposable)
      {
        disposable.Dispose();
      }
      cache.Remove(path);
    }
  }

  public void UnloadAll()
  {
    foreach (var asset in cache.Values)
    {
      if (asset is IDisposable disposable)
      {
        disposable.Dispose();
      }
    }
    cache.Clear();
  }
}