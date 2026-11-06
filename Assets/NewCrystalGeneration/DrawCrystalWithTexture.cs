using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct PolygonData
{
    public int startIndex;  // Индекс первой вершины в общем массиве vertices
    public int vertexCount; // Количество вершин в этом полигоне (например, 3 для треугольной грани)
}

[BurstCompile(CompileSynchronously = true)]
public struct DrawMultiplePolygonsJob : IJob
{
    public int width;
    public int height;

    // Массив пикселей текстуры, куда идет запись
    public NativeArray<Color32> pixels;

    // ПЛОСКИЙ массив, содержащий вершины ВСЕХ полигонов подряд
    [ReadOnly] public NativeArray<int2> allVertices;

    // Массив с метаданными о полигонах (откуда и сколько вершин брать)
    [ReadOnly] public NativeArray<PolygonData> polygons;

    // Массив цветов (размер должен совпадать с длиной массива polygons)
    [ReadOnly] public NativeArray<Color32> polygonColors;

    public void Execute()
    {
        // Проходим по каждому полигону из массива
        for (int p = 0; p < polygons.Length; p++)
        {
            PolygonData poly = polygons[p];
            Color32 color = polygonColors[p];

            if (poly.vertexCount < 3) continue;

            // 1. Находим Bounding Box по оси Y для текущего полигона
            int yMin = height;
            int yMax = 0;

            for (int i = 0; i < poly.vertexCount; i++)
            {
                int2 vertex = allVertices[poly.startIndex + i];
                if (vertex.y < yMin) yMin = vertex.y;
                if (vertex.y > yMax) yMax = vertex.y;
            }

            yMin = math.clamp(yMin, 0, height - 1);
            yMax = math.clamp(yMax, 0, height - 1);

            // Буфер для пересечений на стеке (выделяется один раз на полигон)
            NativeArray<int> xIntersections = new NativeArray<int>(16, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            // 2. Сканируем строки текущего полигона
            for (int y = yMin; y <= yMax; y++)
            {
                int intersectionCount = 0;

                // 3. Ищем пересечения со всеми ребрами текущего полигона
                for (int i = 0; i < poly.vertexCount; i++)
                {
                    // Извлекаем вершины с учетом смещения startIndex
                    int2 p1 = allVertices[poly.startIndex + i];
                    int2 p2 = allVertices[poly.startIndex + ((i + 1) % poly.vertexCount)];

                    if ((p1.y <= y && p2.y > y) || (p2.y <= y && p1.y > y))
                    {
                        if (p1.y != p2.y)
                        {
                            float x = p1.x + (float)(y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y);

                            if (intersectionCount < xIntersections.Length)
                            {
                                xIntersections[intersectionCount] = (int)math.round(x);
                                intersectionCount++;
                            }
                        }
                    }
                }

                // 4. Сортировка пузырьком пересечений X
                for (int i = 0; i < intersectionCount - 1; i++)
                {
                    for (int j = 0; j < intersectionCount - i - 1; j++)
                    {
                        if (xIntersections[j] > xIntersections[j + 1])
                        {
                            int temp = xIntersections[j];
                            xIntersections[j] = xIntersections[j + 1];
                            xIntersections[j + 1] = temp;
                        }
                    }
                }

                // 5. Закрашиваем пиксели строки в текстуре
                for (int i = 0; i < intersectionCount; i += 2)
                {
                    if (i + 1 >= intersectionCount) break;

                    int xStart = math.clamp(xIntersections[i], 0, width - 1);
                    int xEnd = math.clamp(xIntersections[i + 1], 0, width - 1);

                    for (int x = xStart; x <= xEnd; x++)
                    {
                        pixels[y * width + x] = color;
                    }
                }
            }

            // Освобождаем буфер строки
            xIntersections.Dispose();
        }
    }
}


public class DrawCrystalWithTexture : MonoBehaviour
{
    [SerializeField] protected Vector2Int _spriteResolution;
    [SerializeField] protected int _verticesCount;
    [SerializeField] protected int _polygonCount;
    [SerializeField] protected float _seed;

    [SerializeField] protected bool _generateOnValidate = false;

    private void Awake()
    {
        DrawCrystalWithFaces(_seed);
    }

    [ContextMenu("Генерировать")]
    private void DrawInEditor() 
    {
        DrawCrystalWithFaces(_seed);
    }

    private void OnValidate()
    {
        if (_generateOnValidate) DrawCrystalWithFaces(_seed);
    }

    protected virtual void InitCrystalConstants(ref float seed)
    {

    }

    void DrawCrystalWithFaces(float seed)
    {
        InitCrystalConstants(ref seed);

        int width = _spriteResolution.x;
        int height = _spriteResolution.y;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        NativeArray<Color32> pixelData = tex.GetRawTextureData<Color32>();

        // Очищаем текстуру в черный/прозрачный
        Color32 clearColor = new Color32(0, 0, 0, 0);
        for (int i = 0; i < pixelData.Length; i++) pixelData[i] = clearColor;

        // Допустим, мы рисуем кристалл из 3 треугольных граней, сходящихся в центре
        // Всего вершин: 3 грани * 3 вершины = 9 вершин
        NativeArray<int2> allVerts = new NativeArray<int2>(_verticesCount, Allocator.TempJob);
        NativeArray<PolygonData> polys = new NativeArray<PolygonData>(_polygonCount, Allocator.TempJob);
        NativeArray<Color32> colors = new NativeArray<Color32>(_polygonCount, Allocator.TempJob);

        // Центр кристалла, где сходятся все грани
        FillPolygonData(ref seed, allVerts, polys, colors);

        // Запуск единой Job задачи
        DrawMultiplePolygonsJob job = new DrawMultiplePolygonsJob
        {
            width = width,
            height = height,
            pixels = pixelData,
            allVertices = allVerts,
            polygons = polys,
            polygonColors = colors
        };

        JobHandle handle = job.Schedule();
        handle.Complete(); // Ждем выполнения

        // Обновляем текстуру
        tex.Apply();

        // Чистим память
        allVerts.Dispose();
        polys.Dispose();
        colors.Dispose();

        // Назначаем спрайт
        Sprite crystalSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        GetComponent<SpriteRenderer>().sprite = crystalSprite;
    }

    public static Color32 LerpColor32(Color32 c1, Color32 c2, float t)
    {
        // Ограничиваем шаг в пределах [0, 1]
        t = math.clamp(t, 0f, 1f);

        // Переводим float в целое число от 0 до 256 для быстрой целочисленной математики
        int tFixed = (int)(t * 256f);

        // Классическая формула линейной интерполяции: c1 + (c2 - c1) * t
        byte r = (byte)(c1.r + ((c2.r - c1.r) * tFixed >> 8));
        byte g = (byte)(c1.g + ((c2.g - c1.g) * tFixed >> 8));
        byte b = (byte)(c1.b + ((c2.b - c1.b) * tFixed >> 8));
        byte a = (byte)(c1.a + ((c2.a - c1.a) * tFixed >> 8));

        return new Color32(r, g, b, a);
    }

    public static float2 ProjectOnAxisX2D(float2 a, float2 d)
    {
        // Если d.y равно 0, проецирование невозможно (лучи параллельны оси OX)
        if (math.abs(d.y) < 1e-6f)
        {
            Debug.LogError("Направление проецирования параллельно оси абсцисс!");
            return float2.zero;
        }

        // Вычисляем координату X косой проекции
        float xProj = a.x - a.y * (d.x / d.y);

        return new float2(xProj, 0f);
    }

    public static float3 ProjectOnAxisX3D(float3 a, float3 d)
    {
        // Если d.y равно 0, проецирование невозможно (лучи параллельны оси OX)
        if (math.abs(d.y) < 1e-6f)
        {
            Debug.LogError("Направление проецирования параллельно оси абсцисс!");
            return float3.zero;
        }

        // Вычисляем координату X косой проекции
        float xProj = a.x - a.y * (d.x / d.y);

        return new float3(xProj, 0f, a.z);
    }

    public static float myLerp(float a, float b, float t) 
    {
        if (t < -1f || t > 1f) return 0f;
        return (1 + t) * b / 2f + a * (1 - t) / 2f;
    }

    protected virtual void FillPolygonData(ref float seed, NativeArray<int2> vertices, NativeArray<PolygonData> polygons, NativeArray<Color32> colors) { }
}