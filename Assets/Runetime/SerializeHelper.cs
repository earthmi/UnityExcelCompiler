using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MiniGame.Support
{
    public class SerializeHelper
    {
        public static byte[] Serialize(object obj)
        {
            try
            {
                using MemoryStream stream = new();
                BinaryFormatter binaryFormatter = new();
                binaryFormatter.Serialize(stream, obj);
                return stream.GetBuffer();
            }
            catch (Exception e)
            {
                Debug.LogError($"序列化出现问题：{e}");
                return null;
            }
        }

        public static T Deserialize<T>(byte[] bytes) where T : class
        {
            using MemoryStream stream = new(bytes);
            BinaryFormatter binaryFormatter = new();
            return binaryFormatter.Deserialize(stream) as T;
        }
    }
}