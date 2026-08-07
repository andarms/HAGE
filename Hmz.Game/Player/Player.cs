using System.Numerics;
using Hmz.Core;
using Hmz.Core._3D;
using Hmz.Core._3D.Geometry;
using Hmz.Core.Collisions;
using Hmz.Core.GOM;
using Hmz.Core.States;

namespace Hmz.Game.Player;

public class Player : GameObject
{
  readonly PlayerContext context = new();

  public bool InputEnabled { get; set; } = true;

  public Player()
  {
    Transform.Position = new Vector3(0f, 0f, 0f);
    Collider = new(this)
    {
      Size = new Vector3(1f, 2f, 1f),
      Offset = new Vector3(0f, 1f, 0f),
      Layer = CollisionLayer.Player,
      Mask = CollisionLayer.All & ~CollisionLayer.Player,
    };

  }

  protected override void OnInitialize()
  {
    Model model = Engine.Content.LoadModel("models/player.gltf");
    ModelRenderer renderer = new(model);
    Movement movement = new(context);
    PlayerAnimator animator = new(context);
    StateMachine<PlayerContext> stateMachine = new(context);

    Components.Add(renderer);
    Components.Add(movement);
    Components.Add(animator);
    Components.Add(stateMachine);
    Components.Add(new BlobShadow { Texture = Engine.Content.LoadTexture("textures/blob_shadow.png") });

    stateMachine.Register(new States.PlayerIdleState());
    stateMachine.Register(new States.PlayerWalkingState());
    stateMachine.Start<States.PlayerIdleState>();

    Children.Add(new PlayerInteraction());
  }

  public override void HandleInput()
  {
    context.InputEnabled = InputEnabled;

    base.HandleInput();
  }
}


public class PlayerInteraction : GameObject
{
  const float ForwardOffset = 1f;

  public PlayerInteraction()
  {
    Transform.Position = Vector3.UnitZ * ForwardOffset;
    Collider = new(this)
    {
      Type = CollisionType.Trigger,
      Size = new Vector3(0.5f, 0.5f, 0.5f),
      Offset = new Vector3(0f, 0.5f, 0f),
      Layer = CollisionLayer.Player,
      Mask = CollisionLayer.All & ~CollisionLayer.Player,
      OnCollisionEnter = collision => Console.WriteLine($"[Trigger] {collision.Other.GetType().Name} entered"),
      OnCollisionExit = collision => Console.WriteLine($"[Trigger] {collision.Other.GetType().Name} exited"),
    };
  }

}