using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor.TerrainTools;
using JetBrains.Annotations;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Unity.Transforms;

partial struct GenerateMeshSystem : ISystem
{

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {/*
        //Real code
        NativeQueue<RefRW<MeshData>> meshDataQueue = new NativeQueue<RefRW<MeshData>>(Allocator.Temp);

        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (RefRW<MeshData> meshData in SystemAPI.Query<RefRW<MeshData>>())
        {
            if (meshData.ValueRO.onHeightMapGenerated && !meshData.ValueRO.onMeshGenerated)
            {
                DynamicBuffer<VerticeFloat3Buffer> verticesBuffer = SystemAPI.GetBuffer<VerticeFloat3Buffer>(meshData.ValueRO.myEntity);
                DynamicBuffer<TriangleIntBuffer> trianglesBuffer = SystemAPI.GetBuffer<TriangleIntBuffer>(meshData.ValueRO.myEntity);
                DynamicBuffer<UvFloat2Buffer> uvsBuffer = SystemAPI.GetBuffer<UvFloat2Buffer>(meshData.ValueRO.myEntity);

                Vector3[] vertices = new Vector3[verticesBuffer.Length];
                Vector2[] uvs = new Vector2[uvsBuffer.Length];
                int[] triangles = new int[trianglesBuffer.Length];

                Mesh mesh = new Mesh();

                for (int i = 0; i < verticesBuffer.Length; i++)
                {
                    vertices[i] = verticesBuffer[i].value;
                    uvs[i] = uvsBuffer[i].value;

                    meshData.ValueRW.min = math.min(verticesBuffer[i].value, meshData.ValueRW.min);
                    meshData.ValueRW.max = math.max(verticesBuffer[i].value, meshData.ValueRW.max);
                }

                for (int i = 0; i < trianglesBuffer.Length; i++)
                {
                    triangles[i] = trianglesBuffer[i].value;
                }
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.uv = uvs;
                mesh.RecalculateNormals();

                meshData.ValueRW.mesh = mesh;

                meshDataQueue.Enqueue(meshData);

                meshData.ValueRW.onMeshGenerated = true;
            }
        }

        while (meshDataQueue.Count > 0)
        {
            RefRW<MeshData> meshData = meshDataQueue.Dequeue();

            RenderMeshDescription desc = new RenderMeshDescription(ShadowCastingMode.On);

            RenderMeshArray meshArray = new RenderMeshArray(
                new Material[] { 
                    entitiesReferences.material 
                }, new Mesh[] {
                    meshData.ValueRO.mesh 
                }, new MaterialMeshIndex[] {
                    new MaterialMeshIndex { MaterialIndex = 0, MeshIndex = 0 } 
                }
                );

            //RenderMeshUtility.AddComponents(meshData.ValueRW.myEntity, state.EntityManager, desc, meshArray, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));

            //RefRW<MeshData> meshData = meshDataQueue.Dequeue();
            float scale = 5f;

            float3 position = new float3(meshData.ValueRO.coord.x * meshData.ValueRO.size, 0f, meshData.ValueRO.coord.y * meshData.ValueRO.size);

            ecb.SetSharedComponentManaged<RenderMeshArray>(meshData.ValueRW.myEntity, meshArray);
            ecb.AddComponent(meshData.ValueRW.myEntity, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
            ecb.AddComponent(meshData.ValueRW.myEntity, new RenderBounds { Value = new AABB { Center = (meshData.ValueRO.max + meshData.ValueRO.min) * 0.5f, Extents = (meshData.ValueRO.max - meshData.ValueRO.min) * 0.5f } });
            ecb.AddComponent(meshData.ValueRW.myEntity, new WorldRenderBounds { Value = new AABB { Center = (meshData.ValueRO.max + meshData.ValueRO.min) * 0.5f, Extents = (meshData.ValueRO.max - meshData.ValueRO.min) * 0.5f } });
            ecb.AddComponent(meshData.ValueRW.myEntity, new LocalTransform { Position = position * scale, Rotation = quaternion.identity, Scale = scale });
            ecb.AddComponent<LocalToWorld>(meshData.ValueRW.myEntity);

        }

        ecb.Playback(state.EntityManager);

        */








        //Testing code
        /*Entity myEntity = new Entity();
        //var desc = new RenderMeshDescription(ShadowCastingMode.On);
        RenderMeshArray myRenderMeshArray = new RenderMeshArray();

        foreach (RefRW<MeshData> meshData in SystemAPI.Query<RefRW<MeshData>>())
        {
            if (meshData.ValueRO.onHeightMapGenerated && !meshData.ValueRO.onMeshGenerated)
            {
                EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

                DynamicBuffer<VerticeFloat3Buffer> verticesBuffer = SystemAPI.GetBuffer<VerticeFloat3Buffer>(meshData.ValueRO.myEntity);
                DynamicBuffer<TriangleIntBuffer> trianglesBuffer = SystemAPI.GetBuffer<TriangleIntBuffer>(meshData.ValueRO.myEntity);
                DynamicBuffer<UvFloat2Buffer> uvsBuffer = SystemAPI.GetBuffer<UvFloat2Buffer>(meshData.ValueRO.myEntity);
                Mesh mesh = new Mesh();
                Vector3[] vertices = new Vector3[verticesBuffer.Length];
                Vector2[] uvs = new Vector2[uvsBuffer.Length];
                int[] triangles = new int[trianglesBuffer.Length];

                for (int i = 0; i < verticesBuffer.Length; i++)
                {
                    vertices[i] = verticesBuffer[i].value;
                    uvs[i] = uvsBuffer[i].value;
                }

                for (int i = 0; i < trianglesBuffer.Length; i++)
                {
                    triangles[i] = trianglesBuffer[i].value;
                }
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.uv = uvs;
                mesh.RecalculateNormals();

                

                RenderMeshArray meshArray = new RenderMeshArray(new Material[] { entitiesReferences.material, entitiesReferences.material }, new Mesh[] { mesh, mesh }, new MaterialMeshIndex[] { new MaterialMeshIndex { MaterialIndex = 0, MeshIndex = 0 }, new MaterialMeshIndex { MaterialIndex = 1, MeshIndex = 1 } });

                //var desc = new RenderMeshDescription(ShadowCastingMode.Off);
                myRenderMeshArray = meshArray;
                myEntity = meshData.ValueRW.myEntity;
                //RenderMeshUtility.AddComponents(meshData.ValueRO.myEntity, state.EntityManager, desc, meshArray, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));

                //ecb.SetComponent<MaterialMeshInfo>(meshData.ValueRO.myEntity, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));

                //state.EntityManager.SetSharedComponent<RenderMeshArray>(new RenderMeshArray());

                //ecb.SetSharedComponentManaged<MeshArrayHolder>(meshData.ValueRO.myEntity, new MeshArrayHolder { renderMeshArray = new RenderMeshArray(new Material[] { entitiesReferences.material, entitiesReferences.material }, new Mesh[] { mesh, mesh }, new MaterialMeshIndex[] { new MaterialMeshIndex { MaterialIndex = 0, MeshIndex = 0 }, new MaterialMeshIndex { MaterialIndex = 1, MeshIndex = 1 } }) });
                //ecb.SetSharedComponentManaged<RenderMeshArray>(meshData.ValueRW.myEntity, new RenderMeshArray(new Material[] { entitiesReferences.material, entitiesReferences.material }, new Mesh[] { mesh, mesh }, new MaterialMeshIndex[] { new MaterialMeshIndex { MaterialIndex = 0, MeshIndex = 0 }, new MaterialMeshIndex { MaterialIndex = 1, MeshIndex = 1 } }));
                //ecb.SetComponent<MaterialMeshInfo>(meshData.ValueRO.myEntity, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
                //ecb.SetComponent<RenderMeshArray>(meshData.ValueRO.myEntity, new RenderMeshArray(new Material[] { entitiesReferences.material }, new Mesh[] { mesh }));
                //ecb.SetComponent<MeshFilter>(meshData.ValueRO.myEntity, new MeshFilter());

                //state.EntityManager.Instantiate(meshData.ValueRO.myEntity);
                //Debug.Log("hola");

                //ecb.SetComponent<RenderMeshArray>(meshData.ValueRO.myEntity, new RenderMeshArray {  });

                meshData.ValueRW.onMeshGenerated = true;
            }
        }
        
        RenderMeshUtility.AddComponents(myEntity, state.EntityManager, desc, myRenderMeshArray, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));

        */
    }
}
