using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hmz.Core.Tilemap;

public class Vector3JsonConverter : JsonConverter<Vector3>
{
  public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    if (reader.TokenType != JsonTokenType.StartArray)
    {
      throw new JsonException("Expected a 3-element array for a Vector3.");
    }

    Span<float> values = stackalloc float[3];
    int count = 0;
    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
    {
      if (count >= 3)
      {
        throw new JsonException("Vector3 array must have exactly 3 elements.");
      }
      values[count++] = reader.GetSingle();
    }
    if (count != 3)
    {
      throw new JsonException("Vector3 array must have exactly 3 elements.");
    }

    return new Vector3(values[0], values[1], values[2]);
  }

  public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
  {
    writer.WriteStartArray();
    writer.WriteNumberValue(value.X);
    writer.WriteNumberValue(value.Y);
    writer.WriteNumberValue(value.Z);
    writer.WriteEndArray();
  }
}
