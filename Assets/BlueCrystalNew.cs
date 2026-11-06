using System;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public record BlueShardParams(float3 Position, float4x4 BaseMatrix, float ZOffset, float Delay, float Duration, 
    float Width, float Height, float Direction, float SlopeRatio, float PeakRatio)
    : ShardParams(Position, BaseMatrix, ZOffset, Delay, Duration);

public class BlueCrystalNew : CrystalNew
{
    [Header("Настройки кристалла")]
    [SerializeField] private Vector2 _widthRatioRange;
    [SerializeField] private AnimationCurve _heightCurve;
    [SerializeField] private Vector2 _xOffsetRange;
    [SerializeField] private Vector2 _yOffsetRange;
    [SerializeField] private Vector2 _slopeRatioRange;
    [SerializeField] private Vector2 _peakRatioRange;
    [SerializeField] private Vector2Int _countRange;
    [SerializeField] private Vector2 _delayOffsetRange;
    [SerializeField][Range(0f, Mathf.PI * 2)] private float _growAngle;
    private bool _sortBySize = true;

    protected override ShardParams[] CrystalCalculator(ref float seed)
    {
        int count = SeedOperations.RandomNumberFromSeed(ref seed, _countRange);

        BlueShardParams[] shards = new BlueShardParams[count];

        float[] heights = new float[count];

        for (int i = 0; i < count; i++) 
        {
            heights[i] = _heightCurve.Evaluate(SeedOperations.RandomNumberFromSeed(ref seed, 0f, 1f));
        }

        Array.Sort(heights);
        float maxHeight = heights[count - 1];

        for (int i = 0; i < count; i++)
        {
            float height = heights[i];
            float width = height * SeedOperations.RandomNumberFromSeed(ref seed, _widthRatioRange);
            float direction = SeedOperations.RandomNumberFromSeed(ref seed, (Mathf.PI - _growAngle) / 2f, (Mathf.PI + _growAngle) / 2f);
            float3 offset = new float3(
                SeedOperations.RandomNumberFromSeed(ref seed, _xOffsetRange),
                SeedOperations.RandomNumberFromSeed(ref seed, _yOffsetRange),
                0f
                );
            float slopeRatio = SeedOperations.RandomNumberFromSeed(ref seed, _slopeRatioRange);
            float peakRatio = SeedOperations.RandomNumberFromSeed(ref seed, _peakRatioRange);
            float zOffset = count > 1 ? i / (float)(count - 1) : 0f;

            float delayOffset = SeedOperations.RandomNumberFromSeed(ref seed, _delayOffsetRange);
            float delay = math.saturate(1f - height / maxHeight + delayOffset);

            shards[i] = new BlueShardParams(offset, float4x4.identity, zOffset, delay, 1f - delay,
                width, height, direction, slopeRatio, peakRatio);
        }

        //if (_sortBySize) Array.Sort(shards, (a, b) => (a.Height).CompareTo(b.Height));

        return shards;
    }

    protected override void ShardCalculator(int index, int vertexCount, NativeSlice<float3> vertices, NativeSlice<int> triangles, NativeSlice<float2> uvs, ShardParams parameters)
    {
        if (parameters is BlueShardParams p) { }
        else return;

        float3x3 rotM = float3x3.RotateZ(p.Direction - math.PIHALF);
        float3x3 scaleM = float3x3.Scale(new float3(p.Width, p.Height, 0f));

        float3x3 transM = math.mul(rotM, scaleM);

        vertices[0] = math.transform(p.BaseMatrix, math.mul(transM, new float3(-0.5f, 0f, 0f) + p.Position)) + new float3(0f, 0f, p.ZOffset);
        vertices[1] = math.transform(p.BaseMatrix, math.mul(transM, new float3(0.5f, 0f, 0f) + p.Position)) + new float3(0f, 0f, p.ZOffset);

        if (math.sin(p.Direction) != 0f) 
        {
            float3 d = new float3(math.cos(p.Direction), math.sin(p.Direction), 0f);

            vertices[0] -= d * vertices[0].y / d.y;
            vertices[1] -= d * vertices[1].y / d.y;
        }

        vertices[2] = math.transform(p.BaseMatrix, math.mul(transM, new float3(-0.5f + p.SlopeRatio / 2f, 1f - p.PeakRatio, 0f) + p.Position)) + new float3(0f, 0f, p.ZOffset);
        vertices[3] = math.transform(p.BaseMatrix, math.mul(transM, new float3(0.5f - p.SlopeRatio / 2f, 1f - p.PeakRatio, 0f) + p.Position)) + new float3(0f, 0f, p.ZOffset);
        vertices[4] = math.transform(p.BaseMatrix, math.mul(transM, new float3(0f, 1f, 0f) + p.Position)) + new float3(0f, 0f, p.ZOffset);

        int baseIndex = index * vertexCount;
        triangles[0] = baseIndex + 0;
        triangles[1] = baseIndex + 2;
        triangles[2] = baseIndex + 1;
        triangles[3] = baseIndex + 1;
        triangles[4] = baseIndex + 2;
        triangles[5] = baseIndex + 3;
        triangles[6] = baseIndex + 2;
        triangles[7] = baseIndex + 4;
        triangles[8] = baseIndex + 3;

        uvs[0] = new float2(0f, 0f);
        uvs[1] = new float2(1f, 0f);
        uvs[2] = new float2(0f, 1f - p.PeakRatio);
        uvs[3] = new float2(1f, 1f - p.PeakRatio);
        uvs[4] = new float2(0.5f, 1f);

        /*float hx = p.Width * 0.5f;
        float hy = p.Height * 0.5f;

        Vector3 yAxis = new Vector3(Mathf.Cos(p.Direction), Mathf.Sin(p.Direction), 0f);
        Vector3 xAxis = new Vector3(Mathf.Sin(p.Direction), -Mathf.Cos(p.Direction), 0f);

        verticesData = new Vector3[5] {
                (xAxis + yAxis / Mathf.Tan(p.Direction)) * -hx,
                (xAxis + yAxis / Mathf.Tan(p.Direction)) * hx,
                xAxis * -hx * (1 - p.SlopeRatio) + yAxis * p.Height * (1 - p.PeakRatio),
                xAxis * hx * (1 - p.SlopeRatio) + yAxis * p.Height * (1 - p.PeakRatio),
            yAxis * p.Height
        };

        trianglesData = new int[9]
        {
                0, 2, 1, 1, 2, 3, 2, 4, 3
        };

        uvsData = new Vector2[5]
         {
                Vector2.zero,
                Vector2.right,
                new Vector2(0f, 1f - p.PeakRatio),
                new Vector2(1f, 1f - p.PeakRatio),
                new Vector2(0.5f, 1f)
         };*/
    }
}
