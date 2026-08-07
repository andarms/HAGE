namespace Hmz.Core._3D;

using System.Numerics;
using Hmz.Core;
using Hmz.Core._3D.Geometry;
using Hmz.Core.GOM;
using Hmz.Core.Renderer;

public class ModelRenderer(Model model) : Component
{
  public Model Model { get; } = model;

  public Color Color { get; set; } = Color.White;

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
    Engine.Graphics.DrawModel(Model, Owner.WorldMatrix, boneMatrices, Color);
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

    Vector3 position = MathHelper.SampleKeyframes(channel.Positions, animationTimeTicks, Vector3.Zero, Vector3.Lerp);
    Quaternion rotation = MathHelper.SampleKeyframes(channel.Rotations, animationTimeTicks, Quaternion.Identity, Quaternion.Slerp);
    Vector3 scale = MathHelper.SampleKeyframes(channel.Scales, animationTimeTicks, Vector3.One, Vector3.Lerp);

    return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
  }
}
