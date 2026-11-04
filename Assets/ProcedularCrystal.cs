using UnityEngine;
using System;
using System.Linq;

/*public record BlueShardParams(float Width, float Height, float Direction, Vector2 Offset, float SlopeRatio, float PeakRatio) 
    : ShardParams();

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BlueCrystal : Crystal
{
    [Header("Настройки кристалла")]
    [SerializeField] private Vector2 _widthRatioRange;
    [SerializeField] private AnimationCurve _heightCurve;
    [SerializeField] private Vector2 _xOffsetRange;
    [SerializeField] private Vector2 _yOffsetRange;
    [SerializeField] private Vector2 _slopeRatioRange;
    [SerializeField] private Vector2 _peakRatioRange;
    [SerializeField] private Vector2Int _countRange;
    [SerializeField][Range(0f, Mathf.PI * 2)] private float _growAngle;
    [SerializeField] private bool _sortBySize = true;

    protected override ShardParams[] CrystalCalculator(ref float seed)
    {
        int count = SeedOperations.RandomNumberFromSeed(ref seed, _countRange);

        BlueShardParams[] shards = new BlueShardParams[count];

        for (int i = 0; i < count; i++)
        {
            float height = _heightCurve.Evaluate(SeedOperations.RandomNumberFromSeed(ref seed, 0f, 1f));
            float width = height * SeedOperations.RandomNumberFromSeed(ref seed, _widthRatioRange);
            float direction = SeedOperations.RandomNumberFromSeed(ref seed, (Mathf.PI - _growAngle) / 2f, (Mathf.PI + _growAngle) / 2f);
            Vector2 offset = new Vector2(
                SeedOperations.RandomNumberFromSeed(ref seed, _xOffsetRange),
                SeedOperations.RandomNumberFromSeed(ref seed, _yOffsetRange)
                );
            float slopeRatio = SeedOperations.RandomNumberFromSeed(ref seed, _slopeRatioRange);
            float peakRatio = SeedOperations.RandomNumberFromSeed(ref seed, _peakRatioRange);

            shards[i] = new BlueShardParams(width, height, direction, offset, slopeRatio, peakRatio);
        }

        if (_sortBySize) Array.Sort(shards, (a, b) => (a.Height).CompareTo(b.Height));

        return shards;
    }

    protected override float ShardCalculator(int index, int vertexCount, ref Vector3[] verticesData, ref int[] trianglesData, ref Vector2[] uvsData, ref Vector3 allVerticesOffset, ShardParams parameters)
    {
        if (parameters is BlueShardParams p) { }
        else return -1;

        float hx = p.Width * 0.5f;
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
        };

        return p.Height;
    }
}
*/