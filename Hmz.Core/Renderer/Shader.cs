using Silk.NET.OpenGL;
using System.Numerics;

namespace Hmz.Core.Renderer;

public sealed class Shader : IDisposable
{
  public uint Handle { get; }

  public Shader(string vertexSrc, string fragmentSrc)
  {
    uint vert = Compile(ShaderType.VertexShader, vertexSrc);
    uint frag = Compile(ShaderType.FragmentShader, fragmentSrc);

    Handle = Engine.GL.CreateProgram();
    Engine.GL.AttachShader(Handle, vert);
    Engine.GL.AttachShader(Handle, frag);
    Engine.GL.LinkProgram(Handle);
    Engine.GL.GetProgram(Handle, ProgramPropertyARB.LinkStatus, out int ok);
    if (ok == 0) throw new Exception(Engine.GL.GetProgramInfoLog(Handle));

    Engine.GL.DetachShader(Handle, vert);
    Engine.GL.DetachShader(Handle, frag);
    Engine.GL.DeleteShader(vert);
    Engine.GL.DeleteShader(frag);
  }

  static uint Compile(ShaderType type, string src)
  {
    uint s = Engine.GL.CreateShader(type);
    Engine.GL.ShaderSource(s, src);
    Engine.GL.CompileShader(s);
    Engine.GL.GetShader(s, ShaderParameterName.CompileStatus, out int ok);
    if (ok == 0) throw new Exception($"{type}: {Engine.GL.GetShaderInfoLog(s)}");
    return s;
  }

  public void Use()
  {
    Engine.GL.UseProgram(Handle);
  }

  public unsafe void SetMatrix(string name, Matrix4x4 m)
  {
    int loc = Engine.GL.GetUniformLocation(Handle, name);
    Engine.GL.UniformMatrix4(loc, 1, false, (float*)&m);
  }

  public unsafe void SetMatrixArray(string name, ReadOnlySpan<Matrix4x4> matrices)
  {
    if (matrices.Length == 0) return;
    int loc = Engine.GL.GetUniformLocation(Handle, name);
    fixed (Matrix4x4* ptr = matrices)
    {
      Engine.GL.UniformMatrix4(loc, (uint)matrices.Length, false, (float*)ptr);
    }
  }

  public void SetColor(string name, Color c)
  {
    Vector4 color = new(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
    Engine.GL.Uniform4(Engine.GL.GetUniformLocation(Handle, name), color);
  }

  public void SetInt(string name, int value)
  {
    Engine.GL.Uniform1(Engine.GL.GetUniformLocation(Handle, name), value);
  }

  public void SetBool(string name, bool value)
  {
    SetInt(name, value ? 1 : 0);
  }

  public void Dispose()
  {
    Engine.GL.DeleteProgram(Handle);
  }
}