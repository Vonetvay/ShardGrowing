using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    public static MaterialManager Instance { get; private set; }

    [SerializeField] private MaterialDatabase database;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // »нициализируем словарь дл€ быстрого поиска
        if (database != null)
        {
            database.Initialize();
        }
    }

    // ”добный публичный метод дл€ доступа из любой точки кода
    public MaterialPropertyData GetMaterial(MaterialType type)
    {
        return database.GetMaterialData(type);
    }
}
