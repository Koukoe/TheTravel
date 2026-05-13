using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityDatabase", menuName = "EntityDatabase")]
public class EntityDatabase : ScriptableObject
{
    [Serializable]
    public struct EntityTemplate
    {
        public string guid;
        public string defaultName;
        public GameObject prefab;

        public Vector3 defaultPosition;
        public Vector3 defaultRotation;
    }

    public List<EntityTemplate> allEntities;

    private Dictionary<string, EntityTemplate> _fastLookup;

    private void BuildCache()
    {
        if (_fastLookup != null) return;
        _fastLookup = new Dictionary<string, EntityTemplate>();
        foreach (var entry in allEntities)
        {
            if (!string.IsNullOrEmpty(entry.guid)) _fastLookup[entry.guid] = entry;
        }
    }

    public EntityTemplate GetEntity(string guid)
    {
        BuildCache();
        if (_fastLookup.TryGetValue(guid, out var entry)) return entry;

        Debug.LogError($"实体库中找不到 GUID 为 {guid} 的资源配置");
        return default;
    }
}