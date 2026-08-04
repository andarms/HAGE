using System.Numerics;
using System.Reflection;
using Hmz.Core;
using Hmz.Core.GOM;
using Hmz.Core.Scenes;
using ImGuiNET;

namespace Hmz.Editor;

public class EditorScene : Scene
{
  GameObject? selected;

  public override void Draw()
  {
    base.Draw();

    Scene? inspected = Engine.Scenes.Below;

    DrawEntityList(inspected);
    DrawInspector();
  }

  static readonly Vector2 EntitiesMinSize = new(200f, 160f);
  static readonly Vector2 InspectorMinSize = new(320f, 240f);

  void DrawEntityList(Scene? inspected)
  {
    // Sane default placement/size on first run (before the user has ever moved or
    // resized it); a min-size clamp thereafter so a dragged-small window doesn't
    // start clipping labels/values.
    ImGui.SetNextWindowPos(new Vector2(10f, 40f), ImGuiCond.FirstUseEver);
    ImGui.SetNextWindowSize(EntitiesMinSize, ImGuiCond.FirstUseEver);
    ImGui.SetNextWindowSizeConstraints(EntitiesMinSize, new Vector2(float.MaxValue, float.MaxValue));

    ImGui.Begin("Entities");

    if (inspected == null)
    {
      ImGui.TextDisabled("No scene to inspect.");
      ImGui.End();
      return;
    }

    if (selected != null && !inspected.Instances.Contains(selected))
    {
      selected = null;
    }

    for (int i = 0; i < inspected.Instances.Count; i++)
    {
      GameObject entity = inspected.Instances[i];
      if (ImGui.Selectable($"{entity.GetType().Name}##entity{i}", entity == selected))
      {
        selected = entity;
      }
    }

    ImGui.End();
  }

  void DrawInspector()
  {
    ImGui.SetNextWindowPos(new Vector2(220f, 40f), ImGuiCond.FirstUseEver);
    ImGui.SetNextWindowSize(InspectorMinSize, ImGuiCond.FirstUseEver);
    ImGui.SetNextWindowSizeConstraints(InspectorMinSize, new Vector2(float.MaxValue, float.MaxValue));

    ImGui.Begin("Inspector");

    if (selected == null)
    {
      ImGui.TextDisabled("Select an entity to inspect.");
      ImGui.End();
      return;
    }

    ImGui.Text(selected.GetType().Name);
    ImGui.Separator();

    if (ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen))
    {
      DrawProperties(selected.Transform);
    }

    foreach (Component component in selected.Components.All)
    {
      if (ImGui.CollapsingHeader(component.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen))
      {
        DrawProperties(component);
      }
    }

    ImGui.End();
  }

  static void DrawProperties(object target)
  {
    foreach (PropertyInfo property in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
    {
      if (!property.CanRead || property.GetIndexParameters().Length > 0) continue;
      // Transform.EulerAngles is a convenience view over Rotation (see the Quaternion case
      // below); showing both would just be two widgets fighting over the same value.
      if (property.Name == "EulerAngles") continue;

      object? value = property.GetValue(target);

      if (!property.CanWrite)
      {
        ImGui.Text($"{property.Name}: {value}");
        continue;
      }

      switch (value)
      {
        case Vector3 v3:
          if (ImGui.DragFloat3(property.Name, ref v3, 0.1f)) property.SetValue(target, v3);
          break;
        case Quaternion q:
          Vector3 degrees = ToDegrees(q);
          if (ImGui.DragFloat3(property.Name, ref degrees, 1f)) property.SetValue(target, FromDegrees(degrees));
          break;
        case Vector2 v2:
          if (ImGui.DragFloat2(property.Name, ref v2, 0.1f)) property.SetValue(target, v2);
          break;
        case float f:
          if (ImGui.DragFloat(property.Name, ref f, 0.1f)) property.SetValue(target, f);
          break;
        case int i:
          if (ImGui.DragInt(property.Name, ref i)) property.SetValue(target, i);
          break;
        case bool b:
          if (ImGui.Checkbox(property.Name, ref b)) property.SetValue(target, b);
          break;
        case string s:
          if (ImGui.InputText(property.Name, ref s, 256)) property.SetValue(target, s);
          break;
        default:
          ImGui.Text($"{property.Name}: {value}");
          break;
      }
    }
  }

  // Quaternions edit far more intuitively as degrees than as raw x/y/z/w components.
  static Vector3 ToDegrees(Quaternion q)
  {
    float pitch = MathF.Asin(Math.Clamp(2f * (q.W * q.X - q.Z * q.Y), -1f, 1f));
    float yaw = MathF.Atan2(2f * (q.W * q.Y + q.Z * q.X), 1f - 2f * (q.X * q.X + q.Y * q.Y));
    float roll = MathF.Atan2(2f * (q.W * q.Z + q.X * q.Y), 1f - 2f * (q.X * q.X + q.Z * q.Z));
    return new Vector3(pitch, yaw, roll) * (180f / MathF.PI);
  }

  static Quaternion FromDegrees(Vector3 degrees)
  {
    Vector3 radians = degrees * (MathF.PI / 180f);
    return Quaternion.CreateFromYawPitchRoll(radians.Y, radians.X, radians.Z);
  }
}
