using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System;

public class Vector3Converter : JsonConverter<Vector3>
{
    // 将 Vector3 转换为简单的 JSON 对象
    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(value.x);
        writer.WritePropertyName("y");
        writer.WriteValue(value.y);
        writer.WritePropertyName("z");
        writer.WriteValue(value.z);
        writer.WriteEndObject();
    }

    // 从 JSON 对象还原回 Vector3
    public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        float x = jo["x"].Value<float>();
        float y = jo["y"].Value<float>();
        float z = jo["z"].Value<float>();
        return new Vector3(x, y, z);
    }
}