namespace Hmz.Core._3D;

using System.Numerics;
using Hmz.Core;
using Hmz.Core._3D.Geometry;
using Hmz.Core.GOM;

public class ModelRenderer(Model model) : Component
{
  public Model Model { get; } = model;

  Matrix4x4[] boneMatrices = [];
  public IReadOnlyList<Matrix4x4> BoneMatrices => boneMatrices;

  string? currentAnimation;
  float animationTimeTicks;

  public void Play(string animationName)
  {
    if (currentAnimation == animationName || !Model.Animations.ContainsKey(animationName)) return;
    currentAnimation = animationName;
    animationTimeTicks = 0f;
  }

  public override void Update(float dt)
  {
    if (Model.Skeleton == null || Model.BoneOffsets.Length == 0) return;

    if (boneMatrices.Length != Model.BoneOffsets.Length)
    {
      boneMatrices = new Matrix4x4[Model.BoneOffsets.Length];
    }

    AnimationClip? clip = currentAnimation != null ? Model.Animations.GetValueOrDefault(currentAnimation) : null;

    if (clip != null)
    {
      animationTimeTicks += dt * clip.TicksPerSecond;
      if (clip.DurationTicks > 0)
      {
        animationTimeTicks %= clip.DurationTicks;
      }
    }

    ApplyPose(Model.Skeleton, Matrix4x4.Identity, clip);
  }

  public override void Draw()
  {
    Engine.Graphics.DrawModel(Model, Owner.WorldMatrix, boneMatrices);
  }

  void ApplyPose(SkeletonNode node, Matrix4x4 parentGlobal, AnimationClip? clip)
  {
    Matrix4x4 local = SampleLocalTransform(node, clip) ?? node.LocalTransform;
    Matrix4x4 global = local * parentGlobal;

    if (Model.BoneNameToIndex.TryGetValue(node.Name, out int boneIndex))
    {
      boneMatrices[boneIndex] = Model.BoneOffsets[boneIndex] * global;
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
