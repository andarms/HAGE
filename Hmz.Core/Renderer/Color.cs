namespace Hmz.Core.Renderer;

public class Color(byte r, byte g, byte b, byte a = 255)
{
  public byte R { get; set; } = r;
  public byte G { get; set; } = g;
  public byte B { get; set; } = b;
  public byte A { get; set; } = a;

  public static Color Transparent => new(0, 0, 0, 0);
  public static Color Black => new(0, 0, 0);
  public static Color White => new(255, 255, 255);
  public static Color Red => new(255, 0, 0);
  public static Color Green => new(0, 255, 0);
  public static Color Blue => new(0, 0, 255);
  public static Color Yellow => new(255, 255, 0);
  public static Color Cyan => new(0, 255, 255);
  public static Color Magenta => new(255, 0, 255);
  public static Color Gray => new(128, 128, 128);
  public static Color LightGray => new(200, 200, 200);
  public static Color DarkGray => new(64, 64, 64);
  public static Color CornflowerBlue => new(100, 149, 237);
  public static Color Orange => new(255, 165, 0);
  public static Color Purple => new(128, 0, 128);
}