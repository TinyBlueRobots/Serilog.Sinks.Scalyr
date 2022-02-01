using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Serilog.Sinks.Scalyr
{
  /// <inheritdoc />
  /// <summary>
  ///   Handles properly converting MethodBase objects to Json
  /// </summary>
  internal class MethodBaseConverter : JsonConverter<MethodBase>
  {
    /// <inheritdoc />
    public override MethodBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
      throw new NotSupportedException("Deserializing RuntimeMethodHandles is not allowed");

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, MethodBase value, JsonSerializerOptions options)
    {
      writer.WriteStartObject();

      WriteProperty("Name", value.Name);
      WriteProperty("DeclaringType", value.DeclaringType?.FullName);
      WriteProperty("Description", value.ToString());

      writer.WriteEndObject();

      void WriteProperty(string name, string propertyValue)
      {
        switch (options)
        {
          case { DefaultIgnoreCondition: JsonIgnoreCondition.WhenWritingNull }
            or { IgnoreNullValues: true }
            when propertyValue is null:
          case { DefaultIgnoreCondition: JsonIgnoreCondition.WhenWritingDefault }
            when propertyValue == default:
            return;
        }

        writer.WritePropertyName(name);
        writer.WriteStringValue(propertyValue);
      }
    }
  }
}