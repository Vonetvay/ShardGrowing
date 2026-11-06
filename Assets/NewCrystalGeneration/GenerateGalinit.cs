using System;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class GenerateGalinit : DrawCrystalWithTexture
{
    [Header("Настройки кристалла")]
    [SerializeField] private Vector2 _widthRatioRange;
    [SerializeField] private AnimationCurve _heightCurve;
    [SerializeField] private float _height;
    [SerializeField] private Vector2 _xOffsetRange;
    [SerializeField] private Vector2 _yOffsetRange;
    [SerializeField] private Vector2 _slopeRatioRange;
    [SerializeField] private Vector2 _peakRatioRange;
    [SerializeField] private Vector2Int _countRange;
    [SerializeField] private Vector2 _delayOffsetRange;
    [SerializeField][Range(0f, Mathf.PI * 2)] private float _growAngle;
    [SerializeField] private Color32 _backgroundColor;
    [SerializeField] private Color32 _frontColor;
    [SerializeField] private float k1;

    protected override void InitCrystalConstants(ref float seed)
    {
        base.InitCrystalConstants(ref seed);
        _polygonCount = SeedOperations.RandomNumberFromSeed(ref seed, _countRange);
        _verticesCount = _polygonCount * 5;
    }

    protected override void FillPolygonData(ref float seed, NativeArray<int2> vertices, NativeArray<PolygonData> polygons, NativeArray<Color32> colors)
    {
        base.FillPolygonData(ref seed,vertices, polygons, colors);

        float[] heights = new float[_polygonCount];

        for (int i = 0; i < _polygonCount; i++)
        {
            heights[i] = _heightCurve.Evaluate(SeedOperations.RandomNumberFromSeed(ref seed, 0f, 1f));
        }

        Array.Sort(heights);
        float maxHeight = heights[_polygonCount - 1];
        int2 centre = new int2(_spriteResolution.x / 2, _spriteResolution.y / 2);

        Vector2 directionRange = new Vector2((Mathf.PI - _growAngle) / 2f, (Mathf.PI + _growAngle) / 2f);

        for (int i = 0; i < _polygonCount; i++)
        {
            float height = _height * heights[_polygonCount - i - 1];
            float width = height * SeedOperations.RandomNumberFromSeed(ref seed, _widthRatioRange);
            float direction = SeedOperations.RandomNumberFromSeed(ref seed, directionRange);
            /*float direction = myLerp(directionRange.x, directionRange.y, 
                math.pow(SeedOperations.RandomNumberFromSeed(ref seed, -1f, 1f), heights[_polygonCount - i - 1] / maxHeight * k1));*/
            float2 offset = new float2(
                SeedOperations.RandomNumberFromSeed(ref seed, _xOffsetRange),
                SeedOperations.RandomNumberFromSeed(ref seed, _yOffsetRange)
                );
            float slopeRatio = SeedOperations.RandomNumberFromSeed(ref seed, _slopeRatioRange);
            float peakRatio = SeedOperations.RandomNumberFromSeed(ref seed, _peakRatioRange);

            float2x2 scaleM = float2x2.Scale(width, height);
            float2x2 rotM = float2x2.Rotate(direction - math.PIHALF);
            float2x2 transM = math.mul(rotM, scaleM);

            float2[] points = new float2[5]
            {
            new float2(-0.5f, 0f),  // левый нижний угол
            new float2(-0.5f + slopeRatio / 2f, 1f - peakRatio),  // левый верхний угол
            new float2(0f, 1f),  // вершина
            new float2(0.5f - slopeRatio / 2f, 1f - peakRatio),  // правый верхний угол
            new float2(0.5f, 0f),  // правый нижний угол
            };

            float2 d = new float2(math.cos(direction), math.sin(direction));

            vertices[0 + i * 5] = (int2)(ProjectOnAxisX2D(math.mul(transM, points[0]) + offset, d)) + centre;
            vertices[4 + i * 5] = (int2)(ProjectOnAxisX2D(math.mul(transM, points[4]) + offset, d)) + centre;

            vertices[i * 5 + 1] = (int2)(math.mul(transM, points[1]) + offset) + centre;
            vertices[i * 5 + 2] = (int2)(math.mul(transM, points[2]) + offset) + centre;
            vertices[i * 5 + 3] = (int2)(math.mul(transM, points[3]) + offset) + centre;

            polygons[i] = new PolygonData { startIndex = i * 5, vertexCount = 5 };
            colors[i] = LerpColor32(_backgroundColor, _frontColor, (float)i / _polygonCount);
        }
    }
}
