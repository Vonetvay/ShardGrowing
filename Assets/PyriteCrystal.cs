using System;
using UnityEngine;
using Unity.Mathematics;



/*public class PyriteCrystal : Crystal
{
    [SerializeField] private Vector2 _sideRange;
    [SerializeField] private Vector2Int _firstCountRange;

    protected override ShardParams[] CrystalCalculator(ref float seed)
    {
        int count = SeedOperations.RandomNumberFromSeed(ref seed, _firstCountRange) + 1;
        PyriteShardParams[] shards = new PyriteShardParams[count];

        Vector3 position = Vector3.zero;
        float side = SeedOperations.RandomNumberFromSeed(ref seed, _sideRange);
        float direction = SeedOperations.RandomNumberFromSeed(ref seed, 0f, Mathf.PI / 2f);

        //shards[0] = new PyriteShardParams(position, side, direction, 0);

        for (int i = 1; i < count + 1; i++)
        {
            position = Vector3.zero;
            side = SeedOperations.RandomNumberFromSeed(ref seed, _sideRange);
            direction = SeedOperations.RandomNumberFromSeed(ref seed, 0f, Mathf.PI / 2f);

            //shards[i] = new PyriteShardParams(position, side, direction, 0);
        }

        Array.Sort(shards, (a, b) => (a.Side).CompareTo(b.Side));

        return shards;
    }

    protected override float ShardCalculator(int index, int vertexCount, ref Vector3[] verticesData, ref int[] trianglesData, ref Vector2[] uvsData, ref Vector3 allVerticesOffset, ShardParams parameters)
    {
        if (parameters is PyriteShardParams p) { }
        else return -1;

        float h = p.Side / 2f;

        //allVerticesOffset += p.Position;

        Vector3 yAxis = new Vector3(Mathf.Cos(p.Direction), Mathf.Sin(p.Direction), 0f);
        Vector3 xAxis = new Vector3(Mathf.Sin(p.Direction), -Mathf.Cos(p.Direction), 0f);

        float2x2 rotateMatrix = float2x2.Rotate(p.Direction);

        verticesData = new Vector3[4]
        {
            -h * xAxis + -h * yAxis,
            h * xAxis + -h * yAxis,
            -h * xAxis + h * yAxis,
            h * xAxis + h * yAxis
        };

        *//*verticesData = new Vector3[4] 
        {
            -h * xAxis + -h * yAxis,
            h * xAxis + -h * yAxis,
            -h * xAxis + h * yAxis,
            h * xAxis + h * yAxis
        };*//*

        trianglesData = new int[6]
        {
            0, 2, 1, 1, 2, 3
        };

        uvsData = new Vector2[4]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f)
        };

        return Mathf.Max(Mathf.Abs(p.Position.x), Mathf.Abs(p.Position.y));
    }
}
*/