namespace Hmz.Core.Renderer._3D;

using System.Numerics;
using Hmz.Core;

public class Model : IDisposable
{
  public string Path { get; init; } = "";
  public Transform Transform { get; init; } = new Transform();
  public List<Mesh> Meshes { get; init; } = [];

  public SkeletonNode? Skeleton { get; init; }
  public Dictionary<string, int> BoneNameToIndex { get; init; } = [];
  public Matrix4x4[] BoneOffsets { get; init; } = [];
  public Dictionary<string, AnimationClip> Animations { get; init; } = [];

  public Matrix4x4[] BoneMatrices { get; private set; } = [];

  string? currentAnimation;
  float animationTimeTicks;

  public Matrix4x4 GetRenderMatrix()
  {
    Matrix4x4 translation = Matrix4x4.CreateTranslation(Transform.Position);
    Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(Transform.Rotation.Y, Transform.Rotation.X, Transform.Rotation.Z);
    Matrix4x4 scale = Matrix4x4.CreateScale(Transform.Scale);
    return scale * rotation * translation;
  }

  public void Dispose()
  {
    foreach (Mesh mesh in Meshes)
    {
      Engine.GL.DeleteVertexArray(mesh.Vao);
      Engine.GL.DeleteBuffer(mesh.Vbo);
      Engine.GL.DeleteBuffer(mesh.Ebo);
    }
  }

  public void Update(float deltaTime)
  {
    if (Skeleton == null || BoneOffsets.Length == 0) return;

    if (BoneMatrices.Length != BoneOffsets.Length)
    {
      BoneMatrices = new Matrix4x4[BoneOffsets.Length];
    }

    AnimationClip? clip = currentAnimation != null ? Animations.GetValueOrDefault(currentAnimation) : null;

    if (clip != null)
    {
      animationTimeTicks += deltaTime * clip.TicksPerSecond;
      if (clip.DurationTicks > 0)
      {
        animationTimeTicks %= clip.DurationTicks;
      }
    }

    ApplyPose(Skeleton, Matrix4x4.Identity, clip);
  }

  public void Play(string animationName)
  {
    if (currentAnimation == animationName || !Animations.ContainsKey(animationName)) return;
    currentAnimation = animationName;
    animationTimeTicks = 0f;
  }

  // Walks the node hierarchy, sampling the clip (falling back to each node's bind-pose local
  // transform when it has no channel), and bakes offset * globalTransform for every bone.
  void ApplyPose(SkeletonNode node, Matrix4x4 parentGlobal, AnimationClip? clip)
  {
    Matrix4x4 local = SampleLocalTransform(node, clip) ?? node.LocalTransform;
    Matrix4x4 global = local * parentGlobal;

    if (BoneNameToIndex.TryGetValue(node.Name, out int boneIndex))
    {
      BoneMatrices[boneIndex] = BoneOffsets[boneIndex] * global;
    }

    foreach (SkeletonNode child in node.Children)
    {
      ApplyPose(child, global, clip);
    }
  }

  Matrix4x4? SampleLocalTransform(SkeletonNode node, AnimationClip? clip)
  {
    if (clip == null || !clip.Channels.TryGetValue(node.Name, out AnimationChannel? channel)) return null;

    Vector3 position = SampleVector(channel.Positions, animationTimeTicks, Vector3.Zero);
    Quaternion rotation = SampleQuaternion(channel.Rotations, animationTimeTicks, Quaternion.Identity);
    Vector3 scale = SampleVector(channel.Scales, animationTimeTicks, Vector3.One);

    return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
  }

  static Vector3 SampleVector(List<(float Time, Vector3 Value)> keys, float time, Vector3 fallback)
  {
    if (keys.Count == 0) return fallback;
    if (keys.Count == 1 || time <= keys[0].Time) return keys[0].Value;
    if (time >= keys[^1].Time) return keys[^1].Value;

    for (int i = 0; i < keys.Count - 1; i++)
    {
      if (time <= keys[i + 1].Time)
      {
        float span = keys[i + 1].Time - keys[i].Time;
        float t = span > 0f ? (time - keys[i].Time) / span : 0f;
        return Vector3.Lerp(keys[i].Value, keys[i + 1].Value, t);
      }
    }
    return keys[^1].Value;
  }

  static Quaternion SampleQuaternion(List<(float Time, Quaternion Value)> keys, float time, Quaternion fallback)
  {
    if (keys.Count == 0) return fallback;
    if (keys.Count == 1 || time <= keys[0].Time) return keys[0].Value;
    if (time >= keys[^1].Time) return keys[^1].Value;

    for (int i = 0; i < keys.Count - 1; i++)
    {
      if (time <= keys[i + 1].Time)
      {
        float span = keys[i + 1].Time - keys[i].Time;
        float t = span > 0f ? (time - keys[i].Time) / span : 0f;
        return Quaternion.Slerp(keys[i].Value, keys[i + 1].Value, t);
      }
    }
    return keys[^1].Value;
  }
}
