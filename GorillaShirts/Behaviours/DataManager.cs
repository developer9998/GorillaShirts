using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
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

            var vector3Converter = new Vector3Converter();
            var versionConverter = new VersionConverter();

            serializeSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                CheckAdditionalContent = true,
                Formatting = Formatting.Indented
            };
            serializeSettings.Converters.Add(vector3Converter);
            serializeSettings.Converters.Add(versionConverter);

            deserializeSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            deserializeSettings.Converters.Add(vector3Converter);
            deserializeSettings.Converters.Add(versionConverter);

            dataLocation = Path.Combine(Application.persistentDataPath, $"{Constants.Name}.json");

            ReadData();
        }

        public void ReadData()
        {
            if (File.Exists(dataLocation))
            {
                try
                {
                    data = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(dataLocation), deserializeSettings) ?? [];
                }
                catch
                {
                    data = [];
                }
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

        public bool HasItem(string key) => data.ContainsKey(key);

        public T GetItem<T>(string key, T defaultValue = default, bool setDefaultValue = true)
        {
            if (data.TryGetValue(key, out object value))
            {
                if (value is T item) return item;

                TypeCode typeCode = Type.GetTypeCode(typeof(T));

                if (typeCode != TypeCode.Int64 && value is long deserializedLong)
                {
                    switch (typeCode)
                    {
                        case TypeCode.Int16:
                            short int16 = Convert.ToInt16(deserializedLong);
                            data[key] = int16;
                            return (T)(object)int16;
                        case TypeCode.Int32:
                            int int32 = Convert.ToInt32(deserializedLong);
                            data[key] = int32;
                            return (T)(object)int32;
                    }
                }
            }

            if (setDefaultValue) SetItem(key, defaultValue);
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
            if (HasItem(key))
            {
                data.Remove(key);
                WriteData();
            }
        }
    }
}
