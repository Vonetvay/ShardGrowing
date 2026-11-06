using UnityEngine;

[ExecuteInEditMode] // Чтобы работало прямо в редакторе Unity
public class VoronoiBridge : MonoBehaviour
{
    [Header("Compute Settings")]
    public ComputeShader voronoiCompute;
    public int resolution = 512;

    [Header("Voronoi Parameters")]
    public float cellDensity = 10f;
    [Range(0f, 5f)] public float angleOffset = 1f;
    [Range(0f, 0.2f)] public float edgeThickness = 0.02f;
    [Range(0.001f, 0.1f)] public float edgeSmoothness = 0.01f;
    [ColorUsage(true, true)] public Color edgeColor = new Color(0f, 1f, 1f, 1f);
    public Color cellColor = new Color(0.1f, 0.1f, 0.15f, 1f);

    private RenderTexture _renderTexture;
    private int _kernelHandle;

    void OnEnable()
    {
        UpdateTexture();
    }

    void Update()
    {
        // Для демонстрации обновляем каждый кадр. 
        // В продакшене лучше вызывать только при изменении параметров!
        UpdateTexture();
    }

    void UpdateTexture()
    {
        if (voronoiCompute == null) return;

        // Создаем или пересоздаем текстуру при изменении разрешения
        if (_renderTexture == null || _renderTexture.width != resolution)
        {
            if (_renderTexture != null) _renderTexture.Release();

            _renderTexture = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf);
            _renderTexture.enableRandomWrite = true;
            _renderTexture.wrapMode = TextureWrapMode.Repeat; // Важно для бесшовности
            _renderTexture.Create();

            _kernelHandle = voronoiCompute.FindKernel("CSMain");
        }

        // Передаем параметры в Compute Shader
        voronoiCompute.SetInt("_Resolution", resolution);
        voronoiCompute.SetFloat("_CellDensity", cellDensity);
        voronoiCompute.SetFloat("_AngleOffset", angleOffset);
        voronoiCompute.SetFloat("_EdgeThickness", edgeThickness);
        voronoiCompute.SetFloat("_EdgeSmoothness", edgeSmoothness);
        voronoiCompute.SetVector("_EdgeColor", edgeColor);
        voronoiCompute.SetVector("_CellColor", cellColor);

        voronoiCompute.SetTexture(_kernelHandle, "Result", _renderTexture);

        // Рассчитываем количество групп потоков (с округлением вверх)
        int threadGroupsX = Mathf.CeilToInt(resolution / 8f);
        int threadGroupsY = Mathf.CeilToInt(resolution / 8f);

        voronoiCompute.Dispatch(_kernelHandle, threadGroupsX, threadGroupsY, 1);

        // Делаем текстуру глобальной. Теперь любой шейдер в Unity может прочитать её
        Shader.SetGlobalTexture("_GlobalVoronoiTex", _renderTexture);
    }

    void OnDisable()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            _renderTexture = null;
        }
    }
}
