using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

partial struct GenerateHeightMapSystem : ISystem
{
    /*
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        MapGeneratorData mapGeneratorData = SystemAPI.GetSingleton<MapGeneratorData>();

        foreach (RefRW<MeshData> meshData  in SystemAPI.Query<RefRW<MeshData>>())
        {
            if (!meshData.ValueRO.onHeightMapGenerated)
            {

                GenerateNoiseMap(mapGeneratorData, meshData, ref state);
 
                meshData.ValueRW.onHeightMapGenerated = true;
            }
        }

    }

    [BurstCompile]
    public void GenerateNoiseMap(MapGeneratorData mapGeneratorData, RefRW<MeshData> meshData, ref SystemState state)
    {
        //MapGeneratorData mapGeneratorData = SystemAPI.GetSingleton<MapGeneratorData>();

        NativeArray<float> noiseMap = new NativeArray<float>(mapGeneratorData.mapWidth * mapGeneratorData.mapHeight, Allocator.Temp);

        float scale = mapGeneratorData.noiseScale;
        Unity.Mathematics.Random prng = new Unity.Mathematics.Random((uint)mapGeneratorData.seed);
        NativeArray<float2> octaveOffsets = new NativeArray<float2>(mapGeneratorData.octaves, Allocator.Temp);
        for (int i = 0; i < mapGeneratorData.octaves; i++)
        {
            float offsetX = prng.NextFloat(-100000, 100000) + meshData.ValueRO.coord.x;
            float offsetY = prng.NextFloat(-100000, 100000) - meshData.ValueRO.coord.y;
            octaveOffsets[i] = new float2(offsetX, offsetY);
        }


        if (scale <= 0)
        {
            scale = 0.00001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapGeneratorData.mapWidth / 2f;
        float halfHeight = mapGeneratorData.mapHeight / 2f;

        for (int y = 0; y < mapGeneratorData.mapHeight; y++)
        {
            for (int x = 0; x < mapGeneratorData.mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < mapGeneratorData.octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;


                    float perlinValue = ((noise.cnoise(new float2(sampleX, sampleY)) + 1f) * 0.5f )* 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= mapGeneratorData.persistance;
                    frequency *= mapGeneratorData.lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[mapGeneratorData.mapHeight * y + x] = noiseHeight;
            }
        }

        for (int y = 0; y < mapGeneratorData.mapHeight; y++)
        {
            for (int x = 0; x < mapGeneratorData.mapWidth; x++)
            {
                noiseMap[mapGeneratorData.mapHeight * y + x] = math.unlerp(minNoiseHeight, maxNoiseHeight, noiseMap[mapGeneratorData.mapHeight * y + x]);
            }
        }

        //Create the mesh data with the height map created

        float topLeftX = (mapGeneratorData.mapWidth - 1) / -2f;
        float topLeftZ = (mapGeneratorData.mapHeight - 1) / 2f;

        int vertexIndex = 0;

        DynamicBuffer<VerticeFloat3Buffer> verticesBuffer = SystemAPI.GetBuffer<VerticeFloat3Buffer>(meshData.ValueRO.myEntity);
        DynamicBuffer<TriangleIntBuffer> trianglesBuffer = SystemAPI.GetBuffer<TriangleIntBuffer>(meshData.ValueRO.myEntity);
        DynamicBuffer<UvFloat2Buffer> uvsBuffer = SystemAPI.GetBuffer<UvFloat2Buffer>(meshData.ValueRO.myEntity);

        for (int y = 0; y < mapGeneratorData.mapHeight; y++)
        {
            for(int x = 0; x < mapGeneratorData.mapWidth; x++)
            {
                verticesBuffer.Add(new VerticeFloat3Buffer { value = new float3(topLeftX + x, noiseMap[mapGeneratorData.mapHeight * y + x], topLeftZ - y) });
                uvsBuffer.Add(new UvFloat2Buffer { value = new float2(x / (float)mapGeneratorData.mapWidth, y / (float)mapGeneratorData.mapHeight)});

                if (x < mapGeneratorData.mapWidth - 1 && y < mapGeneratorData.mapHeight - 1)
                {
                    //First Triangle
                    trianglesBuffer.Add(new TriangleIntBuffer { value = vertexIndex });
                    trianglesBuffer.Add(new TriangleIntBuffer { value = vertexIndex + mapGeneratorData.mapWidth + 1 });
                    trianglesBuffer.Add(new TriangleIntBuffer { value = vertexIndex + mapGeneratorData.mapWidth });

                    //Second Triangle
                    trianglesBuffer.Add(new TriangleIntBuffer { value = vertexIndex + mapGeneratorData.mapWidth + 1 });
                    trianglesBuffer.Add(new TriangleIntBuffer { value = vertexIndex });
                    trianglesBuffer.Add(new TriangleIntBuffer { value = vertexIndex + 1 });
                }

                vertexIndex++;
            }
        }

    }*/
}
