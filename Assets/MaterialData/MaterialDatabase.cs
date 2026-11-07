using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum MaterialType { Impurities, Blue, Red }

[System.Serializable]
public struct MaterialPropertyData
{
    public MaterialType type;
    public string materialName;
    public Color color;
    public float density;
}

public struct PowderComposition
{
    private Dictionary<MaterialType, float> _powders;
    private float _totalAmount;

    private void RedefineTotalAmount() 
    {
        _totalAmount = 0f;
        foreach (var item in _powders)
        {
            _totalAmount += item.Value;
        }
    }

    public PowderComposition(Dictionary<MaterialType, float> powders)
    {
        _powders = powders != null
            ? new Dictionary<MaterialType, float>(powders)
            : throw new ArgumentNullException(nameof(powders));
        _totalAmount = _powders.Values.Sum();
    }

    public Dictionary<MaterialType, float> GetComponents()
    {
        var result = new Dictionary<MaterialType, float>(_powders.Count);
        if (_totalAmount <= 0) return result;

        foreach (var kvp in _powders)
        {
            result.Add(kvp.Key, kvp.Value / _totalAmount);
        }
        return result;
    }

    public float GetTotalAmount() => _totalAmount;

    public PowderComposition Add(PowderComposition other)
    {
        var newPowders = new Dictionary<MaterialType, float>(_powders);

        foreach (var kvp in other._powders)
        {
            if (newPowders.TryGetValue(kvp.Key, out float currentAmount))
            {
                newPowders[kvp.Key] = currentAmount + kvp.Value;
            }
            else
            {
                newPowders.Add(kvp.Key, kvp.Value);
            }
        }

        return new PowderComposition(newPowders);
    }

    public PowderComposition Subtract(float amountToRemove)
    {
        if (amountToRemove < 0)
            throw new ArgumentException("Количество для удаления не может быть отрицательным.", nameof(amountToRemove));

        if (amountToRemove >= _totalAmount)
            return new PowderComposition(new Dictionary<MaterialType, float>());

        float remainingRatio = (_totalAmount - amountToRemove) / _totalAmount;
        var remainingPowders = new Dictionary<MaterialType, float>(_powders.Count);
        float subtractRatio = amountToRemove / _totalAmount;
        var subtractPowders = new Dictionary<MaterialType, float>(_powders.Count);

        foreach (var kvp in _powders)
        {
            //newPowders.Add(kvp.Key, kvp.Value * remainingRatio);
            subtractPowders.Add(kvp.Key, kvp.Value * subtractRatio);
            remainingPowders.Add(kvp.Key, kvp.Value * remainingRatio);
        }
        _powders = remainingPowders;
        RedefineTotalAmount();

        return new PowderComposition(subtractPowders);
    }
}

[CreateAssetMenu(fileName = "MaterialDatabase", menuName = "Static Data/Material Database")]
public class MaterialDatabase : ScriptableObject
{
    [SerializeField] private List<MaterialPropertyData> materials = new List<MaterialPropertyData>();

    // Быстрый поиск через Dictionary для оптимизации в рантайме
    private Dictionary<MaterialType, MaterialPropertyData> _materialDict;

    public void Initialize()
    {
        _materialDict = new Dictionary<MaterialType, MaterialPropertyData>();
        foreach (var mat in materials)
        {
            _materialDict.Add(mat.type, mat);
        }
    }

    public MaterialPropertyData GetMaterialData(MaterialType type)
    {
        if (_materialDict == null) Initialize();

        if (_materialDict.TryGetValue(type, out var data))
        {
            return data;
        }

        Debug.LogError($"Материал '{type.ToString()}' не найден в базе данных!");
        return default;
    }
}