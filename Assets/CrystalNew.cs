using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public record ShardParams(float3 Position, float4x4 BaseMatrix, float ZOffset, float Delay, float Duration);

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CrystalNew : MonoBehaviour
{
    [SerializeField] protected int vertexCount = 4;
    [SerializeField] private float _seed;
    [SerializeField] private float _mass;
    [SerializeField] private float _density;

    public void OnValidate()
    {
        GenerateCrystal(_seed, _mass);
    }

    public float GenerateCrystal(float seed, float mass)
    {
        int triangleCount = vertexCount * 3 - 6;
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        Mesh mesh = new Mesh { name = "ProceduralCrystal" };

        float currentSeed = seed;
        SeedOperations.RandomNumberFromSeed(ref currentSeed, 0f, 1f);

        ShardParams[] shardsParams = CrystalCalculator(ref currentSeed);
        int count = shardsParams.Length;

        if (count == 0) return 0f;

        int totalVertices = vertexCount * count;
        int totalTriangles = triangleCount * count;

        // Выделяем нативную память для всех данных меша (Быстро и без GC)
        NativeArray<float3> myVertices = new NativeArray<float3>(totalVertices, Allocator.Temp);
        NativeArray<int> myTriangles = new NativeArray<int>(totalTriangles, Allocator.Temp);
        NativeArray<float2> myUvs = new NativeArray<float2>(totalVertices, Allocator.Temp);
        NativeArray<float2> timeAttributes = new NativeArray<float2>(totalVertices, Allocator.Temp);
        NativeArray<float2> positionAttributes = new NativeArray<float2>(totalVertices, Allocator.Temp);

        for (int i = 0; i < count; i++)
        {
            // Создаем срезы (Slices) для каждого отдельного осколка, чтобы не копировать массивы
            NativeSlice<float3> vertexSlice = new NativeSlice<float3>(myVertices, i * vertexCount, vertexCount);
            NativeSlice<int> triangleSlice = new NativeSlice<int>(myTriangles, i * triangleCount, triangleCount);
            NativeSlice<float2> uvSlice = new NativeSlice<float2>(myUvs, i * vertexCount, vertexCount);

            //float zOffset = (count > 1) ? (float)i / (count - 1) : 0f;

            ShardCalculator(i, vertexCount, vertexSlice, triangleSlice, uvSlice, shardsParams[i]);

            // Заполняем массив delay для текущего осколка
            for (int v = 0; v < vertexCount; v++)
            {
                timeAttributes[i * vertexCount + v] = new float2(shardsParams[i].Delay, shardsParams[i].Duration);//new float2(size, 0f);
                float3 worldShardPosition = math.transform(shardsParams[i].BaseMatrix, shardsParams[i].Position);
                positionAttributes[i * vertexCount + v] = new float2(worldShardPosition.x, worldShardPosition.y);
            }
        }

        // Записываем данные НАПРЯМУЮ в Меш из нативной памяти
        mesh.SetVertices(myVertices);

        Bounds bounds = mesh.bounds;
        float xSize = bounds.size.x;
        float ySize = bounds.size.y;
        float area = xSize * ySize;

        float3 offset = bounds.center;

        float sizeCorrection = math.sqrt((mass / math.max(0.0001f, _density)) / math.max(0.0001f, area));

        Debug.Log(area);

        for (int i = 0; i < totalVertices; i++) 
        {
            float3 vertex = myVertices[i];
            vertex.x *= sizeCorrection;
            vertex.y *= sizeCorrection;
            vertex.x -= offset.x;
            vertex.y -= offset.y;
            positionAttributes[i] -= new float2(offset.x, offset.y);
            myVertices[i] = vertex;
        }

        mesh.SetVertices(myVertices);

        mesh.SetIndexBufferParams(totalTriangles, IndexFormat.UInt32);
        mesh.SetIndexBufferData(myTriangles, 0, 0, totalTriangles, MeshUpdateFlags.DontValidateIndices);

        // Субмеш обязателен при использовании SetIndexBufferData
        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, totalTriangles));

        //mesh.SetTriangles(myTriangles.ToArray(), 0); // Метод SetTriangles пока требует массив, либо используйте MeshUpdateFlags

        mesh.SetUVs(0, myUvs);
        mesh.SetUVs(2, timeAttributes);
        mesh.SetUVs(3, positionAttributes);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;

        // Расчет площади Bounds (вместо тяжелого LINQ Sort на CPU)
        

        // Обязательно освобождаем выделенную нативную память
        myVertices.Dispose();
        myTriangles.Dispose();
        myUvs.Dispose();
        timeAttributes.Dispose();
        positionAttributes.Dispose();

        return xSize * ySize;
    }

    protected virtual ShardParams[] CrystalCalculator(ref float seed) => Array.Empty<ShardParams>();

    protected virtual void ShardCalculator(int index, int vertexCount, NativeSlice<float3> vertices, NativeSlice<int> triangles, NativeSlice<float2> uvs, ShardParams parameters)
    {
        
    }
}
