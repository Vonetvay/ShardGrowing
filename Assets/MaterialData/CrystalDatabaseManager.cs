/*using System.Collections.Generic;
using UnityEngine;

public class CrystalDatabaseManager : MonoBehaviour
{
    public static CrystalDatabaseManager Instance { get; private set; }

    [SerializeField] private CrystalDatabase database;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Инициализируем словарь для быстрого поиска
        if (database != null)
        {
            database.Initialize();
        }
    }

    // Удобный публичный метод для доступа из любой точки кода
    public CrystalPropertyData GetCrystal(string id)
    {
        if (database == null)
        {
            Debug.LogError("Ссылка на CrystalDatabase не установлена в CrystalDatabaseManager!");
            return default;
        }
        return database.GetCrystalData(id);
    }

    public Dictionary<MaterialType, CrystalPropertyData> GetDatabase() => database.GetDatabase();
}
*/