using Newtonsoft.Json;
using System.IO;
using TJson.NET.Contracts;
using TJson.NET.Helpers;

namespace TJson.NET
{
    public class Json
    {
        public static JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new TypeOnlyContractResolver()
        };

        /// <summary>
        /// Save <typeparamref name="T"/> to File
        /// <para>will automatically create a valid backup</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectBeingStored">The <typeparamref name="T"/> to Store</param>
        /// <param name="filePath">file path to save the <typeparamref name="T"/> as JSON</param>
        /// <param name="jsonSerializerSettings"><see cref="JsonSerializerSettings"/> to use; if you don't specify <see cref="SerializerSettings"/> will be used</param>
        /// <param name="backup">true if a .bak of the file should be created</param>
        /// <returns>true if the <typeparamref name="T"/> was written to file and the backup was successfully created (if true)</returns>
        public static bool Save<T>(T objectBeingStored, string filePath, JsonSerializerSettings? jsonSerializerSettings = null, bool backup = true)
        {
            try
            {
                if (objectBeingStored != null)
                {
                    jsonSerializerSettings ??= SerializerSettings;

                    File.WriteAllText(filePath, JsonConvert.SerializeObject(objectBeingStored, jsonSerializerSettings));

                    if (backup)
                    {
                        string CheckValidityOfBackup = File.ReadAllText(filePath).Normalize();
                        var b = JsonConvert.DeserializeObject<T>(CheckValidityOfBackup, jsonSerializerSettings);

                        if (b != null)
                        {
                            return Backup.SaveBackup(filePath);
                        }

                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Load <typeparamref name="T"/> from file, Repairing it if required
        /// <para>Will automatically attempt to restore a backup when required</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">file path to load the <typeparamref name="T"/> from</param>
        /// <param name="shouldBeOneDimensional">true if repair should be attempted, will remove { } from the entire string</param>
        /// <param name="jsonSerializerSettings"><see cref="JsonSerializerSettings"/> to use; if you don't specify <see cref="SerializerSettings"/> will be used</param>
        /// <param name="restore">true if restore from backup should be attempted</param>
        /// <returns>The deserialized <typeparamref name="T"/> or default(<typeparamref name="T"/>)</returns>
        public static T? Load<T>(string filePath, bool shouldBeOneDimensional = false, JsonSerializerSettings? jsonSerializerSettings = null, bool restore = true)
        {
            if (File.Exists(filePath))
            {
                bool attempt = false;
                jsonSerializerSettings ??= SerializerSettings;

                try
                {
                    return DeserializedObjectOrDefault<T>(shouldBeOneDimensional, filePath, jsonSerializerSettings);
                }
                catch
                {
                    if (restore)
                    {
                        attempt = Backup.RestoreBackup(filePath);
                    }
                }

                if (attempt && restore)
                {
                    try
                    {
                        return DeserializedObjectOrDefault<T>(shouldBeOneDimensional, filePath, jsonSerializerSettings);
                    }
                    catch
                    {
                        return default;
                    }
                }
            }

            return default;
        }

        public static T? DeserializedObjectOrDefault<T>(bool shouldBeOneDimensional, string filePath, JsonSerializerSettings? jsonSerializerSettings = null)
        {
            try
            {
                string serializedText = File.ReadAllText(filePath).Normalize();
                if (serializedText != null && serializedText != "")
                {
                    if (shouldBeOneDimensional && (serializedText.StartsWith("{\"List\":[") || serializedText.StartsWith("{[")))
                    {
                        serializedText = serializedText.Replace("{\"List\":[", "[").Replace("{[\"", "\"[").Replace("\"]}", "\"]").Replace("}", "").Replace("{", "");
                        File.WriteAllText(filePath, serializedText);
                    }

                    var deserializedObject = JsonConvert.DeserializeObject<T>(serializedText, jsonSerializerSettings ?? SerializerSettings);
                    if (deserializedObject != null)
                    {
                        return deserializedObject;
                    }
                }
            }
            catch
            {
                return default;
            }

            return default;
        }

    }
}
