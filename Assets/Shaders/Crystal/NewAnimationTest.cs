using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public record TestShardParams(float3 Position, float4x4 BaseMatrix, float ZOffset, float Delay, float Duration,
    float Height, float Width, float Direction) : ShardParams(Position, BaseMatrix, ZOffset, Delay, Duration);

public class NewAnimationTest : CrystalNew
{
    [SerializeField] private float3 Position = float3.zero;
    [SerializeField] private float Height = 1f;
    [SerializeField] private float Width = 1f;
    [SerializeField] private float GrainsGrowthDuration = 0.2f;
    [SerializeField][Range(-math.PIHALF, math.PIHALF)] private float Direction = 0f;

    protected override ShardParams[] CrystalCalculator(ref float seed)
    {
        TestShardParams[] shards = new TestShardParams[1];
        shards[0] = new TestShardParams(Position, float4x4.identity, 0f, 0f, 1f, Height, Width, Direction);

        return shards;
    }

    protected override void SetShardVertices(NativeSlice<float3> vertices, int index, ShardParams parameters)
    {
        if (parameters is TestShardParams p) { }
        else return;

        Debug.Log("Vertices set");

        float2x2 scaleM = float2x2.Scale(new float2(p.Width, p.Height));
        float2x2 rotM = float2x2.Rotate(p.Direction);
        float2x2 transM = math.mul(rotM, scaleM);
        //float2x2 transM = float2x2.identity;

        vertices[0] = new float3(math.mul(transM, new float2(-0.5f, 0f)), p.ZOffset) + p.Position;
        vertices[1] = new float3(math.mul(transM, new float2(0.5f, 0f)), p.ZOffset) + p.Position;
        vertices[2] = new float3(math.mul(transM, new float2(-0.5f, 0.8f)), p.ZOffset) + p.Position;
        vertices[3] = new float3(math.mul(transM, new float2(0.5f, 0.8f)), p.ZOffset) + p.Position;
        vertices[4] = new float3(math.mul(transM, new float2(0f, 1f)), p.ZOffset) + p.Position;
    }

    protected override void SetShardTriangles(NativeSlice<int> triangles, int baseIndex, ShardParams parameters)
    {
        if (parameters is TestShardParams p) { }
        else return;

        triangles[0] = 0 + baseIndex;
        triangles[1] = 2 + baseIndex;
        triangles[2] = 1 + baseIndex;
        triangles[3] = 1 + baseIndex;
        triangles[4] = 2 + baseIndex;
        triangles[5] = 3 + baseIndex;
        triangles[6] = 2 + baseIndex;
        triangles[7] = 4 + baseIndex;
        triangles[8] = 3 + baseIndex;
    }

    protected override void SetShardUVs(NativeSlice<float2> uvs, int index, ShardParams parameters)
    {
        if (parameters is TestShardParams p) { }
        else return;

        uvs[0] = new float2(0f, 0f);
        uvs[1] = new float2(1f, 0f);
        uvs[2] = new float2(0f, 0.8f);
        uvs[3] = new float2(1f, 0.8f);
        uvs[4] = new float2(0.5f, 1f);
    }

    protected override void SetShardAnimation(NativeSlice<float2> animationUVs, int index, ShardParams parameters)
    {
        if (parameters is TestShardParams p) { }
        else return;

        float2x2 rotM = float2x2.Rotate(p.Direction);

        /*animationUVs[0] = CalculateAnimationDuration(new float2(GrainsGrowthDuration, 1f), p.Direction, new float2(-0.5f, 0f));
        animationUVs[1] = CalculateAnimationDuration(new float2(GrainsGrowthDuration, 1f), p.Direction, new float2(0.5f, 0f));
        animationUVs[2] = CalculateAnimationDuration(new float2(GrainsGrowthDuration, 1f), p.Direction, new float2(-0.5f, 0.8f));
        animationUVs[3] = CalculateAnimationDuration(new float2(GrainsGrowthDuration, 1f), p.Direction, new float2(0.5f, 0.8f));
        animationUVs[4] = CalculateAnimationDuration(new float2(1f, 1f), p.Direction, new float2(0f, 1f));*/

        animationUVs[0] = new float2(GrainsGrowthDuration, 1f);
        animationUVs[1] = new float2(GrainsGrowthDuration, 1f);
        animationUVs[2] = new float2(GrainsGrowthDuration, 1f);
        animationUVs[3] = new float2(GrainsGrowthDuration, 1f);
        animationUVs[4] = new float2(1f, 1f);
    }

    protected override void SetShardOrientation(NativeSlice<float2> orientationUVs, int index, ShardParams parameters)
    {
        if (parameters is TestShardParams p) { }
        else return;

        orientationUVs[0] = new float2(p.Direction, 0f);
        orientationUVs[1] = new float2(p.Direction, 0f);
        orientationUVs[2] = new float2(p.Direction, 0f);
        orientationUVs[3] = new float2(p.Direction, 0f);
        orientationUVs[4] = new float2(p.Direction, 0f);
    }

    protected float2 CalculateAnimationDuration(float2 duration, float direction, float2 position) 
    {
        float2 velocity = new float2(position.x == 0 ? 1f : position.x / duration.x, position.y == 0 ? 1f : position.y / duration.y);
        float2 newVelocity = new float2(
            velocity.x * math.cos(direction) + velocity.y * math.sin(direction),
            velocity.x * math.sin(direction) + velocity.y * math.cos(direction));
        float2 newPosition = math.mul(float2x2.Rotate(direction), position);
        return new float2(newPosition.x / newVelocity.x, newPosition.y / newVelocity.y);
    }
}
