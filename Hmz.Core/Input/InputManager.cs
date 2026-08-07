using System.Numerics;
using Silk.NET.Input;

using SilkKey = Silk.NET.Input.Key;
using SilkMouseButton = Silk.NET.Input.MouseButton;
using SilkButtonName = Silk.NET.Input.ButtonName;

namespace Hmz.Core.Input;

public enum Key
{
  Unknown,
  A, B, C, D, E, F, G, H, I, J, K, L, M,
  N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
  Num0, Num1, Num2, Num3, Num4,
  Num5, Num6, Num7, Num8, Num9,
  F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
  Escape,
  LeftControl,
  LeftShift,
  LeftAlt,
  LeftSuper,
  RightControl,
  RightShift,
  RightAlt,
  RightSuper,
  Menu,
  Space,
  Enter,
  Backspace,
  Tab,
  CapsLock,
  ScrollLock,
  Pause,
  Insert,
  Home,
  PageUp,
  Delete,
  End,
  PageDown,
  RightArrow,
  LeftArrow,
  DownArrow,
  UpArrow
}

public enum MouseButton
{
  Left,
  Right,
  Middle
}

public enum GamepadButton
{
  A,
  B,
  X,
  Y,
  LeftBumper,
  RightBumper,
  Back,
  Start,
  Guide,
  LeftThumbstick,
  RightThumbstick,
  DPadUp,
  DPadRight,
  DPadDown,
  DPadLeft
}

public enum GamepadAxis
{
  LeftStickX,
  LeftStickY,
  RightStickX,
  RightStickY,
  LeftTrigger,
  RightTrigger
}

public class InputAction
{
  public string Name { get; }
  public List<Key> Keys { get; } = new();
  public List<MouseButton> MouseButtons { get; } = new();
  public List<GamepadButton> GamepadButtons { get; } = new();

  public InputAction(string name)
  {
    Name = name;
  }
}

public class InputManager
{
  static readonly Dictionary<Key, SilkKey> KeyMap = new()
  {
    [Key.A] = SilkKey.A, [Key.B] = SilkKey.B, [Key.C] = SilkKey.C, [Key.D] = SilkKey.D,
    [Key.E] = SilkKey.E, [Key.F] = SilkKey.F, [Key.G] = SilkKey.G, [Key.H] = SilkKey.H,
    [Key.I] = SilkKey.I, [Key.J] = SilkKey.J, [Key.K] = SilkKey.K, [Key.L] = SilkKey.L,
    [Key.M] = SilkKey.M, [Key.N] = SilkKey.N, [Key.O] = SilkKey.O, [Key.P] = SilkKey.P,
    [Key.Q] = SilkKey.Q, [Key.R] = SilkKey.R, [Key.S] = SilkKey.S, [Key.T] = SilkKey.T,
    [Key.U] = SilkKey.U, [Key.V] = SilkKey.V, [Key.W] = SilkKey.W, [Key.X] = SilkKey.X,
    [Key.Y] = SilkKey.Y, [Key.Z] = SilkKey.Z,
    [Key.Num0] = SilkKey.Number0, [Key.Num1] = SilkKey.Number1, [Key.Num2] = SilkKey.Number2,
    [Key.Num3] = SilkKey.Number3, [Key.Num4] = SilkKey.Number4, [Key.Num5] = SilkKey.Number5,
    [Key.Num6] = SilkKey.Number6, [Key.Num7] = SilkKey.Number7, [Key.Num8] = SilkKey.Number8,
    [Key.Num9] = SilkKey.Number9,
    [Key.F1] = SilkKey.F1, [Key.F2] = SilkKey.F2, [Key.F3] = SilkKey.F3, [Key.F4] = SilkKey.F4,
    [Key.F5] = SilkKey.F5, [Key.F6] = SilkKey.F6, [Key.F7] = SilkKey.F7, [Key.F8] = SilkKey.F8,
    [Key.F9] = SilkKey.F9, [Key.F10] = SilkKey.F10, [Key.F11] = SilkKey.F11, [Key.F12] = SilkKey.F12,
    [Key.Escape] = SilkKey.Escape,
    [Key.LeftControl] = SilkKey.ControlLeft,
    [Key.LeftShift] = SilkKey.ShiftLeft,
    [Key.LeftAlt] = SilkKey.AltLeft,
    [Key.LeftSuper] = SilkKey.SuperLeft,
    [Key.RightControl] = SilkKey.ControlRight,
    [Key.RightShift] = SilkKey.ShiftRight,
    [Key.RightAlt] = SilkKey.AltRight,
    [Key.RightSuper] = SilkKey.SuperRight,
    [Key.Menu] = SilkKey.Menu,
    [Key.Space] = SilkKey.Space,
    [Key.Enter] = SilkKey.Enter,
    [Key.Backspace] = SilkKey.Backspace,
    [Key.Tab] = SilkKey.Tab,
    [Key.CapsLock] = SilkKey.CapsLock,
    [Key.ScrollLock] = SilkKey.ScrollLock,
    [Key.Pause] = SilkKey.Pause,
    [Key.Insert] = SilkKey.Insert,
    [Key.Home] = SilkKey.Home,
    [Key.PageUp] = SilkKey.PageUp,
    [Key.Delete] = SilkKey.Delete,
    [Key.End] = SilkKey.End,
    [Key.PageDown] = SilkKey.PageDown,
    [Key.RightArrow] = SilkKey.Right,
    [Key.LeftArrow] = SilkKey.Left,
    [Key.DownArrow] = SilkKey.Down,
    [Key.UpArrow] = SilkKey.Up,
  };

  static readonly Dictionary<SilkKey, Key> ReverseKeyMap =
    KeyMap.ToDictionary(kv => kv.Value, kv => kv.Key);

  static readonly Dictionary<MouseButton, SilkMouseButton> MouseButtonMap = new()
  {
    [MouseButton.Left] = SilkMouseButton.Left,
    [MouseButton.Right] = SilkMouseButton.Right,
    [MouseButton.Middle] = SilkMouseButton.Middle,
  };

  static readonly Dictionary<SilkMouseButton, MouseButton> ReverseMouseButtonMap =
    MouseButtonMap.ToDictionary(kv => kv.Value, kv => kv.Key);

  static readonly Dictionary<GamepadButton, SilkButtonName> GamepadButtonMap = new()
  {
    [GamepadButton.A] = SilkButtonName.A,
    [GamepadButton.B] = SilkButtonName.B,
    [GamepadButton.X] = SilkButtonName.X,
    [GamepadButton.Y] = SilkButtonName.Y,
    [GamepadButton.LeftBumper] = SilkButtonName.LeftBumper,
    [GamepadButton.RightBumper] = SilkButtonName.RightBumper,
    [GamepadButton.Back] = SilkButtonName.Back,
    [GamepadButton.Start] = SilkButtonName.Start,
    [GamepadButton.Guide] = SilkButtonName.Home,
    [GamepadButton.LeftThumbstick] = SilkButtonName.LeftStick,
    [GamepadButton.RightThumbstick] = SilkButtonName.RightStick,
    [GamepadButton.DPadUp] = SilkButtonName.DPadUp,
    [GamepadButton.DPadRight] = SilkButtonName.DPadRight,
    [GamepadButton.DPadDown] = SilkButtonName.DPadDown,
    [GamepadButton.DPadLeft] = SilkButtonName.DPadLeft,
  };

  static readonly Dictionary<SilkButtonName, GamepadButton> ReverseGamepadButtonMap =
    GamepadButtonMap.ToDictionary(kv => kv.Value, kv => kv.Key);

  static readonly HashSet<GamepadButton> EmptyGamepadButtons = new();

  IInputContext? context;
  bool initialized;
  readonly HashSet<IInputDevice> hookedDevices = new();

  readonly HashSet<Key> keysDown = new();
  readonly HashSet<Key> keysDownPrevious = new();

  readonly HashSet<MouseButton> mouseButtonsDown = new();
  readonly HashSet<MouseButton> mouseButtonsDownPrevious = new();

  readonly Dictionary<int, HashSet<GamepadButton>> gamepadButtonsDown = new();
  readonly Dictionary<int, HashSet<GamepadButton>> gamepadButtonsDownPrevious = new();

  readonly Dictionary<string, InputAction> actions = new();

  public Vector2 MousePosition { get; private set; }
  public Vector2 MouseDelta { get; private set; }
  public float MouseScrollDelta { get; private set; }

  public bool MouseCapturedByCanvas { get; private set; }

  public void CaptureMouse()
  {
    MouseCapturedByCanvas = true;
  }

  internal void ResetMouseCapture()
  {
    MouseCapturedByCanvas = false;
  }

  public void Initialize(IInputContext inputContext)
  {
    if (initialized)
    {
      throw new InvalidOperationException("InputManager is already initialized.");
    }
    initialized = true;

    context = inputContext;
    context.ConnectionChanged += OnConnectionChanged;

    foreach (var keyboard in context.Keyboards) HookDevice(keyboard);
    foreach (var mouse in context.Mice) HookDevice(mouse);
    foreach (var gamepad in context.Gamepads) HookDevice(gamepad);
  }

  void OnConnectionChanged(IInputDevice device, bool connected)
  {
    if (connected)
    {
      HookDevice(device);
    }
    else if (device is IGamepad gamepad)
    {
      gamepadButtonsDown.Remove(gamepad.Index);
      gamepadButtonsDownPrevious.Remove(gamepad.Index);
    }
  }

  void HookDevice(IInputDevice device)
  {
    if (!hookedDevices.Add(device)) return;

    switch (device)
    {
      case IKeyboard keyboard:
        keyboard.KeyDown += (_, key, _) => { if (ReverseKeyMap.TryGetValue(key, out var mapped)) keysDown.Add(mapped); };
        keyboard.KeyUp += (_, key, _) => { if (ReverseKeyMap.TryGetValue(key, out var mapped)) keysDown.Remove(mapped); };
        break;

      case IMouse mouse:
        mouse.MouseDown += (_, button) => { if (ReverseMouseButtonMap.TryGetValue(button, out var mapped)) mouseButtonsDown.Add(mapped); };
        mouse.MouseUp += (_, button) => { if (ReverseMouseButtonMap.TryGetValue(button, out var mapped)) mouseButtonsDown.Remove(mapped); };
        mouse.MouseMove += (_, position) =>
        {
          MouseDelta += position - MousePosition;
          MousePosition = position;
        };
        mouse.Scroll += (_, wheel) => MouseScrollDelta += wheel.Y;
        break;

      case IGamepad gamepad:
        gamepad.ButtonDown += (pad, button) => GetGamepadSet(gamepadButtonsDown, pad.Index).Add(MapGamepadButton(button.Name));
        gamepad.ButtonUp += (pad, button) => GetGamepadSet(gamepadButtonsDown, pad.Index).Remove(MapGamepadButton(button.Name));
        break;
    }
  }

  static GamepadButton MapGamepadButton(SilkButtonName name) =>
    ReverseGamepadButtonMap.TryGetValue(name, out var mapped) ? mapped : GamepadButton.A;

  static HashSet<GamepadButton> GetGamepadSet(Dictionary<int, HashSet<GamepadButton>> dict, int gamepadIndex)
  {
    if (!dict.TryGetValue(gamepadIndex, out var set))
    {
      set = new HashSet<GamepadButton>();
      dict[gamepadIndex] = set;
    }
    return set;
  }

  public void EndFrame()
  {
    keysDownPrevious.Clear();
    keysDownPrevious.UnionWith(keysDown);

    mouseButtonsDownPrevious.Clear();
    mouseButtonsDownPrevious.UnionWith(mouseButtonsDown);

    gamepadButtonsDownPrevious.Clear();
    foreach (var (index, set) in gamepadButtonsDown)
    {
      gamepadButtonsDownPrevious[index] = new HashSet<GamepadButton>(set);
    }

    MouseDelta = Vector2.Zero;
    MouseScrollDelta = 0f;
  }

  #region Raw key queries

  public bool IsKeyPressed(Key key) => keysDown.Contains(key);
  public bool IsKeyJustPressed(Key key) => keysDown.Contains(key) && !keysDownPrevious.Contains(key);
  public bool IsKeyJustReleased(Key key) => !keysDown.Contains(key) && keysDownPrevious.Contains(key);

  #endregion

  #region Raw mouse queries

  public bool IsMouseButtonPressed(MouseButton button) => mouseButtonsDown.Contains(button);
  public bool IsMouseButtonJustPressed(MouseButton button) => mouseButtonsDown.Contains(button) && !mouseButtonsDownPrevious.Contains(button);
  public bool IsMouseButtonJustReleased(MouseButton button) => !mouseButtonsDown.Contains(button) && mouseButtonsDownPrevious.Contains(button);

  #endregion

  #region Raw gamepad queries

  public bool IsGamepadButtonPressed(GamepadButton button, int gamepadIndex = 0) =>
    gamepadButtonsDown.GetValueOrDefault(gamepadIndex, EmptyGamepadButtons).Contains(button);

  public bool IsGamepadButtonJustPressed(GamepadButton button, int gamepadIndex = 0) =>
    IsGamepadButtonPressed(button, gamepadIndex) &&
    !gamepadButtonsDownPrevious.GetValueOrDefault(gamepadIndex, EmptyGamepadButtons).Contains(button);

  public bool IsGamepadButtonJustReleased(GamepadButton button, int gamepadIndex = 0) =>
    !IsGamepadButtonPressed(button, gamepadIndex) &&
    gamepadButtonsDownPrevious.GetValueOrDefault(gamepadIndex, EmptyGamepadButtons).Contains(button);

  public float GetGamepadAxis(GamepadAxis axis, int gamepadIndex = 0)
  {
    var pad = context?.Gamepads.FirstOrDefault(g => g.Index == gamepadIndex);
    if (pad is null) return 0f;

    return axis switch
    {
      GamepadAxis.LeftStickX => pad.Thumbsticks.Count > 0 ? pad.Thumbsticks[0].X : 0f,
      GamepadAxis.LeftStickY => pad.Thumbsticks.Count > 0 ? pad.Thumbsticks[0].Y : 0f,
      GamepadAxis.RightStickX => pad.Thumbsticks.Count > 1 ? pad.Thumbsticks[1].X : 0f,
      GamepadAxis.RightStickY => pad.Thumbsticks.Count > 1 ? pad.Thumbsticks[1].Y : 0f,
      GamepadAxis.LeftTrigger => pad.Triggers.Count > 0 ? pad.Triggers[0].Position : 0f,
      GamepadAxis.RightTrigger => pad.Triggers.Count > 1 ? pad.Triggers[1].Position : 0f,
      _ => 0f,
    };
  }

  #endregion

  #region Action map

  public InputAction AddAction(string name)
  {
    if (!actions.TryGetValue(name, out var action))
    {
      action = new InputAction(name);
      actions[name] = action;
    }
    return action;
  }

  public void RemoveAction(string name) => actions.Remove(name);

  public void AddBinding(string action, Key key) => AddAction(action).Keys.Add(key);
  public void AddBinding(string action, MouseButton button) => AddAction(action).MouseButtons.Add(button);
  public void AddBinding(string action, GamepadButton button) => AddAction(action).GamepadButtons.Add(button);

  public bool IsActionPressed(string action) =>
    actions.TryGetValue(action, out var a) && IsActionPressed(a, current: true);

  public bool IsActionJustPressed(string action) =>
    actions.TryGetValue(action, out var a) && IsActionPressed(a, current: true) && !IsActionPressed(a, current: false);

  public bool IsActionJustReleased(string action) =>
    actions.TryGetValue(action, out var a) && !IsActionPressed(a, current: true) && IsActionPressed(a, current: false);

  public float GetActionStrength(string action) => IsActionPressed(action) ? 1f : 0f;

  bool IsActionPressed(InputAction action, bool current)
  {
    var keys = current ? keysDown : keysDownPrevious;
    foreach (var key in action.Keys)
    {
      if (keys.Contains(key)) return true;
    }

    var mouseButtons = current ? mouseButtonsDown : mouseButtonsDownPrevious;
    foreach (var button in action.MouseButtons)
    {
      if (mouseButtons.Contains(button)) return true;
    }

    if (action.GamepadButtons.Count > 0)
    {
      var gamepadSets = current ? gamepadButtonsDown : gamepadButtonsDownPrevious;
      foreach (var set in gamepadSets.Values)
      {
        foreach (var button in action.GamepadButtons)
        {
          if (set.Contains(button)) return true;
        }
      }
    }

    return false;
  }

  #endregion
}
