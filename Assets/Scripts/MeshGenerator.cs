using UnityEngine;

public static class MeshGenerator
{
    /*public static MeshData GenerateTerrainMesh(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        MeshData meshData = new MeshData();
        meshData.vertices = new Unity.Collections.NativeArray<Unity.Mathematics.float3>(width * height, Unity.Collections.Allocator.Temp);
        meshData.uvs = new Unity.Collections.NativeArray<Unity.Mathematics.float2>(width * height, Unity.Collections.Allocator.Temp);
        meshData.triangles = new Unity.Collections.NativeArray<int>((width - 1) * (height - 1) * 6, Unity.Collections.Allocator.Temp);

        int vertexIndex = 0;
        int triangleIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {

                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightMap[x, y], topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if (x < width - 1 && y < height - 1)
                {
                    AddTriangles(vertexIndex, vertexIndex + width + 1, vertexIndex + width, meshData, triangleIndex);
                    AddTriangles(vertexIndex + width + 1, vertexIndex, vertexIndex + 1, meshData, triangleIndex);
                }

                vertexIndex++;
            }
        }

        return meshData;

    }

    public static void AddTriangles(int a, int b, int c, MeshData meshData, int triangleIndex)
    {
        meshData.triangles[triangleIndex] = a;
        meshData.triangles[triangleIndex + 1] = b;
        meshData.triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public static Mesh CreateMesh(MeshData meshData)
    {
        Mesh mesh = new Mesh();
        Vector3[] p = new Vector3[meshData.vertices.Length];
        for (int i = 0; i < meshData.vertices.Length; i++)
        {
            p[i] = meshData.vertices[i];
        }
        mesh.vertices = p;
        int[] v = new int[meshData.triangles.Length];
        for (int i = 0; i < meshData.triangles.Length; i++)
        {
            v[i] = meshData.triangles[i];
        }
        mesh.triangles = v;
        Vector2[] d = new Vector2[meshData.uvs.Length];
        for (int i = 0; i < meshData.uvs.Length; i++)
        {
            d[i] = meshData.uvs[i];
        }
        mesh.uv = d;
        mesh.RecalculateNormals();
        return mesh;
    }*/
}
