namespace PRTGInsight.Services
{
    public interface ISerializationService
    {
        string Serialize<T>(T obj);
        T Deserialize<T>(string json) where T : class;
    }
}