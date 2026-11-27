using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    public static SaveManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    [Serializable]
    private struct SaveRecord
    {
        public string id;
        public string json;
    }

    [Serializable]
    private struct SaveFile
    {
        public List<SaveRecord> records;
    }

    public void SaveGame()
    {
        var saveFile = new SaveFile { records = new List<SaveRecord>() };

        foreach (var entity in FindObjectsByType<SaveableEntity>(FindObjectsSortMode.None))
        {
            saveFile.records.Add(new SaveRecord
            {
                id = entity.Id,
                json = entity.CaptureState()
            });
        }

        string json = JsonUtility.ToJson(saveFile, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Игра сохранена: {SavePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("Файл сохранения не найден.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        var saveFile = JsonUtility.FromJson<SaveFile>(json);

        var lookup = new Dictionary<string, string>();
        foreach (var record in saveFile.records)
        {
            lookup[record.id] = record.json;
        }

        foreach (var entity in FindObjectsByType<SaveableEntity>(FindObjectsSortMode.None))
        {
            if (lookup.TryGetValue(entity.Id, out string entityJson))
            {
                entity.RestoreState(entityJson);
            }
        }

        Physics.SyncTransforms();
        Debug.Log("Игра загружена.");
    }

    public void ClearSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Сохранение удалено.");
        }
        else
        {
            Debug.LogWarning("Файл сохранения не найден для удаления.");
        }
    }

}
