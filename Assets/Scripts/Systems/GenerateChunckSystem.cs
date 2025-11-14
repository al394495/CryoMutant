using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using Unity.Transforms;
using Unity.Physics;

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

        int countLimit = 0;

        foreach ((RefRW<CoordInfo> coordInfo, EnabledRefRO<MeshNotCreated> created, Entity entity) in SystemAPI.Query<RefRW<CoordInfo>, EnabledRefRO<MeshNotCreated>>().WithEntityAccess())
        {
            if (countLimit < 5)
            {
                DynamicBuffer<VerticeFloat3Buffer> verticesBuffer = SystemAPI.GetBuffer<VerticeFloat3Buffer>(entity);
                DynamicBuffer<UvFloat2Buffer> uvsBuffer = SystemAPI.GetBuffer<UvFloat2Buffer>(entity);
                DynamicBuffer<TriangleIntBuffer> trianglesBuffer = SystemAPI.GetBuffer<TriangleIntBuffer>(entity);
                DynamicBuffer<NormalFloat3Buffer> normalsBuffer = SystemAPI.GetBuffer<NormalFloat3Buffer>(entity);

                NativeArray<float3> verticesNativeArray = new NativeArray<float3>(verticesBuffer.Length, Allocator.Temp);
                NativeArray<int3> trianglesNativeArray = new NativeArray<int3>(trianglesBuffer.Length, Allocator.Temp);

                Vector3[] vertices = new Vector3[verticesBuffer.Length];
                Vector2[] uvs = new Vector2[uvsBuffer.Length];
                Vector3[] normals = new Vector3[normalsBuffer.Length];
                int[] triangles = new int[trianglesBuffer.Length];

                //Debug.Log("Tengo tantos vertices " + vertices.Length);

                Mesh mesh = new Mesh();

                float3 min = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
                float3 max = new float3(float.MinValue, float.MinValue, float.MinValue);

                for (int i = 0; i < verticesBuffer.Length; i++)
                {
                    vertices[i] = verticesBuffer[i].value;
                    uvs[i] = uvsBuffer[i].value;

                    normals[i] = normalsBuffer[i].value;

                    verticesNativeArray[i] = verticesBuffer[i].value;

                    min = math.min(verticesBuffer[i].value, min);
                    max = math.max(verticesBuffer[i].value, max);
                }

                int count = 0;
                int triangleCount = 0;

                for (int i = 0; i < trianglesBuffer.Length; i++)
                {
                    triangles[i] = trianglesBuffer[i].value;

                    if ((count + 1) % 3 == 0)
                    {
                        trianglesNativeArray[triangleCount] = new int3(trianglesBuffer[i - 2].value, trianglesBuffer[i - 1].value, trianglesBuffer[i].value);

                        triangleCount++;
                    }

                    count++;
                }

                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.uv = uvs;
                mesh.normals = normals;

                RenderMeshArray meshArray = new RenderMeshArray(
                    new UnityEngine.Material[] {
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

                float scale = 10f;

                float3 position = new float3(coord.x, 0f, coord.y);

                ecb.SetComponent(entity, new LocalTransform { Position = position * scale, Rotation = quaternion.identity, Scale = scale });
                ecb.SetSharedComponentManaged<RenderMeshArray>(entity, meshArray);
                ecb.SetComponent(entity, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
                ecb.SetComponent(entity, new RenderBounds { Value = new AABB { Center = (max + min) * 0.5f, Extents = (max - min) * 0.5f } });

                ecb.SetComponentEnabled<MeshNotCreated>(entity, false);

                if (vertices.Length == 81)
                {
                    ecb.SetComponentEnabled<DecorationsNotCreated>(entity, true);


                    ecb.SetComponent(entity, new PhysicsCollider { Value = Unity.Physics.MeshCollider.Create(verticesNativeArray, trianglesNativeArray) });

                }
            }
            else if(countLimit >= 5)
            {
                break;
            }

            countLimit++;

            //meshData.ValueRW.onMeshGenerated = true;
        }

        ecb.Playback(state.EntityManager);


        EntityCommandBuffer ecb3 = new EntityCommandBuffer(Allocator.TempJob);

        GenerateDecorationsJob generateDecorationsJob = new GenerateDecorationsJob
        {
            mapGeneratorDataJob = mapGeneratorData,
            entitiesReferences = entitiesReferences,
            ecb = ecb3.AsParallelWriter(),
        };

        generateDecorationsJob.ScheduleParallel(state.Dependency).Complete();

        ecb3.Playback(state.EntityManager);

        ecb3.Dispose();

    }
}

[BurstCompile]
public partial struct GenerateDataJob : IJobEntity
{
    public MapGeneratorData mapGeneratorDataJob;
    public EntitiesReferences entitiesReferences;

    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute([EntityIndexInQuery] int entityInQueryIndex, ref MeshData meshData, ref ChildContainer childContainer, EnabledRefRO<VericesNotCreated> created, Entity entity)
    {

        NativeArray<float> noiseMap = new NativeArray<float>((mapGeneratorDataJob.mapSize + 8) * (mapGeneratorDataJob.mapSize + 8), Allocator.Temp);

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

        float halfSize = (mapGeneratorDataJob.mapSize + 8) / 2f;

        for (int y = 0; y < (mapGeneratorDataJob.mapSize + 8); y++)
        {
            for (int x = 0; x < (mapGeneratorDataJob.mapSize + 8); x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < mapGeneratorDataJob.octaves; i++)
                {
                    float sampleX = (x - halfSize + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfSize + octaveOffsets[i].y) / scale * frequency;


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

                noiseMap[(mapGeneratorDataJob.mapSize + 8) * y + x] = noiseHeight;
            }
        }

        for (int y = 0; y < (mapGeneratorDataJob.mapSize + 8); y++)
        {
            for (int x = 0; x < (mapGeneratorDataJob.mapSize + 8); x++)
            {
                float normalizedHeight = (noiseMap[(mapGeneratorDataJob.mapSize + 8) * y + x] + 1) / (maxPossibleHeight / 0.9f);
                noiseMap[(mapGeneratorDataJob.mapSize + 8) * y + x] = math.clamp(normalizedHeight, 0, int.MaxValue);
            }
        }

        //Create the mesh data with the height map created

        Entity entity1 = childContainer.child1;
        Entity entity2 = childContainer.child2;
        Entity entity3 = childContainer.child3;

        //ecb.AddComponent(entityInQueryIndex, entity1, new Parent { Value = entity });
        //ecb.AddComponent(entityInQueryIndex, entity2, new Parent { Value = entity });
        //ecb.AddComponent(entityInQueryIndex, entity3, new Parent { Value = entity });

        ecb.SetComponent(entityInQueryIndex, entity, new MeshLODGroupComponent { LODDistances0 = new float4(600f, 1300f, 2500f, 0f), LocalReferencePoint = new float3(meshData.coord.x * 10, 0f, meshData.coord.y * 10) });
        ecb.SetComponent(entityInQueryIndex, entity1, new MeshLODComponent { Group = entity, ParentGroup = entity, LODMask = 1 });
        ecb.SetComponent(entityInQueryIndex, entity2, new MeshLODComponent { Group = entity, ParentGroup = entity, LODMask = 2 });
        ecb.SetComponent(entityInQueryIndex, entity3, new MeshLODComponent { Group = entity, ParentGroup = entity, LODMask = 4 });

        float topLeftX = (mapGeneratorDataJob.mapSize - 1) / -2f;
        float topLeftZ = (mapGeneratorDataJob.mapSize - 1) / 2f;

        int currentLod = 1;

        NativeArray<Entity> kids = new NativeArray<Entity>(3, Allocator.Temp);
        kids[0] = entity1;
        kids[1] = entity2;
        kids[2] = entity3;

        NativeArray<int> capacity = new NativeArray<int>(3, Allocator.Temp);
        capacity[0] = 81;
        capacity[1] = 25;
        capacity[2] = 9;

        NativeArray<int> capacityBorder = new NativeArray<int>(3, Allocator.Temp);
        capacityBorder[0] = 121;
        capacityBorder[1] = 49;
        capacityBorder[2] = 25;

        NativeArray<int> borderIndexArray = new NativeArray<int>(3, Allocator.Temp);
        borderIndexArray[0] = 3;
        borderIndexArray[1] = 2;
        borderIndexArray[2] = 0;

        for (int i = 0; i < 3; i++)
        {
            int vertexIndex = 0;
            int triangleIndex = 0;

            int vertexIndexBorder = 0;

            Entity currentKid = kids[i];
            int currentCapacity = capacity[i];

            int currentCapacityBorder = capacityBorder[i];
            int borderIndex = borderIndexArray[i];

            float topLeftBorderX = (mapGeneratorDataJob.mapSize - 1 + currentLod * 2) / -2f;
            float topLeftBorderZ = (mapGeneratorDataJob.mapSize - 1 + currentLod * 2) / 2f;

            NativeArray<VerticeFloat3Buffer> verticesArray = new NativeArray<VerticeFloat3Buffer>(currentCapacity, Allocator.Temp);
            NativeArray<UvFloat2Buffer> uvsArray = new NativeArray<UvFloat2Buffer>(currentCapacity, Allocator.Temp);
            NativeArray<TriangleIntBuffer> trianglesArray = new NativeArray<TriangleIntBuffer>(currentCapacity * 6, Allocator.Temp);
            NativeArray<NormalFloat3Buffer> normalsArray = new NativeArray<NormalFloat3Buffer> (currentCapacity, Allocator.Temp);

            NativeArray<float3> verticesBorderArray = new NativeArray<float3>(currentCapacityBorder, Allocator.Temp);

            for (int y = 0; y < mapGeneratorDataJob.mapSize + 8; y += currentLod)
            {
                for(int x = 0; x < mapGeneratorDataJob.mapSize + 8; x += currentLod)
                {
                    if (x >= borderIndex && y >= borderIndex && x <= mapGeneratorDataJob.mapSize + 7 - borderIndex && y <= mapGeneratorDataJob.mapSize + 7 - borderIndex )
                    {
                        verticesBorderArray[vertexIndexBorder] = new float3(topLeftBorderX + x - borderIndex, (math.pow(noiseMap[(mapGeneratorDataJob.mapSize + 8) * y + x], 3)) * 150f, topLeftBorderZ - y + borderIndex);

                        vertexIndexBorder++;
                    }

                    if (x > 3 && y > 3 && x < mapGeneratorDataJob.mapSize + 4 && y < mapGeneratorDataJob.mapSize + 4)
                    {
                        verticesArray[vertexIndex] = new VerticeFloat3Buffer { value = new float3(topLeftX + x - 4, (math.pow(noiseMap[(mapGeneratorDataJob.mapSize + 8) * y + x], 3)) * 150f, topLeftZ - y + 4) };
                        uvsArray[vertexIndex] = new UvFloat2Buffer { value = new float2(((x - 4) / (float)(mapGeneratorDataJob.mapSize - 1)), ( 1 - (y - 4) / (float)(mapGeneratorDataJob.mapSize - 1))) };

                        if (x < mapGeneratorDataJob.mapSize + 3  && y < mapGeneratorDataJob.mapSize + 3)
                        {
                            //First Triangle
                            trianglesArray[triangleIndex] = (new TriangleIntBuffer { value = vertexIndex });
                            trianglesArray[triangleIndex + 1] = (new TriangleIntBuffer { value = vertexIndex + (mapGeneratorDataJob.mapSize - 1) / (currentLod) + 2 });
                            trianglesArray[triangleIndex + 2] = (new TriangleIntBuffer { value = vertexIndex + (mapGeneratorDataJob.mapSize - 1) / (currentLod) + 1 });

                            //Second Triangle
                            trianglesArray[triangleIndex + 3] = (new TriangleIntBuffer { value = vertexIndex + (mapGeneratorDataJob.mapSize - 1) / (currentLod) + 2 });
                            trianglesArray[triangleIndex + 4] = (new TriangleIntBuffer { value = vertexIndex });
                            trianglesArray[triangleIndex + 5] = (new TriangleIntBuffer { value = vertexIndex + 1 });

                            triangleIndex += 6;
                        }

                        vertexIndex++;
                    }
                }
            }

            int diference = (mapGeneratorDataJob.mapSize - 1) / currentLod + 2;

            for (int y = 0; y < (mapGeneratorDataJob.mapSize - 1) / currentLod + 2; y++)
            {
                for (int x = 0; x < (mapGeneratorDataJob.mapSize - 1) / currentLod + 2; x++)
                {
                    int size = ((mapGeneratorDataJob.mapSize - 1) / currentLod + 3);
                    int index = size * y + x;

                    //First Triangle
                    float3 pointA = verticesBorderArray[index];
                    float3 pointB = verticesBorderArray[index + size + 1];
                    float3 pointC = verticesBorderArray[index + size];

                    float3 vectorAB = pointB - pointA;
                    float3 vectorAC = pointC - pointA;

                    float3 triangleNormal = math.cross(vectorAB, vectorAC);

                    if (x > 0 && x < size - 1 && y > 0 && y < size - 1)
                    {
                        normalsArray[index - diference] = new NormalFloat3Buffer { value = triangleNormal + normalsArray[index - diference].value };
                    }

                    if(x + 1 > 0 && x + 1 < size - 1 && y + 1 > 0 && y + 1 < size - 1)
                    {
                        normalsArray[index + size + 1 - (diference + 2)] = new NormalFloat3Buffer { value = triangleNormal + normalsArray[index + size + 1 - (diference + 2)].value };
                    }

                    if (x > 0 && x < size - 1 && y + 1 > 0 && y + 1 < size - 1)
                    {
                        normalsArray[index + size - (diference + 2)] = new NormalFloat3Buffer { value = triangleNormal + normalsArray[index + size - (diference + 2)].value };
                    }

                    //Second Triangle
                    pointA = verticesBorderArray[index + size + 1];
                    pointB = verticesBorderArray[index];
                    pointC = verticesBorderArray[index + 1];

                    vectorAB = pointB - pointA;
                    vectorAC = pointC - pointA;

                    triangleNormal = math.cross(vectorAB, vectorAC);

                    if (x + 1 > 0 && x + 1 < size - 1 && y + 1 > 0 && y + 1 < size - 1)
                    {
                        normalsArray[index + size + 1 - (diference + 2)] = new NormalFloat3Buffer { value = triangleNormal + normalsArray[index + size + 1 - (diference + 2)].value };
                    }

                    if (x > 0 && x < size - 1 && y > 0 && y < size - 1)
                    {
                        normalsArray[index - diference] = new NormalFloat3Buffer { value = triangleNormal + normalsArray[index - diference].value };
                    }

                    if (x + 1 > 0 && x + 1 < size - 1 && y > 0 && y < size - 1)
                    {
                        normalsArray[index + 1 - diference] = new NormalFloat3Buffer { value = triangleNormal + normalsArray[index + 1 - diference].value };
                    }


                }

                diference += 2;
            }

            for (int j = 0; j < normalsArray.Length; j++)
            {
                normalsArray[j] = new NormalFloat3Buffer { value = math.normalize(normalsArray[j].value) };
            }

            ecb.SetBuffer<VerticeFloat3Buffer>(entityInQueryIndex, currentKid).CopyFrom(verticesArray);
            ecb.SetBuffer<UvFloat2Buffer>(entityInQueryIndex, currentKid).CopyFrom(uvsArray);
            ecb.SetBuffer<TriangleIntBuffer>(entityInQueryIndex, currentKid).CopyFrom(trianglesArray);
            ecb.SetBuffer<NormalFloat3Buffer>(entityInQueryIndex, currentKid).CopyFrom(normalsArray);

            ecb.SetComponentEnabled<MeshNotCreated>(entityInQueryIndex, currentKid, true);

            ecb.SetComponent(entityInQueryIndex, currentKid, new CoordInfo { coord = meshData.coord, size = meshData.size });

            currentLod *= 2;
        }

        ecb.SetComponentEnabled<VericesNotCreated>(entityInQueryIndex, entity, false);
        
    }
}

[BurstCompile]
public partial struct GenerateDecorationsJob : IJobEntity
{
    public MapGeneratorData mapGeneratorDataJob;
    public EntitiesReferences entitiesReferences;
    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute([EntityIndexInQuery] int entityInQueryIndex, ref DynamicBuffer<VerticeFloat3Buffer> verticesBuffer, ref CoordInfo coordInfo, EnabledRefRO<DecorationsNotCreated> created, Entity entity)
    {
        NativeList<float3> treesPositions = new NativeList<float3>(Allocator.Temp);

        NativeList<float3> posiblePositions = new NativeList<float3>(Allocator.Temp);

        Unity.Mathematics.Random prng = new Unity.Mathematics.Random((uint) entityInQueryIndex + 1);

        for (int y = 2; y < mapGeneratorDataJob.mapSize - 2; y++)
        {
            for (int x = 2; x < mapGeneratorDataJob.mapSize - 2; x++)
            {
                if (verticesBuffer[mapGeneratorDataJob.mapSize * y + x].value.y > 3f)
                {
                    posiblePositions.Add(verticesBuffer[mapGeneratorDataJob.mapSize * y + x].value);
                }
            }
        }

        while(!posiblePositions.IsEmpty)
        {
            int random = prng.NextInt(0, posiblePositions.Length);
            bool plantTree = true;

            for (int i = 0; i < treesPositions.Length; i++)
            {

                if (math.distancesq(treesPositions[i], posiblePositions[random]) < 20f)
                {
                    plantTree = false;
                }

            }

            if (plantTree)
            {
                float3 vertexPosition = posiblePositions[random];
                float3 position = new float3(coordInfo.coord.x + vertexPosition.x + prng.NextFloat(-0.5f, 0.5f), vertexPosition.y - 0.4f, coordInfo.coord.y + vertexPosition.z + prng.NextFloat(-0.5f, 0.5f));
                treesPositions.Add(vertexPosition);

                Entity tree = ecb.Instantiate(entityInQueryIndex, entitiesReferences.treePrefabEntity);
                ecb.SetComponent(entityInQueryIndex, tree, new LocalTransform { Position = position * 10f, Rotation = quaternion.identity, Scale = 10f });
                ecb.SetComponent(entityInQueryIndex, tree, new MeshLODGroupComponent { LODDistances0 = new float4(600f, 1300f, 2500f, 0f), LocalReferencePoint = new float3(0, 0, 0) });
            }
                

            posiblePositions.RemoveAt(random);
        }


        ecb.SetComponentEnabled<DecorationsNotCreated>(entityInQueryIndex, entity, false);
    }

}
