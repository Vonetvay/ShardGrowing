using UnityEngine;

public class CreateSquareTexture : MonoBehaviour
{
    [Header("Настройки текстуры")]
    public int textureSize = 256; // Размер стороны квадрата в пикселях
    public Color squareColor = Color.green; // Цвет квадрата

    private void Start()
    {
        // 1. Создаем пустую текстуру (квадратную)
        Texture2D texture = new Texture2D(textureSize, textureSize);

        // 2. Создаем массив пикселей нужного размера
        Color[] pixels = new Color[textureSize * textureSize];

        // 3. Заполняем массив выбранным цветом
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = squareColor;
        }

        // 4. Применяем массив пикселей к текстуре
        texture.SetPixels(pixels);
        texture.Apply(); // Переносим изменения в память видеокарты

        // 5. Назначаем текстуру на объект (например, на Renderer текущего объекта)
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.mainTexture = texture;
        }
        else
        {
            Debug.LogWarning("Компонент Renderer не найден на объекте. Текстура создана, но не отображена.");
        }
    }

    public Texture2D CreateSquareTextureFast(int size, Color32 color)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        // Родной массив Unity работает на уровне движка гораздо быстрее обычного цикла
        var pixels = new Color32[size * size];
        System.Array.Fill(pixels, color); // Быстрое заполнение массива в .NET

        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }

}
