using System;
using System.IO;
using ESFA.DC.Serialization.Interfaces;
using Newtonsoft.Json;

namespace SFA.DAS.Payments.EarningEvents.Application.Infrastructure.Serialization
{
    public interface IFm36JsonSerializationService : IJsonSerializationService
    {
    }

    public class Fm36JsonSerializationService : IFm36JsonSerializationService
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public Fm36JsonSerializationService()
        {
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            _jsonSerializerSettings.Converters.Add(new DateOnlyJsonConverter());
        }

        public T Deserialize<T>(string serializedObject)
        {
            if (string.IsNullOrWhiteSpace(serializedObject))
            {
                throw new ArgumentNullException("Serialized Object string must not be null or whitespace.");
            }

            return JsonConvert.DeserializeObject<T>(serializedObject, _jsonSerializerSettings);
        }

        public T Deserialize<T>(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("Stream must be initialized.");
            }

            stream.Seek(0, SeekOrigin.Begin);

            using (var streamReader = new StreamReader(stream))
            {
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var serializer = new JsonSerializer
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    };
                    serializer.Converters.Add(new DateOnlyJsonConverter());
                    return serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        public string Serialize<T>(T objectToSerialize)
        {
            if (objectToSerialize == null)
            {
                throw new ArgumentNullException("Object To Serialize must not be Null.");
            }

            return JsonConvert.SerializeObject(objectToSerialize, _jsonSerializerSettings);
        }

        public void Serialize<T>(T objectToSerialize, Stream stream)
        {
            if (objectToSerialize == null)
            {
                throw new ArgumentNullException("Object To Serialize must not be Null.");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("Stream must be initialized.");
            }

            stream.Seek(0, SeekOrigin.Begin);

            var streamWriter = new StreamWriter(stream);

            var jsonTextWriter = new JsonTextWriter(streamWriter);

            new JsonSerializer().Serialize(jsonTextWriter, objectToSerialize);

            jsonTextWriter.Flush();
        }
    }
}
