using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using Unity.Transforms;

partial struct GenerateChunckSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }


    public void OnUpdate(ref SystemState state)
    {
        MapGeneratorData mapGeneratorData = SystemAPI.GetSingleton<MapGeneratorData>();
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        EntityCommandBuffer ecb2 = new EntityCommandBuffer(Allocator.TempJob);

        GenerateDataJob generateDataJob = new GenerateDataJob
        {
            mapGeneratorDataJob = mapGeneratorData,
            entitiesReferences = entitiesReferences,
            ecb = ecb2.AsParallelWriter()
        };

        generateDataJob.ScheduleParallel(state.Dependency).Complete();

        ecb2.Playback(state.EntityManager);

        ecb2.Dispose();

        foreach ((RefRW<CoordInfo> coordInfo, EnabledRefRO<MeshNotCreated> created, Entity entity) in SystemAPI.Query<RefRW<CoordInfo>, EnabledRefRO<MeshNotCreated>>().WithEntityAccess())
        {
            DynamicBuffer<VerticeFloat3Buffer> verticesBuffer = SystemAPI.GetBuffer<VerticeFloat3Buffer>(entity);
            DynamicBuffer<UvFloat2Buffer> uvsBuffer = SystemAPI.GetBuffer<UvFloat2Buffer>(entity);
            DynamicBuffer<TriangleIntBuffer> trianglesBuffer = SystemAPI.GetBuffer<TriangleIntBuffer>(entity);

            Vector3[] vertices = new Vector3[verticesBuffer.Length];
            Vector2[] uvs = new Vector2[uvsBuffer.Length];
            int[] triangles = new int[trianglesBuffer.Length];

            Mesh mesh = new Mesh();

            float3 min = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            float3 max = new float3(float.MinValue, float.MinValue, float.MinValue);

            for (int i = 0; i < verticesBuffer.Length; i++)
            {
                vertices[i] = verticesBuffer[i].value;
                uvs[i] = uvsBuffer[i].value;

                min = math.min(verticesBuffer[i].value, min);
                max = math.max(verticesBuffer[i].value, max);
            }

            for (int i = 0; i < trianglesBuffer.Length; i++)
            {
                triangles[i] = trianglesBuffer[i].value;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();

            RenderMeshArray meshArray = new RenderMeshArray(
                new Material[] {
                entitiesReferences.material
                }, new Mesh[] {
                mesh,
                }, new MaterialMeshIndex[] {
                new MaterialMeshIndex { MaterialIndex = 0, MeshIndex = 0 }
                }
                );

            float2 coord = coordInfo.ValueRO.coord;
            int size = coordInfo.ValueRO.size;
            //Entity entity = meshData.ValueRW.myEntity;

            float scale = 50f;

            float3 position = new float3(coord.x * size / 8, 0f, coord.y * size / 8);

            ecb.SetSharedComponentManaged<RenderMeshArray>(entity, meshArray);
            ecb.AddComponent(entity, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
            ecb.AddComponent(entity, new RenderBounds { Value = new AABB { Center = (max + min) * 0.5f, Extents = (max - min) * 0.5f } });
            ecb.AddComponent(entity, new WorldRenderBounds { Value = new AABB { Center = (max + min) * 0.5f, Extents = (max - min) * 0.5f } });
            ecb.AddComponent(entity, new LocalTransform { Position = position * scale, Rotation = quaternion.identity, Scale = scale });
            ecb.AddComponent<LocalToWorld>(entity);

            ecb.SetComponentEnabled<MeshNotCreated>(entity, false);

            //meshData.ValueRW.onMeshGenerated = true;
        }

        ecb.Playback(state.EntityManager);

    }
}

[BurstCompile]
public partial struct GenerateDataJob : IJobEntity
{
    public MapGeneratorData mapGeneratorDataJob;
    public EntitiesReferences entitiesReferences;

    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute([EntityIndexInQuery] int entityInQueryIndex, ref MeshData meshData, ref DynamicBuffer<VerticeFloat3Buffer> verticesBuffer, ref DynamicBuffer<UvFloat2Buffer> uvsBuffer, ref DynamicBuffer<TriangleIntBuffer> trianglesBuffer, EnabledRefRO<VericesNotCreated> created)
    {

        NativeArray<float> noiseMap = new NativeArray<float>(mapGeneratorDataJob.mapWidth * mapGeneratorDataJob.mapHeight, Allocator.Temp);

        float scale = mapGeneratorDataJob.noiseScale;
        Unity.Mathematics.Random prng = new Unity.Mathematics.Random((uint)mapGeneratorDataJob.seed);
        NativeArray<float2> octaveOffsets = new NativeArray<float2>(mapGeneratorDataJob.octaves, Allocator.Temp);

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < mapGeneratorDataJob.octaves; i++)
        {
            float offsetX = prng.NextFloat(-100000, 100000) + meshData.coord.x;
            float offsetY = prng.NextFloat(-100000, 100000) - meshData.coord.y;
            octaveOffsets[i] = new float2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= frequency;
        }


        if (scale <= 0)
        {
            scale = 0.00001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapGeneratorDataJob.mapWidth / 2f;
        float halfHeight = mapGeneratorDataJob.mapHeight / 2f;

        for (int y = 0; y < mapGeneratorDataJob.mapHeight; y++)
        {
            for (int x = 0; x < mapGeneratorDataJob.mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < mapGeneratorDataJob.octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;


                    float perlinValue = ((noise.cnoise(new float2(sampleX, sampleY)) + 1f) * 0.5f) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= mapGeneratorDataJob.persistance;
                    frequency *= mapGeneratorDataJob.lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[mapGeneratorDataJob.mapHeight * y + x] = noiseHeight;
            }
        }

        for (int y = 0; y < mapGeneratorDataJob.mapHeight; y++)
        {
            for (int x = 0; x < mapGeneratorDataJob.mapWidth; x++)
            {
                float normalizedHeight = (noiseMap[mapGeneratorDataJob.mapHeight * y + x] + 1) / (maxPossibleHeight / 0.9f);
                noiseMap[mapGeneratorDataJob.mapHeight * y + x] = math.clamp(normalizedHeight, 0, int.MaxValue);
            }
        }

        //Create the mesh data with the height map created

        Entity entity1 = ecb.Instantiate(entityInQueryIndex, entitiesReferences.terrainChunkPrefabEntity);
        Entity entity2 = ecb.Instantiate(entityInQueryIndex, entitiesReferences.terrainChunkPrefabEntity);
        Entity entity3 = ecb.Instantiate(entityInQueryIndex, entitiesReferences.terrainChunkPrefabEntity);

        ecb.AddComponent(entityInQueryIndex, entity1, new Parent { Value = meshData.myEntity });
        ecb.AddComponent(entityInQueryIndex, entity2, new Parent { Value = meshData.myEntity });
        ecb.AddComponent(entityInQueryIndex, entity3, new Parent { Value = meshData.myEntity });

        ecb.AddComponent(entityInQueryIndex, meshData.myEntity, new MeshLODGroupComponent { LODDistances0 = new float4(500f, 2000f, 3000f, 0f), LocalReferencePoint = new float3(meshData.coord.x * 8, 0f, meshData.coord.y * 8) });
        ecb.AddComponent(entityInQueryIndex, entity1, new MeshLODComponent { Group = meshData.myEntity, ParentGroup = meshData.myEntity, LODMask = 1 });
        ecb.AddComponent(entityInQueryIndex, entity2, new MeshLODComponent { Group = meshData.myEntity, ParentGroup = meshData.myEntity, LODMask = 2 });
        ecb.AddComponent(entityInQueryIndex, entity3, new MeshLODComponent { Group = meshData.myEntity, ParentGroup = meshData.myEntity, LODMask = 4 });

        float topLeftX = (mapGeneratorDataJob.mapWidth - 1) / -2f;
        float topLeftZ = (mapGeneratorDataJob.mapHeight - 1) / 2f;

        int currentLod = 1;

        NativeArray<Entity> kids = new NativeArray<Entity>(3, Allocator.Temp);
        kids[0] = entity1;
        kids[1] = entity2;
        kids[2] = entity3;

        NativeArray<int> capacity = new NativeArray<int>(3, Allocator.Temp);
        capacity[0] = 81;
        capacity[1] = 25;
        capacity[2] = 9;

        for (int i = 0; i < 3; i++)
        {
            int vertexIndex = 0;
            int triangleIndex = 0;

            Entity currentKid = kids[i];
            int currentCapacity = capacity[i];

            NativeArray<VerticeFloat3Buffer> verticesArray = new NativeArray<VerticeFloat3Buffer>(currentCapacity, Allocator.Temp);
            NativeArray<UvFloat2Buffer> uvsArray = new NativeArray<UvFloat2Buffer>(currentCapacity, Allocator.Temp);
            NativeArray<TriangleIntBuffer> trianglesArray = new NativeArray<TriangleIntBuffer>(currentCapacity * 6, Allocator.Temp);

            for (int y = 0; y < mapGeneratorDataJob.mapHeight; y += currentLod)
            {
                for(int x = 0; x < mapGeneratorDataJob.mapWidth; x += currentLod)
                {
                    verticesArray[vertexIndex] = new VerticeFloat3Buffer { value = new float3(topLeftX + x, (math.pow(noiseMap[mapGeneratorDataJob.mapHeight * y + x], 3)) * 150f, topLeftZ - y )};
                    uvsArray[vertexIndex] = new UvFloat2Buffer { value = new float2((x / (float)mapGeneratorDataJob.mapWidth), (y / (float)mapGeneratorDataJob.mapHeight)) };

                    if (x < mapGeneratorDataJob.mapWidth - 1 && y < mapGeneratorDataJob.mapHeight - 1)
                    {
                        //First Triangle
                        trianglesArray[triangleIndex] = (new TriangleIntBuffer { value = vertexIndex });
                        trianglesArray[triangleIndex + 1] = (new TriangleIntBuffer { value = vertexIndex + (mapGeneratorDataJob.mapWidth - 1) / (currentLod) + 2 });
                        trianglesArray[triangleIndex + 2] = (new TriangleIntBuffer { value = vertexIndex + (mapGeneratorDataJob.mapWidth - 1) / (currentLod) + 1 });

                        //Second Triangle
                        trianglesArray[triangleIndex + 3] = (new TriangleIntBuffer { value = vertexIndex + (mapGeneratorDataJob.mapWidth - 1) / (currentLod) + 2 });
                        trianglesArray[triangleIndex + 4] = (new TriangleIntBuffer { value = vertexIndex });
                        trianglesArray[triangleIndex + 5] = (new TriangleIntBuffer { value = vertexIndex + 1 });

                        triangleIndex += 6;
                    }

                    vertexIndex++;
                }
            }

            ecb.AddBuffer<VerticeFloat3Buffer>(entityInQueryIndex, currentKid).CopyFrom(verticesArray);
            ecb.AddBuffer<UvFloat2Buffer>(entityInQueryIndex, currentKid).CopyFrom(uvsArray);
            ecb.AddBuffer<TriangleIntBuffer>(entityInQueryIndex, currentKid).CopyFrom(trianglesArray);

            ecb.AddComponent(entityInQueryIndex, currentKid, new MeshNotCreated());

            ecb.AddComponent(entityInQueryIndex, currentKid, new CoordInfo { coord = meshData.coord, size = meshData.size });

            currentLod *= 2;
        }

        ecb.SetComponentEnabled<VericesNotCreated>(entityInQueryIndex, meshData.myEntity, false);
        
    }
}
