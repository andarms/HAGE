namespace Hmz.Core._3D;

using System.Numerics;
using Hmz.Core;
using Hmz.Core._3D.Geometry;
using Hmz.Core.GOM;
using Hmz.Core.Renderer;
using Hmz.Core.Renderer.Styles;

public class BlobShadow : Component
{
  const float GroundOffset = 0.02f;

  readonly Quad quad = new();

  public Texture2D? Texture { get; set; }
  public Color Color { get; set; } = new(0, 0, 0, 140);
  public float Scale { get; set; } = 1f;

  public override void Draw()
  {
    Vector3 footprint = Owner.Collider?.Size ?? Vector3.One;
    quad.Size = new Vector2(footprint.X, footprint.Z) * Scale;

    Vector3 position = Owner.GlobalPosition;
    Matrix4x4 worldMatrix = Matrix4x4.CreateTranslation(position.X, position.Y + GroundOffset, position.Z);

    Engine.Graphics.DrawQuad(quad, worldMatrix, new QuadStyle { Texture = Texture, Color = Color });
  }
}
