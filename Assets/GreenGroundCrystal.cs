using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public record GreenGroundShardParams(float3 Position, float4x4 BaseMatrix, float ZOffset, float Delay, float Duration,
    float Width, float Height, float Direction, float PeakRatio, float TopWidthRatio) : ShardParams(Position, BaseMatrix, ZOffset, Delay, Duration);

public class GreenGroundCrystal : CrystalNew
{
    [SerializeField] private Vector2Int _countRange;
    [SerializeField] private Vector2 _widthRatioRange;
    [SerializeField] private AnimationCurve _heightCurve;
    [SerializeField] private float _maxIncline;
    [SerializeField] private Vector2 _xOffsetRange;
    [SerializeField] private Vector2 _yOffsetRange;
    [SerializeField] private Vector2 _peakRatioRange;
    [SerializeField] private Vector2 _topWidthRatioRange;

    protected override ShardParams[] CrystalCalculator(ref float seed)
    {
        int count = SeedOperations.RandomNumberFromSeed(ref seed, _countRange);

        GreenGroundShardParams[] shards = new GreenGroundShardParams[count];

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
            float direction = SeedOperations.RandomNumberFromSeed(ref seed, -_maxIncline, _maxIncline);
            float3 offset = new float3(
                SeedOperations.RandomNumberFromSeed(ref seed, _xOffsetRange),
                SeedOperations.RandomNumberFromSeed(ref seed, _yOffsetRange),
                0f
                );
            float peakRatio = SeedOperations.RandomNumberFromSeed(ref seed, _peakRatioRange);
            float topWidthRatio = SeedOperations.RandomNumberFromSeed(ref seed, _topWidthRatioRange);
            float zOffset = count > 1 ? i / (float)(count - 1) : 0f;

            shards[i] = new GreenGroundShardParams(offset, float4x4.identity, zOffset, 0f, height / maxHeight,
                width, height, direction, peakRatio, topWidthRatio);
        }

        return shards;
    }

    protected override void ShardCalculator(int index, int vertexCount, NativeSlice<float3> vertices, NativeSlice<int> triangles, NativeSlice<float2> uvs, ShardParams parameters)
    {
        if (parameters is GreenGroundShardParams p) { }
        else return;

        float3x3 rotM = float3x3.RotateZ(p.Direction);
        float3x3 scaleM = float3x3.Scale(new float3(p.Width, p.Height, 0f));

        float3x3 transM = math.mul(rotM, scaleM);

        vertices[0] = math.mul(transM, new float3(-0.5f, 0f, 0f)) + p.Position + new float3(0f, 0f, p.ZOffset);
        vertices[1] = math.mul(transM, new float3(0.5f, 0f, 0f)) + p.Position + new float3(0f, 0f, p.ZOffset);

        if (math.sin(p.Direction + math.PIHALF) != 0f)
        {
            float3 d = new float3(math.cos(p.Direction + math.PIHALF), math.sin(p.Direction + math.PIHALF), 0f);

            vertices[0] -= d * vertices[0].y / d.y;
            vertices[1] -= d * vertices[1].y / d.y;
        }

        vertices[2] = math.mul(transM, new float3(-0.5f, 1f - p.PeakRatio, 0f)) + p.Position + new float3(0f, 0f, p.ZOffset);
        vertices[3] = math.mul(transM, new float3(0.5f, 1f - p.PeakRatio, 0f)) + p.Position + new float3(0f, 0f, p.ZOffset);
        vertices[4] = math.mul(transM, new float3(-p.TopWidthRatio / 2f, 1f, 0f)) + p.Position + new float3(0f, 0f, p.ZOffset);
        vertices[5] = math.mul(transM, new float3(p.TopWidthRatio / 2f, 1f, 0f)) + p.Position + new float3(0f, 0f, p.ZOffset);

        int baseIndex = index * vertexCount;
        triangles[0] = baseIndex + 0;
        triangles[1] = baseIndex + 2;
        triangles[2] = baseIndex + 1;
        triangles[3] = baseIndex + 2;
        triangles[4] = baseIndex + 4;
        triangles[5] = baseIndex + 1;
        triangles[6] = baseIndex + 4;
        triangles[7] = baseIndex + 5;
        triangles[8] = baseIndex + 1;
        triangles[9] = baseIndex + 5;
        triangles[10] = baseIndex + 3;
        triangles[11] = baseIndex + 1;

        uvs[0] = new float2(0f, 0f);
        uvs[1] = new float2(1f, 0f);
        uvs[2] = new float2(0f, 1f - p.PeakRatio);
        uvs[3] = new float2(1f, 1f - p.PeakRatio);
        uvs[4] = new float2(0.5f - p.TopWidthRatio / 2f, 1f);
        uvs[5] = new float2(0.5f + p.TopWidthRatio / 2f, 1f);
    }
}
