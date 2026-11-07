using System;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;

public record BlueShardParams(float3 Position, float4x4 BaseMatrix, float4x4 TransformMatrix, float ZOffset, float Delay, float Duration, 
    float Width, float Height, float Direction, float SlopeRatio, float PeakRatio, float MaxHeight)
    : ShardParams(Position, BaseMatrix, TransformMatrix, ZOffset, Delay, Duration);

[Serializable]
public struct CrystalGrowthLevel 
{
    [MinMaxSlider(0f, 1f)] public Vector2 HeightRange;
    public Vector2Int CountRange;
    [Range(0f, Mathf.PI)] public float DirectionRange;
    public float Delay;
    [Min(0f)] public float _maxXOffset;

    [NonSerialized] public int count;
}

public class BlueCrystalNew : CrystalNew
{
    [SerializeField] private CrystalGrowthLevel _largeCrystalLevel;
    [SerializeField] private CrystalGrowthLevel _mediumCrystalLevel;
    [SerializeField] private CrystalGrowthLevel _smallCrystalLevel;

    [SerializeField] private int _directionSampleCount = 10;

    [Header("Настройка анимации")]
    [SerializeField] private float _startingAnimationWidth = 0.4f;
    [SerializeField] private float _startingAnimationConstantHeight = 0.2f;
    //[SerializeField] private float _startingAnimationHeight = 0.2f;
    [SerializeField] private float _widthAnimationSpeedRatio = 0.6f;

    [Header("Настройки кристалла")]
    [SerializeField] private Vector2 _widthRatioRange;
    [SerializeField] private AnimationCurve _heightCurve;
    
    [SerializeField] private Vector2 _yOffsetRange;
    [SerializeField] private Vector2 _slopeRatioRange;
    [SerializeField] private Vector2 _peakRatioRange;
    [SerializeField] private Vector2Int _countRange;
    [SerializeField] private Vector2 _delayOffsetRange;
    [SerializeField][Range(0f, Mathf.PI * 2)] private float _growAngle;
    private bool _sortBySize = true;

    private float GetUniformDirection(ref float seed, float[] points, float range, int sampleCount=10) 
    {
        if (points.Length == 0) 
        {
            return SeedOperations.RandomNumberFromSeed(ref seed, (Mathf.PI - range) / 2f, (Mathf.PI + range) / 2f);
        }

        float[] sample = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++) 
        {
            sample[i] = SeedOperations.RandomNumberFromSeed(ref seed, (Mathf.PI - range) / 2f, (Mathf.PI + range) / 2f);
        }

        float[] distances = new float[sampleCount];

        float[] merge = sample.Concat(points).ToArray();

        for (int i = 0; i < sampleCount; i++) 
        {
            distances[i] = math.PI2;
            foreach (var p in merge) 
            {
                if (p == sample[i]) continue;
                distances[i] = math.min(math.abs(sample[i] - p), distances[i]);
            }
        }

        return sample[Array.IndexOf(distances, distances.Max())];
    }

    protected override ShardParams[] CrystalCalculator(ref float seed)
    {
        _largeCrystalLevel.count = SeedOperations.RandomNumberFromSeed(ref seed, _largeCrystalLevel.CountRange);
        _mediumCrystalLevel.count = SeedOperations.RandomNumberFromSeed(ref seed, _mediumCrystalLevel.CountRange);
        _smallCrystalLevel.count = SeedOperations.RandomNumberFromSeed(ref seed, _smallCrystalLevel.CountRange);

        int totalCount = _largeCrystalLevel.count + _mediumCrystalLevel.count + _smallCrystalLevel.count;

        BlueShardParams[] shards = new BlueShardParams[totalCount];

        int index = 0;

        foreach (var level in new CrystalGrowthLevel[3] { _smallCrystalLevel, _mediumCrystalLevel, _largeCrystalLevel })
        {
            float[] directions = new float[level.count];
            float[] heights = new float[level.count];

            for (int i = 0; i < level.count; i++)
            {
                heights[i] = SeedOperations.RandomNumberFromSeed(ref seed, level.HeightRange);
            }

            Array.Sort(heights);
            //float maxHeight = heights[totalCount - 1];

            for (int i = 0; i < level.count; i++)
            {
                float height = heights[i];
                float width = height * SeedOperations.RandomNumberFromSeed(ref seed, _widthRatioRange);
                float direction = GetUniformDirection(ref seed, directions, level.DirectionRange, _directionSampleCount);//SeedOperations.RandomNumberFromSeed(ref seed, (Mathf.PI - level.DirectionRange) / 2f, (Mathf.PI + level.DirectionRange) / 2f);
                directions[i] = direction;
                float3 offset = new float3(
                    SeedOperations.RandomNumberFromSeed(ref seed, 
                    level._maxXOffset * (-1f + math.sign(math.cos(direction))) / 2f, 
                    level._maxXOffset * (1f + math.sign(math.cos(direction))) / 2f
                    ),
                    SeedOperations.RandomNumberFromSeed(ref seed, _yOffsetRange),
                    0f
                    );
                float slopeRatio = SeedOperations.RandomNumberFromSeed(ref seed, _slopeRatioRange);
                float peakRatio = SeedOperations.RandomNumberFromSeed(ref seed, _peakRatioRange);
                float zOffset = totalCount > 1 ? (i + index) / (float)(totalCount - 1) : 0f;

                float delayOffset = SeedOperations.RandomNumberFromSeed(ref seed, _delayOffsetRange);
                float delay = level.Delay;

                shards[i + index] = new BlueShardParams(offset, float4x4.identity, float4x4.TRS(offset + new float3(0f, 0f, zOffset), quaternion.RotateZ(direction - math.PIHALF), new float3(width, height, 0f)), zOffset, delay, 1f - delay,
                    width, height, direction, slopeRatio, peakRatio, 0f);
            }

            index += level.count - 1;
        }

        /*for (int i = 0; i < count; i++)
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

            shards[i] = new BlueShardParams(offset, float4x4.identity, float4x4.TRS(offset + new float3(0f, 0f, zOffset), quaternion.RotateZ(direction - math.PIHALF), new float3(width, height, 0f)), zOffset, delay, 1f - delay,
                width, height, direction, slopeRatio, peakRatio, maxHeight);
        }*/

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

        /*vertices[0] = math.transform(p.BaseMatrix, math.mul(transM, new float3(-0.5f, 0f, 0f) + p.Position)) + new float3(0f, 0f, p.ZOffset);
        vertices[1] = math.transform(p.BaseMatrix, math.mul(transM, new float3(0.5f, 0f, 0f) + p.Position)) + new float3(0f, 0f, p.ZOffset);

        if (math.sin(p.Direction) != 0f)
        {
            float3 d = new float3(math.cos(p.Direction), math.sin(p.Direction), 0f);

            vertices[0] -= d * vertices[0].y / d.y;
            vertices[1] -= d * vertices[1].y / d.y;
        }*/

        vertices[2] = math.transform(p.BaseMatrix, math.mul(transM, new float3(-0.5f + p.SlopeRatio / 2f, 1f - p.PeakRatio, 0f) + p.Position)) + new float3(0f, 0f, p.ZOffset);
        vertices[3] = math.transform(p.BaseMatrix, math.mul(transM, new float3(0.5f - p.SlopeRatio / 2f, 1f - p.PeakRatio, 0f) + p.Position)) + new float3(0f, 0f, p.ZOffset);
        vertices[4] = math.transform(p.BaseMatrix, math.mul(transM, new float3(0f, 1f, 0f) + p.Position)) + new float3(0f, 0f, p.ZOffset);

        vertices[0] = new float3(-0.5f, 0f, 0f);
        vertices[1] = new float3(0.5f, 0f, 0f);
        vertices[2] = new float3(-0.5f + p.SlopeRatio / 2f, 1f - p.PeakRatio, 0f);
        vertices[3] = new float3(0.5f - p.SlopeRatio / 2f, 1f - p.PeakRatio, 0f);
        vertices[4] = new float3(0f, 1f, 0f);

        LocalShardAnimate(_animationProgress, vertices, p.Position, p);

        for (int i = 0; i < vertexCount; i++) 
        {
            vertices[i] = math.transform(p.BaseMatrix, math.transform(p.TransformMatrix, vertices[i]));
        }

        //GlobalShardAnimate(_animationProgress, vertices, new float3(0f, 0f, 0f), p);

        if (math.cos(p.Direction) != 0f)
        {
            float3 d = new float3(math.cos(p.Direction), math.sin(p.Direction), 0f);

            vertices[0] -= d * vertices[0].y / d.y;
            vertices[1] -= d * vertices[1].y / d.y;
        }

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
    }

    /*protected override void GlobalShardAnimate(float progress, NativeSlice<float3> vertices, float3 animationPivot, ShardParams parameters) 
    {
        base.GlobalShardAnimate(progress, vertices, animationPivot, parameters);

        if (parameters is BlueShardParams p) { }
        else return;

        float delay = p.Delay;
        float duration = p.Duration;

        progress = math.clamp((_animationProgress - delay) / duration, 0f, 1f);

        float vertexProgressY = 0f;
        float vertexProgressX = 0f;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertexProgressY += (float)_startingAnimationConstantHeight;
            //vertexProgressX = _startingAnimationWidth * math.sign(progress) + progress * _widthAnimationSpeedRatio;
            vertices[i] = new float3(
                math.lerp(animationPivot.x, vertices[i].x, math.clamp(vertexProgressX, 0f, 1f)),
                math.lerp(animationPivot.y, vertices[i].y, math.clamp(vertexProgressY, 0f, 1f)),
                vertices[i].z
            );
        }
    }*/

    protected override void LocalShardAnimate(float progress, NativeSlice<float3> localVertices, float3 animationPivot, ShardParams parameters)
    {
        base.LocalShardAnimate(progress, localVertices, animationPivot, parameters);

        if (parameters is BlueShardParams p) { }
        else return;

        float delay = p.Delay;
        float duration = p.Duration;

        progress = math.clamp((_animationProgress - delay) / duration, 0f, 1f);

        float vertexProgressY = 0f;
        float vertexProgressX = 0f;

        for (int i = 0; i < localVertices.Length; i++)
        {
            vertexProgressY = progress + (float)_startingAnimationConstantHeight / p.Height;
            vertexProgressX = _startingAnimationWidth * math.sign(progress) + progress * _widthAnimationSpeedRatio;
            localVertices[i] = new float3(
                math.lerp(animationPivot.x, localVertices[i].x, math.clamp(vertexProgressX, 0f, 1f)),
                math.lerp(animationPivot.y, localVertices[i].y, math.clamp(vertexProgressY, 0f, 1f)),
                localVertices[i].z
            );
        };
    }
}
