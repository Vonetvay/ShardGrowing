using System.Collections.Generic;
using UnityEngine;

// 1. Перечислители для гибкости

public enum ProcessType { Crystallization, Melting }

// 2. Класс-паспорт самого кристалла
[CreateAssetMenu(fileName = "NewCrystal", menuName = "Crystals/Crystal Data")]
public class CrystalData : ScriptableObject
{
    public string crystalName;
    public GameObject visualPrefab; // Как кристалл выглядит в мире
    public Shader shader;
    public float density;
    public int quality;
}

// 3. Класс рецепта
[CreateAssetMenu(fileName = "NewRecipe", menuName = "Crystals/Growth Recipe")]
public class GrowthRecipe : ScriptableObject
{
    [Header("Результат")]
    public CrystalData resultCrystal;

    [Header("Условия процесса")]
    public ProcessType processType = ProcessType.Crystallization;
    public float minTemperature;
    public float maxTemperature;

    [Header("Требуемые ингредиенты")]
    // В Unity словари не отображаются в инспекторе по умолчанию, 
    // поэтому используем список вспомогательных структур
    public List<IngredientRequirement> requiredMaterials;
    
    // Метод проверки: подходит ли смесь под этот рецепт?
    public bool Matches(Dictionary<MaterialType, float> currentIngredients, float temperature, ProcessType currentProcess)
    {
        // Проверяем тип прибора и температуру
        if (currentProcess != processType) return false;
        if (temperature < minTemperature || temperature > maxTemperature) return false;

        // Проверяем, хватает ли каждого нужного материала
        foreach (var req in requiredMaterials)
        {
            if (!currentIngredients.TryGetValue(req.material, out float share)) return false;
            if (share < req.minShare || share > req.maxShare) return false;
        }

        return true;
    }
}

[System.Serializable]
public struct IngredientRequirement
{
    public MaterialType material;
    public float minShare;
    public float maxShare;
}
