using YamlDotNet.Serialization;

namespace CommandForgeGeneratorUtil
{
    public static class Yaml 
    {
        public static string ToJson(string yaml)
        {
            var yamlDeserializer = new Deserializer();
            var yamlObject = yamlDeserializer.Deserialize(yaml);
            var serializer = new SerializerBuilder().JsonCompatible().Build();
        
            return serializer.Serialize(yamlObject).Trim();
        }
    }
}

