using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Slag : CrystalNew
{
    protected override ShardParams[] CrystalCalculator(ref float seed)
    {
        ShardParams[] shards = new ShardParams[1];
        shards[0] = new ShardParams(float3.zero, float4x4.identity, float4x4.identity, 0f, 0f, 1f);
        return shards;
    }

    protected override void ShardCalculator(int index, int vertexCount, NativeSlice<float3> vertices, NativeSlice<int> triangles, NativeSlice<float2> uvs, ShardParams parameters)
    {
        vertices[0] = new float3(-0.5f, -0.5f, 0.0f);
        vertices[1] = new float3(0.5f, -0.5f, 0.0f);
        vertices[2] = new float3(-0.5f, 0.5f, 0.0f);
        vertices[3] = new float3(0.5f, 0.5f, 0.0f);

        int baseIndex = index * vertexCount;
        triangles[0] = baseIndex + 0;
        triangles[1] = baseIndex + 2;
        triangles[2] = baseIndex + 1;
        triangles[3] = baseIndex + 2;
        triangles[4] = baseIndex + 3;
        triangles[5] = baseIndex + 1;

        uvs[0] = new float2(0f, 0f);
        uvs[1] = new float2(1f, 0f);
        uvs[2] = new float2(0f, 1f);
        uvs[3] = new float2(1f, 1f);
    }
}
