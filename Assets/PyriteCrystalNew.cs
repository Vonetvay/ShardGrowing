using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public record PyriteShardParams(float3 Position, float4x4 BaseMatrix, float ZOffset, float Delay, float Duration, float Side, float Direction)
    : ShardParams(Position, BaseMatrix, ZOffset, Delay, Duration), IComparable<PyriteShardParams>
{
    public int CompareTo(PyriteShardParams obj)
    {
        if (obj == null) return 1;

        return this.Side.CompareTo(obj.Side);
    }
}

public class PyriteCrystalNew : CrystalNew
{
    [SerializeField] private Vector2 _rootSideRange;
    [SerializeField] private bool _canRotatedRoot = false;
    [SerializeField] private Vector2 _secondarySideFactorRange;
    [SerializeField] private Vector2Int _secondaryCountRange = new Vector2Int(1, 4);
    [SerializeField] private bool _canRotatedSecondary = false;
    
    protected override ShardParams[] CrystalCalculator(ref float seed)
    {
        int countToSelect = SeedOperations.RandomNumberFromSeed(ref seed, _secondaryCountRange);

        int count = countToSelect + 1;
        PyriteShardParams[] shards = new PyriteShardParams[count];

        float3 position = float3.zero;
        float side = SeedOperations.RandomNumberFromSeed(ref seed, _rootSideRange);
        float direction = _canRotatedRoot ? SeedOperations.RandomNumberFromSeed(ref seed, 0f, math.PIHALF) : 0f;
        float4x4 baseMatrix = float4x4.identity;

        shards[0] = new PyriteShardParams(position, baseMatrix, 1f, 0f, 0.5f, side, direction);

        baseMatrix = float4x4.TRS(position, quaternion.RotateZ(direction), new float3(side, side, 0f));

        List<float3> points = new List<float3>();

        for (int i = 1; i < count; i++) 
        {
            AddBestCandidatePoint(points, ref seed);
            position = points.Last();

            side = SeedOperations.RandomNumberFromSeed(ref seed, _secondarySideFactorRange);
            
            direction = _canRotatedSecondary ? SeedOperations.RandomNumberFromSeed(ref seed, 0f, math.PIHALF) : 0f;

            float zOffset = 1f - (side - _secondarySideFactorRange.x) / 
                (_secondarySideFactorRange.y == _secondarySideFactorRange.x ? 1f : _secondarySideFactorRange.y - _secondarySideFactorRange.x);

            shards[i] = new PyriteShardParams(position, baseMatrix, zOffset, 0.5f, 0.5f, side, direction);
        }

        Array.Sort(shards);
        return shards;
    }

    protected override void ShardCalculator(int index, int vertexCount, NativeSlice<float3> vertices, NativeSlice<int> triangles, NativeSlice<float2> uvs, ShardParams parameters)
    {
        if (parameters is not PyriteShardParams p) return;

        float3x3 rotM = float3x3.RotateZ(p.Direction);
        float3x3 scaleM = float3x3.Scale(new float3(p.Side, p.Side, 0f));

        float3x3 transM = math.mul(rotM, scaleM);

        // 1. Создаем матрицу поворота 2x2

        // 2. Поворачиваем вершины через math.mul и сразу записываем в срез NativeSlice
        vertices[0] = math.transform(p.BaseMatrix, math.mul(transM, new float3(-0.5f, -0.5f, 0f)) + p.Position) + new float3(0f, 0f, p.ZOffset);
        vertices[1] = math.transform(p.BaseMatrix, math.mul(transM, new float3(0.5f, -0.5f, 0f)) + p.Position) + new float3(0f, 0f, p.ZOffset);
        vertices[2] = math.transform(p.BaseMatrix, math.mul(transM, new float3(-0.5f, 0.5f, 0f)) + p.Position) + new float3(0f, 0f, p.ZOffset);
        vertices[3] = math.transform(p.BaseMatrix, math.mul(transM, new float3(0.5f, 0.5f, 0f)) + p.Position) + new float3(0f, 0f, p.ZOffset);
        /*vertices[0] = new float3(math.mul(p.A, math.mul(localMatrix, new float3(-0.5f, -0.5f, 0f)) + p.Position), p.ZOffset);
        vertices[1] = new float3(math.mul(p.A, math.mul(localMatrix, new float3(0.5f, -0.5f, 0f)) + p.Position), p.ZOffset);
        vertices[2] = new float3(math.mul(p.A, math.mul(localMatrix, new float3(-0.5f, 0.5f)) + p.Position), p.ZOffset);
        vertices[3] = new float3(math.mul(p.A, math.mul(localMatrix, new float3(0.5f, 0.5f)) + p.Position), p.ZOffset);*/

        // 3. Записываем индексы треугольников со смещением под локальный индекс
        int baseIndex = index * vertexCount;
        triangles[0] = baseIndex + 0;
        triangles[1] = baseIndex + 2;
        triangles[2] = baseIndex + 1;
        triangles[3] = baseIndex + 1;
        triangles[4] = baseIndex + 2;
        triangles[5] = baseIndex + 3;

        // 4. Записываем UV
        uvs[0] = new float2(0f, 0f);
        uvs[1] = new float2(1f, 0f);
        uvs[2] = new float2(0f, 1f);
        uvs[3] = new float2(1f, 1f);
    }

    public static List<float3> AddBestCandidatePoint(List<float3> existingPoints, ref float seed, int candidatesCount = 20)
    {
        // Если список пуст, создаем его при необходимости и добавляем первую случайную точку
        if (existingPoints == null || existingPoints.Count == 0)
        {
            existingPoints ??= new List<float3>();

            float rx = SeedOperations.RandomNumberFromSeed(ref seed, -0.5f, 0.5f);
            float ry = SeedOperations.RandomNumberFromSeed(ref seed, -0.5f, 0.5f);

            existingPoints.Add(new float3(rx, ry, 0f));
            return existingPoints;
        }

        float3 bestCandidate = float3.zero;
        float maxBestDistanceSq = -1f;

        for (int i = 0; i < candidatesCount; i++)
        {
            // Генерируем координаты кандидата, используя ваш SeedOperations
            float cx = SeedOperations.RandomNumberFromSeed(ref seed, -0.5f, 0.5f);
            float cy = SeedOperations.RandomNumberFromSeed(ref seed, -0.5f, 0.5f);
            float3 candidate = new float3(cx, cy, 0f);

            float minDistanceSq = float.MaxValue;

            // Ищем ближайшего соседа среди существующих точек
            for (int j = 0; j < existingPoints.Count; j++)
            {
                float distSq = math.distancesq(candidate, existingPoints[j]);

                if (distSq < minDistanceSq)
                {
                    minDistanceSq = distSq;
                }
            }

            // Ищем кандидата, у которого минимальное расстояние до соседей МАКСИМАЛЬНО
            if (minDistanceSq > maxBestDistanceSq)
            {
                maxBestDistanceSq = minDistanceSq;
                bestCandidate = candidate;
            }
        }

        existingPoints.Add(bestCandidate);
        return existingPoints;
    }

    public static List<float3> AddBestCandidatePointVoronoi(List<float3> existingPoints, ref float seed, int candidatesCount = 20)
    {
        existingPoints ??= new List<float3>();
        int initialCount = existingPoints.Count;

        // Если точек нет, просто создаем первую случайную точку
        if (initialCount == 0)
        {
            float rx = SeedOperations.RandomNumberFromSeed(ref seed, -0.5f, 0.5f);
            float ry = SeedOperations.RandomNumberFromSeed(ref seed, -0.5f, 0.5f);
            existingPoints.Add(new float3(rx, ry, 0f));
            return existingPoints;
        }

        // --- ШАГ 1: ЧЕСТНАЯ РЕЛАКСАЦИЯ ЛЛОЙДА (Через дискретизацию пространства) ---
        // Разрешение сетки для поиска центроидов ячеек Вороного.
        // 32x32 дает отличный баланс точности и скорости.
        const int gridSize = 32;

        // Массивы для подсчета центра масс (суммы координат пикселей) для каждой точки
        float2[] cellSum = new float2[initialCount];
        int[] cellPixelCount = new int[initialCount];

        // Сканируем всю плоскость от -1 до 1 с шагом сетки
        float step = 2.0f / gridSize;

        for (int gx = 0; gx < gridSize; gx++)
        {
            // Переводим индекс сетки в координату X от -1 до 1
            float px = -1.0f + (gx + 0.5f) * step;

            for (int gy = 0; gy < gridSize; gy++)
            {
                // Переводим индекс сетки в координату Y от -1 до 1
                float py = -1.0f + (gy + 0.5f) * step;
                float2 pixelPos = new float2(px, py);

                // Ищем, к какой из существующих точек этот пиксель ближе всего (ячейка Вороного)
                int closestPointIndex = 0;
                float minDataDistSq = float.MaxValue;

                for (int p = 0; p < initialCount; p++)
                {
                    float distSq = math.distancesq(pixelPos, existingPoints[p].xy);
                    if (distSq < minDataDistSq)
                    {
                        minDataDistSq = distSq;
                        closestPointIndex = p;
                    }
                }

                // Добавляем координаты пикселя в копилку этой ячейки
                cellSum[closestPointIndex] += pixelPos;
                cellPixelCount[closestPointIndex]++;
            }
        }

        // Перемещаем каждую точку в истинный геометрический центр (центроид) её ячейки
        for (int i = 0; i < initialCount; i++)
        {
            if (cellPixelCount[i] > 0)
            {
                float2 centroid = cellSum[i] / cellPixelCount[i];
                existingPoints[i] = new float3(centroid.x, centroid.y, 0f);
            }
        }

        // --- ШАГ 2: ДОБАВЛЕНИЕ НОВОЙ ТОЧКИ (Митчелл поверх Ллойда) ---
        float3 bestCandidate = float3.zero;
        float maxBestDistanceSq = -1f;

        for (int i = 0; i < candidatesCount; i++)
        {
            float cx = SeedOperations.RandomNumberFromSeed(ref seed, -0.5f, 0.5f);
            float cy = SeedOperations.RandomNumberFromSeed(ref seed, -0.5f, 0.5f);
            float3 candidate = new float3(cx, cy, 0f);

            float minDistanceSq = float.MaxValue;

            for (int j = 0; j < initialCount; j++)
            {
                float distSq = math.distancesq(candidate, existingPoints[j]);
                if (distSq < minDistanceSq)
                {
                    minDistanceSq = distSq;
                }
            }

            if (minDistanceSq > maxBestDistanceSq)
            {
                maxBestDistanceSq = minDistanceSq;
                bestCandidate = candidate;
            }
        }

        existingPoints.Add(bestCandidate);
        return existingPoints;
    }
}

