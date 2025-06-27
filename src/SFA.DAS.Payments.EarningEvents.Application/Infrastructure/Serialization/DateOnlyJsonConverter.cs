using System;
using Newtonsoft.Json;

namespace SFA.DAS.Payments.EarningEvents.Application.Infrastructure.Serialization
{
    public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly?>
    {
        public override void WriteJson(JsonWriter writer, DateOnly? dateOnlyValue, JsonSerializer serializer)
        {
            writer.WriteValue(dateOnlyValue.Value.ToString("O"));
        }

        public override DateOnly? ReadJson(JsonReader reader, Type objectType, DateOnly? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonToken.String)
            {
                var textValue = (string?)reader.Value;

                if (DateTime.TryParse(textValue, out var dateTimeValue))
                {
                    return DateOnly.FromDateTime(dateTimeValue);
                }

                if (DateOnly.TryParse(textValue, out var dateOnly))
                {
                    return dateOnly;
                }

                throw new JsonSerializationException($"Invalid date format: {textValue}");
            }

            if (reader.TokenType == JsonToken.Date)
            {
                var dateTimeValue = (DateTime?)reader.Value;
                if (dateTimeValue.HasValue)
                {
                    return DateOnly.FromDateTime(dateTimeValue.Value);
                }
                return null;
            }

            throw new JsonSerializationException($"Unexpected token type: {reader.TokenType}");
        }
    }
}
