using System.Numerics;
using Hmz.Core;
using Hmz.Core._3D;
using Hmz.Core._3D.Geometry;
using Hmz.Core.Collisions;
using Hmz.Core.GOM;

namespace Hmz.Game;

public class Tree : GameObject
{
  public Tree()
  {
    Transform.Position = new Vector3(2f, 0f, 2f);
    Collider = new(this)
    {
      Size = new Vector3(1f, 2f, 1f),
      Offset = new Vector3(0f, 1f, 0f),
      Layer = CollisionLayer.Environment,
      Mask = CollisionLayer.All & ~CollisionLayer.Environment,
    };

  }

  protected override void OnInitialize()
  {
    Model model = Engine.Content.LoadModel("models/tree_1.gltf");
    ModelRenderer renderer = new(model);
    Components.Add(renderer);
    Components.Add(new BlobShadow { Texture = Engine.Content.LoadTexture("textures/blob_shadow.png") });
  }
}
