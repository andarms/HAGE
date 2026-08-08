using System.Numerics;
using Hmz.Core;
using Hmz.Core._3D;
using Hmz.Core._3D.Geometry;
using Hmz.Core.Collisions;
using Hmz.Core.GOM;
using Hmz.Core.Renderer;
using Hmz.Core.States;

namespace Hmz.Game.Player;

public class Player : GameObject
{
  readonly PlayerContext context = new();

  public bool InputEnabled { get; set; } = true;

  public Player()
  {
    Transform.Position = new Vector3(0f, 2.5f, 0f);
    Collider = new(this)
    {
      Size = new Vector3(1f, 1.2f, 1f),
      Offset = new Vector3(0f, 0.6f, 0f),
      Layer = CollisionLayer.Player,
      Mask = CollisionLayer.All & ~CollisionLayer.Player,
    };

  }

  protected override void OnInitialize()
  {
    Model model = Engine.Content.LoadModel("models/player.gltf");
    ModelRenderer renderer = new(model);
    CameraController cameraController = new(context);
    Movement movement = new(context);
    PlayerAnimator animator = new(context);
    StateMachine<PlayerContext> stateMachine = new(context);

    context.Movement = movement;

    Components.Add(renderer);
    Components.Add(cameraController);
    Components.Add(movement);
    Components.Add(animator);
    Components.Add(stateMachine);
    Components.Add(new BlobShadow { Texture = Engine.Content.LoadTexture("textures/blob_shadow.png") });

    stateMachine.Register(new States.PlayerIdleState());
    stateMachine.Register(new States.PlayerWalkingState());
    stateMachine.Register(new States.PlayerAttackState());
    stateMachine.Start<States.PlayerIdleState>();

    Children.Add(new PlayerInteraction());
  }

  public override void HandleInput()
  {
    context.InputEnabled = InputEnabled;
    context.AttackPressed = InputEnabled && Engine.Input.IsActionJustPressed("action_1");

    base.HandleInput();
  }

  public override void Debug()
  {
    base.Debug();
    Console.WriteLine($"Player Position: {Transform.Position}");
  }
}
