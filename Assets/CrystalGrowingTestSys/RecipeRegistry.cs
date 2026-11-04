using System.Collections.Generic;
using UnityEngine;

public class RecipeRegistry : MonoBehaviour
{
    public static RecipeRegistry Instance { get; private set; }

    // Сюда в инспекторе перетаскиваются ВСЕ созданные файлы рецептов
    [SerializeField] private List<GrowthRecipe> allRecipes;
    [SerializeField] private CrystalData _slag;

    private void Awake()
    {
        Instance = this;
    }

    // Главная функция матчинга
    public CrystalData EvaluateMix(Dictionary<MaterialType, float> mix, float temperature, ProcessType process)
    {
        foreach (var recipe in allRecipes)
        {
            if (recipe.Matches(mix, temperature, process))
            {
                return recipe.resultCrystal; // Нашли! Возвращаем данные кристалла
            }
        }

        return null; // Ничего не подошло, смесь испорчена или кристалл не вырос
    }

    public CrystalData GetSlag() => _slag;
}
