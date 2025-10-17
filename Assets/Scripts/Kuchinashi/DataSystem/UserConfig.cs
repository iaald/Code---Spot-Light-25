using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Kuchinashi.DataSystem
{
    public class UserConfig
    {
        public static string Path { get => System.IO.Path.Combine(Application.persistentDataPath, "config"); }
        public static Dictionary<string, string> Instance
        {
            get => _instance ??= DeSerialization();
            private set => _instance = value;
        }
        private static Dictionary<string, string> _instance;

        public static Dictionary<string, string> DeSerialization() => ReadableData.DeSerialize<Dictionary<string, string>>(Path);
        public static void Serialization() => WriteableData.Serialize(Path, Instance);

        public static Dictionary<string, string> GetConfig()
        {
            return Instance;
        }

        public static T Read<T>(string id)
        {
            if (!Instance.TryGetValue(id, out var value)) return default;
            return typeof(T).Name switch
            {
                "String" => (T)Convert.ChangeType(value, typeof(T)),
                "Int32" => (T)Convert.ChangeType(int.TryParse(value, out var res) ? res : 0, typeof(T)),
                "Single" => (T)Convert.ChangeType(float.TryParse(value, out var res) ? res : 0f, typeof(T)),
                "Double" => (T)Convert.ChangeType(double.TryParse(value, out var res) ? res : 0d, typeof(T)),
                "Decimal" => (T)Convert.ChangeType(decimal.TryParse(value, out var res) ? res : 0m, typeof(T)),
                "Boolean" => (T)Convert.ChangeType(bool.TryParse(value, out var res) ? res : false, typeof(T)),
                _ => default(T),
            };
        }

        public static T ReadWithDefaultValue<T>(string id, T defaultValue)
        {
            if (!Instance.TryGetValue(id, out var value)) return defaultValue;
            return typeof(T).Name switch
            {
                "String" => (T)Convert.ChangeType(value, typeof(T)),
                "Int32" => (T)Convert.ChangeType(int.TryParse(value, out var res) ? res : defaultValue, typeof(T)),
                "Single" => (T)Convert.ChangeType(float.TryParse(value, out var res) ? res : (float)Convert.ChangeType(defaultValue, typeof(float)), typeof(T)),
                "Double" => (T)Convert.ChangeType(double.TryParse(value, out var res) ? res : (double)Convert.ChangeType(defaultValue, typeof(double)), typeof(T)),
                "Decimal" => (T)Convert.ChangeType(decimal.TryParse(value, out var res) ? res : (decimal)Convert.ChangeType(defaultValue, typeof(decimal)), typeof(T)),
                "Boolean" => (T)Convert.ChangeType(bool.TryParse(value, out var res) ? res : defaultValue, typeof(T)),
                _ => defaultValue,
            };
        }

        public static bool TryRead<T>(string id, out T data)
        {
            data = default;
            if (!Instance.TryGetValue(id, out var value)) return false;
            
            try
            {
                data = typeof(T).Name switch
                {
                    "String" => (T)Convert.ChangeType(value, typeof(T)),
                    "Int32" => (T)Convert.ChangeType(int.TryParse(value, out var res) ? res : 0, typeof(T)),
                    "Single" => (T)Convert.ChangeType(float.TryParse(value, out var res) ? res : 0f, typeof(T)),
                    "Double" => (T)Convert.ChangeType(double.TryParse(value, out var res) ? res : 0d, typeof(T)),
                    "Decimal" => (T)Convert.ChangeType(decimal.TryParse(value, out var res) ? res : 0m, typeof(T)),
                    "Boolean" => (T)Convert.ChangeType(bool.TryParse(value, out var res) ? res : false, typeof(T)),
                    _ => default(T),
                };
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Write<T>(string id, T data)
        {
            if (Instance.ContainsKey(id)) Instance[id] = data.ToString();
            else Instance.Add(id, data.ToString());

            Serialization();
        }

        public static bool Contains(string id) => Instance.ContainsKey(id);

        public static void Clear()
        {
            _instance = new();
            Serialization();
        }
    }
}