using BepInEx;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GorillaShirts.Behaviours
{
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }

        private string dataLocation;

        private Dictionary<string, object> data = [];

        private JsonSerializerSettings serializeSettings, deserializeSettings;

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            var converter = new Vector3Converter();

            serializeSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                CheckAdditionalContent = true,
                Formatting = Formatting.Indented
            };
            serializeSettings.Converters.Add(converter);

            deserializeSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            deserializeSettings.Converters.Add(converter);

            dataLocation = Path.Combine(Application.persistentDataPath, $"{Constants.Name}.json");

            ReadData();
        }

        public void ReadData()
        {
            if (File.Exists(dataLocation))
            {
                data = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(dataLocation), deserializeSettings);
                data ??= [];
                return;
            }

            WriteData();
        }

        public void WriteData()
        {
            string output = JsonConvert.SerializeObject(data, serializeSettings);
            ThreadingHelper.Instance.StartAsyncInvoke(() =>
            {
                File.WriteAllText(dataLocation, output);
                return null;
            });
        }

        public T GetItem<T>(string key, T defaultValue = default)
        {
            if (data.TryGetValue(key, out object value) && value is T item)
            {
                return item;
            }

            //SetItem(key, defaultValue);
            return defaultValue;
        }

        public void SetItem(string key, object value)
        {
            if (data.ContainsKey(key)) data[key] = value;
            else data.Add(key, value);

            WriteData();
        }

        public void RemoveItem(string key)
        {
            if (data.ContainsKey(key))
            {
                data.Remove(key);
                WriteData();
            }
        }
    }
}
