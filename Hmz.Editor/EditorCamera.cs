using System.Numerics;
using Hmz.Core;
using Hmz.Core._3D;

namespace Hmz.Editor;

public enum CameraPreset { Normal, Isometric, Top }

// Switchable camera angles, all driven through Camera3D.Follow - the same method
// GameplayScene uses to follow the player - with a different offset/projection/up per preset.
public sealed class EditorCamera
{
  const float FollowSpeed = 6f;

  static readonly Vector3 NormalOffset = new(0f, 10f, 12f);
  static readonly Vector3 IsometricOffset = new(10f, 10f, 10f);
  static readonly Vector3 TopOffset = new(0f, 20f, 0f);

  public CameraPreset Preset { get; private set; } = CameraPreset.Normal;

  Vector3 Offset => Preset switch
  {
    CameraPreset.Isometric => IsometricOffset,
    CameraPreset.Top => TopOffset,
    _ => NormalOffset,
  };

  public void Initialize(Camera3D camera)
  {
    camera.AspectRatio = Engine.Viewport.AspectRatio;
    camera.NearPlane = 0.1f;
    camera.FarPlane = 1000f;
    camera.FieldOfView = MathF.PI / 4f;
    Apply(camera);
  }

  public void Cycle(Camera3D camera)
  {
    Preset = (CameraPreset)(((int)Preset + 1) % 3);
    Apply(camera);
  }

  void Apply(Camera3D camera)
  {
    (camera.Projection, camera.Up) = Preset switch
    {
      // -UnitZ, not UnitZ: CreateLookAt is left-handed, and with the camera directly above
      // the ghost, Up = UnitZ maps world +X to screen-left and +Z to screen-up - both
      // inverted relative to the arrow keys (Right = +X, Up = -Z). Negating Up flips both
      // axes at once so the controls match what's on screen.
      CameraPreset.Top => (ProjectionMode.Orthographic, -Vector3.UnitZ),
      CameraPreset.Isometric => (ProjectionMode.Orthographic, Vector3.UnitY),
      _ => (ProjectionMode.Perspective, Vector3.UnitY),
    };
    camera.OrthographicSize = Preset == CameraPreset.Top ? 16f : 10f;
  }

  public void Follow(Camera3D camera, Vector3 target, float dt) => camera.Follow(target, Offset, FollowSpeed, dt);
}
