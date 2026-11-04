using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TextureTest : MonoBehaviour
{
    public int textureWidth = 256;
    public int textureHeight = 256;

    public float growingDuration = 15f;
    private float currentDuration = 0f;

    private float height = 0f;
    private float width = 0f;
    private float a = 0f;

    private List<Vector3> points = new List<Vector3>();

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        //a = Random.Range(0f, Mathf.PI * 2f);
        for (int i = 0; i < Random.Range(8, 13); i++) 
        {
            points.Add(new Vector3(Random.Range(0f, Mathf.PI * 2f), Random.Range(1f, 3f), Random.Range(6f, 15f)));
        }
    }

    private void Update()
    {
        currentDuration += Time.deltaTime;
        if (currentDuration < growingDuration) 
        {
            
            UpdateTexture();
        }
    }

    private void UpdateTexture()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 1. Создаем текстуру с поддержкой альфа-канала (RGBA32)
        Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

        // Настройка фильтрации (полезно для пиксель-арта)
        texture.filterMode = FilterMode.Point;

        // 2. Создаем массив пикселей
        Color[] colors = new Color[textureWidth * textureHeight];

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                int index = y * textureWidth + x;

                /*if ((textureWidth / 2f - width <= x && x <= textureWidth / 2f + width) &&
                        (textureHeight / 2f - height <= y && y <= textureHeight / 2f + height))*/

                colors[index] = new Color(0f, 0f, 0f, 0f);
                foreach (var point in points) 
                {
                    if (IsPixelOnFatLine(x, y, textureWidth / 2f, textureHeight / 2f, textureWidth / 2f + currentDuration * point.z * Mathf.Cos(point.x), textureHeight / 2f + currentDuration * point.z * Mathf.Sin(point.x), currentDuration * point.y))
                    {
                        colors[index] = new Color(1f, 0f, 1f, Mathf.Clamp(0.5f + colors[index].a, 0f, 1f));
                    }
                }
            }
        }

        // 3. Загружаем пиксели в текстуру и применяем изменения
        texture.SetPixels(colors);
        texture.Apply();

        // 4. Создаем Sprite и передаем его в SpriteRenderer
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Sprite generatedSprite = Sprite.Create(texture, rect, pivot, 100f);

        spriteRenderer.sprite = generatedSprite;
    }

    void Start()
    {
        
    }

    public bool IsPixelOnFatLine(
    float px, float py,     // Координаты проверяемого пикселя
    float x0, float y0,     // Начало линии (осевая линия)
    float x1, float y1,     // Конец линии (осевая линия)
    float lineWidth)         // Полная толщина линии в пикселях
    {
        // Радиус проверки (расстояние от оси до края линии)
        float radius = lineWidth / 2f;

        // 1. Быстрая проверка по габаритам (Bounding Box) с учетом толщины
        if (px < Mathf.Min(x0, x1) - radius || px > Mathf.Max(x0, x1) + radius ||
            py < Mathf.Min(y0, y1) - radius || py > Mathf.Max(y0, y1) + radius)
        {
            return false;
        }

        // Длина отрезка в квадрате
        float lineLenSq = (x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0);

        // Если начальная и конечная точки совпадают, проверяем расстояние до точки
        if (lineLenSq == 0)
        {
            float distToPointSq = (px - x0) * (px - x0) + (py - y0) * (py - y0);
            return distToPointSq <= (radius * radius);
        }

        // 2. Находим проекцию точки на отрезок (параметр t от 0 до 1)
        float t = ((px - x0) * (x1 - x0) + (py - y0) * (y1 - y0)) / lineLenSq;
        t = Mathf.Max(0, Mathf.Min(1, t));

        // 3. Находим координаты ближайшей точки на оси отрезка
        float closestX = x0 + t * (x1 - x0);
        float closestY = y0 + t * (y1 - y0);

        // 4. Вычисляем КВАДРАТ расстояния от пикселя до ближайшей точки отрезка
        float distSq = (px - closestX) * (px - closestX) + (py - closestY) * (py - closestY);

        // Сравниваем квадрат расстояния с квадратом радиуса (избегаем Math.Sqrt)
        return distSq <= (radius * radius);
    }

}
