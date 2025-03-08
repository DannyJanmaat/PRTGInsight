using Newtonsoft.Json;
using System;

namespace PRTGInsight.Services
{
    public class SerializationService : ISerializationService
    {
        private readonly JsonSerializerSettings _settings;

        public SerializationService()
        {
            _settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public string Serialize<T>(T obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, _settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error serializing object: {ex.Message}");
                return string.Empty;
            }
        }

        public T Deserialize<T>(string json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<T>(json, _settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deserializing JSON: {ex.Message}");
                return null;
            }
        }
    }
}