using Nez;
using SharpYaml.Serialization;
using System.IO;

namespace NewGame.Shared.Utilities
{
    public static class YamlSerializer
    {
        private static readonly string Extension = "yaml";

        public static T Deserialize<T>(string filename)
        {
            var deserializer = new Serializer();
            var path = $"{Core.content.RootDirectory}/{filename}.{Extension}";
            var obj = deserializer.Deserialize<T>(File.ReadAllText(path));
            return obj;
        }

    }
}
