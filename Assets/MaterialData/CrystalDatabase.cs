/*using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CrystalComponent
{
    public string materialId; // ID материала из базы материалов
    [Range(0f, 1f)] public float minShare;
    [Range(0f, 1f)] public float maxShare;
}

[System.Serializable]
public struct CrystalPropertyData
{
    public string crystalId;
    public GameObject crystalObject;
    public float quality; // Числовой параметр "качество"

    // Список компонентов (материалы и их доли), из которых состоит кристалл
    public List<CrystalComponent> composition;
}

[CreateAssetMenu(fileName = "CrystalDatabase", menuName = "Static Data/Crystal Database")]
public class CrystalDatabase : ScriptableObject
{
    [SerializeField] private List<CrystalPropertyData> crystals = new List<CrystalPropertyData>();

    // Быстрый поиск через Dictionary для оптимизации в рантайме
    private Dictionary<string, CrystalPropertyData> _crystalDict;

    public void Initialize()
    {
        _crystalDict = new Dictionary<string, CrystalPropertyData>();
        foreach (var crystal in crystals)
        {
            if (!string.IsNullOrEmpty(crystal.crystalId) && !_crystalDict.ContainsKey(crystal.crystalId))
            {
                _crystalDict.Add(crystal.crystalId, crystal);
            }
        }
    }

    public CrystalPropertyData GetCrystalData(string id)
    {
        if (_crystalDict == null) Initialize();

        if (_crystalDict.TryGetValue(id, out var data))
        {
            return data;
        }

        Debug.LogError($"Кристалл с ID '{id}' не найден в базе данных!");
        return default;
    }

    public Dictionary<string, CrystalPropertyData> GetDatabase() => _crystalDict;
}
*/