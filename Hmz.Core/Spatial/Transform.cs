namespace Hmz.Core.Spatial;

using System.Numerics;

public class Transform
{
  Vector3 position = Vector3.Zero;
  Quaternion rotation = Quaternion.Identity;
  Vector3 scale = Vector3.One;

  public Vector3 Position
  {
    get => position;
    set { position = value; Version++; }
  }

  // Source of truth for orientation, so it composes and interpolates (Slerp) correctly
  // with the quaternion-based animation system, without gimbal lock. See EulerAngles for
  // a yaw/pitch/roll view when that's the more natural way to reason about a rotation.
  public Quaternion Rotation
  {
    get => rotation;
    set { rotation = value; Version++; }
  }

  public Vector3 Scale
  {
    get => scale;
    set { scale = value; Version++; }
  }

  // Bumped whenever Position/Rotation/Scale changes. GameObject uses this to know when its
  // cached world matrix needs to be recomputed instead of doing it on every access.
  internal int Version { get; private set; }

  public Vector3 EulerAngles
  {
    get
    {
      float pitch = MathF.Asin(Math.Clamp(2f * (rotation.W * rotation.X - rotation.Z * rotation.Y), -1f, 1f));
      float yaw = MathF.Atan2(
        2f * (rotation.W * rotation.Y + rotation.Z * rotation.X),
        1f - 2f * (rotation.X * rotation.X + rotation.Y * rotation.Y));
      float roll = MathF.Atan2(
        2f * (rotation.W * rotation.Z + rotation.X * rotation.Y),
        1f - 2f * (rotation.X * rotation.X + rotation.Z * rotation.Z));
      return new Vector3(pitch, yaw, roll);
    }
    set => Rotation = Quaternion.CreateFromYawPitchRoll(value.Y, value.X, value.Z);
  }

  public static Transform FromMatrix(Matrix4x4 matrix)
  {
    if (!Matrix4x4.Decompose(matrix, out Vector3 scale, out Quaternion rotation, out Vector3 position))
    {
      throw new InvalidOperationException("The matrix cannot be decomposed into a transform.");
    }

    return new Transform
    {
      Position = position,
      Rotation = rotation,
      Scale = scale,
    };
  }

  public Matrix4x4 GetLocalMatrix()
  {
    return Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);
  }

}
