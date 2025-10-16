using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eshava.DomainDrivenDesign.Application.Converter
{
	public class BoolConverterTextJson : JsonConverter<bool>
	{
		public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return reader.TokenType switch
			{
				JsonTokenType.True => true,
				JsonTokenType.False => false,
				JsonTokenType.String => Boolean.TryParse(reader.GetString()?.ToLower(), out var boolvalue) ? boolvalue : throw new JsonException(),
				JsonTokenType.Number => reader.TryGetInt64(out var longValue)
					? Convert.ToBoolean(longValue) :
					reader.TryGetDouble(out var doubleValue)
						? Convert.ToBoolean(doubleValue)
						: false,
				_ => throw new JsonException(),
			};
		}

		public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
		{
			writer.WriteBooleanValue(value);
		}
	}
}