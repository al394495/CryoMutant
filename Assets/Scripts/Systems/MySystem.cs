using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

partial struct MySystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        //MapGeneratorData mapGeneratorData = SystemAPI.GetSingleton<MapGeneratorData>();

        foreach (RefRW<MeshData> meshData  in SystemAPI.Query<RefRW<MeshData>>())
        {
            if (!meshData.ValueRO.onHeightMapGenerated)
            {
                DynamicBuffer<FloatBuffer> floatBuffer = SystemAPI.GetBuffer<FloatBuffer>(meshData.ValueRO.myEntity);

                NativeArray<float> help = GenerateNoiseMap();
                Debug.Log(help.Length);
                for (int i = 0; i < help.Length; i++)
                {
                    floatBuffer.Add(new FloatBuffer { value = help[i] });
                }
                if (floatBuffer.Length > 0)
                {
                    Debug.Log("Tengo el mapa de alturas :D");
                }
                meshData.ValueRW.onHeightMapGenerated = true;
            }
        }

    }

    public NativeArray<float> GenerateNoiseMap()
    {
        MapGeneratorData mapGeneratorData = SystemAPI.GetSingleton<MapGeneratorData>();

        NativeArray<float> noiseMap = new NativeArray<float>(mapGeneratorData.mapWidth * mapGeneratorData.mapHeight, Allocator.Temp);

        float scale = mapGeneratorData.noiseScale;
        Unity.Mathematics.Random prng = new Unity.Mathematics.Random((uint)mapGeneratorData.seed);
        NativeArray<float2> octaveOffsets = new NativeArray<float2>(mapGeneratorData.octaves, Allocator.Temp);
        for (int i = 0; i < mapGeneratorData.octaves; i++)
        {
            float offsetX = prng.NextFloat(-100000, 100000) + mapGeneratorData.offset.x;
            float offsetY = prng.NextFloat(-100000, 100000) + mapGeneratorData.offset.y;
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
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;


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

        return noiseMap;

    }
}
