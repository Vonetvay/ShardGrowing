using System;
using System.Linq;
using UnityEngine;


/*public class Crystal : MonoBehaviour
{
    [SerializeField] protected int vertexCount;
    [SerializeField] private float _seed;

    public void OnValidate()
    {
        GenerateCrystal(_seed);
    }

    public float GenerateCrystal(float seed)
    {
        int triangleCount = vertexCount * 3 - 6;
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralCrystal";

        float currentSeed = seed;
        SeedOperations.RandomNumberFromSeed(ref currentSeed, 0f, 1f);

        ShardParams[] shardsParams = CrystalCalculator(ref currentSeed);
        int count = shardsParams.Length;

        Vector3[] myVertices = new Vector3[vertexCount * count];
        int[] myTriangles = new int[triangleCount * count];
        Vector2[] myUvs = new Vector2[vertexCount * count];
        Vector3[] myNormals = new Vector3[vertexCount * count];
        Vector2[] delay = new Vector2[count * vertexCount];
        float[] shardSize = new float[count];
        
        for (int i = 0; i < count; i++) 
        {
            shardSize[i] = GenerateShard(i, vertexCount, count, ref myVertices, ref myTriangles, ref myUvs, ref delay, shardsParams[i]);
        }

        Array.Fill(myNormals, -Vector3.forward);

        mesh.vertices = myVertices;
        mesh.triangles = myTriangles;
        mesh.uv = myUvs;
        mesh.normals = myNormals;

        float biggestShard = shardSize.Max();

        for (int i = 0; i < myVertices.Length; i++)
        {
            delay[i].x /= biggestShard > 0f ? biggestShard : 1f;
        }
        mesh.SetUVs(2, delay);
        
        meshFilter.mesh = mesh;

        float xSize = (myVertices.OrderByDescending(p => p.x).First() - myVertices.OrderByDescending(p => p.x).Last()).x;
        float ySize = (myVertices.OrderByDescending(p => p.y).First() - myVertices.OrderByDescending(p => p.y).Last()).y;

        return xSize * ySize;
    }

    protected virtual ShardParams[] CrystalCalculator(ref float seed) 
    {
        return null;
    }

    protected float GenerateShard(int index, int vertexCount, int count, ref Vector3[] vertices, ref int[] triangles, ref Vector2[] uvs, ref Vector2[] delay, ShardParams parameters)
    {
        int triangleCount = vertexCount * 3 - 6;
        Vector3[] verticesData = new Vector3[vertexCount];
        Vector3 allVerticesOffset = Vector3.zero;
        int[] trianglesData = new int[triangleCount];
        Vector2[] uvsData = new Vector2[vertexCount];

        float z = 0f;
        if (count > 1) z = index / (float)(count - 1);
        allVerticesOffset += Vector3.forward * z;

        float size = ShardCalculator(index, vertexCount, ref verticesData, ref trianglesData, ref uvsData, ref allVerticesOffset, parameters);

        Array.Fill(delay, size * Vector2.right, index * vertexCount, vertexCount);

        Array.Copy(Array.ConvertAll(verticesData, x => x + allVerticesOffset), 0, vertices, index * vertexCount, vertexCount);

        Array.Copy(Array.ConvertAll(trianglesData, x => x + index * vertexCount), 0, triangles, index * triangleCount, triangleCount);

        Array.Copy(uvsData, 0, uvs, index * vertexCount, vertexCount);

        return size;
    }

    protected virtual float ShardCalculator(int index, int vertexCount, ref Vector3[] verticesData, ref int[] trianglesData, ref Vector2[] uvsData, ref Vector3 allVerticesOffset, ShardParams parameters)
    {
        return 0f;
    }
}
*/