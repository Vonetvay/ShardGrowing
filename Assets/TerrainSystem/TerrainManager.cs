using System.Drawing;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public enum TerrainType 
{
    None,
    Ground,
    Field
}

public class TerrainManager : MonoBehaviour
{
    public Camera mainCamera;
    public Vector2Int textureSize;
    public int brushRadius = 5;

    [SerializeField] private Material material;

    private Texture2D maskTexture;
    private Color32[] texturePixels;
    [SerializeField] private MeshRenderer targetRenderer;

    [SerializeField] private string _textureNameOnDisk;

    public static TerrainManager Instance;

    public TerrainType ColorToTerrain(Color32 color) 
    {
        switch (color)
        {
            case { r: 0, g: 0, b: 0, a: 255 }:
                return TerrainType.Ground;
            case { r: 255, g: 255, b: 255, a: 255 }:
                return TerrainType.Field;
            default:
                return TerrainType.None;
        }
    }

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Создаем текстуру данных в памяти
        maskTexture = new Texture2D(textureSize.x, textureSize.y, TextureFormat.R8, false);
        maskTexture.filterMode = FilterMode.Point;

        Texture2D loadTexture = LoadTextureFromDisk(_textureNameOnDisk);

        if (loadTexture == null || loadTexture.GetPixels32().Length != textureSize.x * textureSize.y)
        {
            ResetTexture();
        }
        else 
        {
            texturePixels = loadTexture.GetPixels32();
            maskTexture.SetPixels32(texturePixels);
            maskTexture.Apply();
        }

        // Отдаем текстуру в материал объекта, чтобы другой шейдер мог её использовать
        //targetRenderer.material.mainTexture = maskTexture;
        targetRenderer.material = material;
        material.SetTexture("_Mask", maskTexture);
    }

    /*private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse.leftButton.isPressed)
        {
            Ray ray = mainCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == targetRenderer.gameObject)
            {
                int centerX = (int)(hit.textureCoord.x * textureSize.x);
                int centerY = (int)(hit.textureCoord.y * textureSize.y);

                DrawCircleCPU(centerX, centerY);
            }
        }
    }*/

    private void OnDestroy()
    {
        SaveTextureToDisk(maskTexture, _textureNameOnDisk);
    }

    public Color32 GetColorFromUV(Vector2 uv) 
    {
        (int, int) coord = UVToPixelCoord(uv);
        return GetColorFromCoord(coord.Item1, coord.Item2);
    }

    public Color32 GetColorFromCoord(int x, int y)
    {
        return texturePixels[y * textureSize.x + x];
    }

    public TerrainType GetTerrainFromUV(Vector2 uv) 
    {
        return ColorToTerrain(GetColorFromUV(uv));
    }

    public TerrainType GetTerrainFromCoord(int x, int y)
    {
        return ColorToTerrain(GetColorFromCoord(x, y));
    }

    public void DrawCircleCPU(int cx, int cy, float radius)
    {
        // Проходимся квадратом вокруг точки клика
        for (int y = cy - brushRadius; y <= cy + brushRadius; y++)
        {
            if (y < 0 || y >= textureSize.y) continue;

            for (int x = cx - brushRadius; x <= cx + brushRadius; x++)
            {
                if (x < 0 || x >= textureSize.x) continue;

                // Проверка на круглую форму (теорема Пифагора)
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius)
                {
                    int index = y * textureSize.x + x;
                    texturePixels[index] = new Color32(255, 255, 255, 255); // Записываем 255 (или 1) в R канал
                }
            }
        }

        // Синхронизируем изменения с GPU
        maskTexture.SetPixels32(texturePixels);
        maskTexture.Apply(false); // false отключает генерацию мип-мапов для скорости
    }

    [ContextMenu("Перезагрузить карту")]
    private void ResetTexture() 
    {
        texturePixels = new Color32[textureSize.x * textureSize.y];

        for (int i = 0; i < texturePixels.Length; i++) texturePixels[i] = new Color32(0, 0, 0, 255);

        maskTexture.SetPixels32(texturePixels);
        maskTexture.Apply();
    }

    public void SaveTextureToDisk(Texture2D texture, string fileName)
    {
        // 1. Конвертируем текстуру в массив байтов PNG
        byte[] bytes = texture.EncodeToPNG();

        // 2. Формируем полный путь (например, в папку PersistentDataPath)
        string filePath = Path.Combine(Application.persistentDataPath, fileName + ".png");

        // 3. Записываем байты на диск
        File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Текстура успешно сохранена по пути: {filePath}");
    }

    public Texture2D LoadTextureFromDisk(string fileName)
    {
        // 1. Формируем путь к файлу
        string filePath = Path.Combine(Application.persistentDataPath, fileName + ".png");
        Debug.Log(Application.persistentDataPath);

        // 2. Проверяем, существует ли файл
        if (File.Exists(filePath))
        {
            // 3. Читаем байты с диска
            byte[] fileData = File.ReadAllBytes(filePath);

            // 4. Создаем пустую текстуру (размер изменится автоматически при LoadImage)
            Texture2D texture = new Texture2D(2, 2, TextureFormat.R8, false);

            // 5. Загружаем данные в текстуру
            texture.LoadImage(fileData);

            return texture;
        }

        Debug.Log($"Файл не найден по пути: {filePath}");
        return null;
    }

    public void DeleteTextureFromDisk(string fileName)
    {
        // 1. Формируем точный путь к файлу
        string filePath = Path.Combine(Application.persistentDataPath, fileName + ".png");

        // 2. Проверяем, существует ли этот файл на диске
        if (File.Exists(filePath))
        {
            // 3. Удаляем файл с диска
            File.Delete(filePath);
            Debug.Log($"Файл успешно удален: {filePath}");
        }
        else
        {
            Debug.LogWarning($"Не удалось удалить. Файл не найден по пути: {filePath}");
        }
    }

    public (int, int) UVToPixelCoord(Vector2 uv) => ((int)(uv.x * textureSize.x), (int)(uv.y * textureSize.y));
}
