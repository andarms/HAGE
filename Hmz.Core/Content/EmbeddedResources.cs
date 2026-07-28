namespace Hmz.Core.Content;

using System.Reflection;

internal static class EmbeddedResources
{
  static readonly Assembly Asm = typeof(EmbeddedResources).Assembly;

  public static string ReadText(string name)
  {
    using Stream s = Open(name);
    using var reader = new StreamReader(s);
    return reader.ReadToEnd();
  }

  public static byte[] ReadBytes(string name)
  {
    using Stream s = Open(name);
    using var ms = new MemoryStream();
    s.CopyTo(ms);
    return ms.ToArray();
  }

  static Stream Open(string name)
  {
    return Asm.GetManifestResourceStream(name)
         ?? throw new InvalidOperationException(
              $"Embedded resource '{name}' not found. Check the name is " +
              $"'{{RootNamespace}}.{{folder.path}}.{{file}}' and the file is marked EmbeddedResource.");
  }
}