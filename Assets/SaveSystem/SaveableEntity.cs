using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SaveableEntity : MonoBehaviour
{
    [SerializeField] private string id = string.Empty;
    public string Id => id;

    [Serializable]
    private struct ComponentData
    {
        public string typeName;
        public string json;
    }

    [Serializable]
    private struct EntityData
    {
        public List<ComponentData> components;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Application.IsPlaying(gameObject)) return;
        if (string.IsNullOrEmpty(gameObject.scene.path)) return; // Не генерируем ID для префабов в ассетах

        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif

    public string CaptureState()
    {
        var data = new EntityData { components = new List<ComponentData>() };

        foreach (var saveable in GetComponents<ISaveable>())
        {
            data.components.Add(new ComponentData
            {
                typeName = saveable.GetType().AssemblyQualifiedName,
                json = saveable.CaptureState()
            });
        }

        return JsonUtility.ToJson(data);
    }

    public void RestoreState(string jsonState)
    {
        var data = JsonUtility.FromJson<EntityData>(jsonState);
        if (data.components == null) return;

        var saveables = GetComponents<ISaveable>();
        foreach (var compData in data.components)
        {
            foreach (var saveable in saveables)
            {
                if (saveable.GetType().AssemblyQualifiedName == compData.typeName)
                {
                    saveable.RestoreState(compData.json);
                    break;
                }
            }
        }
    }
}
