using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial struct EndlessTerrainSystem : ISystem
{
    NativeParallelHashMap<float2, Entity> diccionary;
    //EntitiesReferences entitiesReferences;
    //MapGeneratorData mapGenerator;
    Help help;
    int chunckSize;
    int chunckVisibleInViewDistance;
    float maxViewDst;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        diccionary = new NativeParallelHashMap<float2, Entity>(1000, Allocator.Persistent);
        //entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        //mapGenerator = SystemAPI.GetSingleton<MapGeneratorData>();
        //help = SystemAPI.GetSingleton<Help>();
        chunckSize = 8;

        maxViewDst = 300;

        chunckVisibleInViewDistance = (int)math.round(maxViewDst / chunckSize);

    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        //Entity viewer = entitiesReferences.example;
        //LocalTransform viewerPosition = SystemAPI.GetComponent<LocalTransform>(viewer);
        help = SystemAPI.GetSingleton<Help>();
        LocalTransform viewerPosition = SystemAPI.GetComponent<LocalTransform>(help.help);
        ecb.SetComponent<LocalTransform>(help.help, new LocalTransform { Position = new float3(viewerPosition.Position.x + 1f, 0 , viewerPosition.Position.z + 1f), Rotation = viewerPosition.Rotation, Scale = viewerPosition.Scale });

        //Debug.Log(viewerPosition.Position.x);
        //Debug.Log(viewerPosition.Position.z);
        int currentChunckCoordX = (int)math.round(viewerPosition.Position.x / chunckSize);
        int currentChunckCoordY = (int)math.round(viewerPosition.Position.z / chunckSize);

        for (int yOffset = -chunckVisibleInViewDistance; yOffset <= chunckVisibleInViewDistance; yOffset++)
        {
            for (int  xOffset = -chunckVisibleInViewDistance; xOffset <= chunckVisibleInViewDistance; xOffset++)
            {
                float2 viewedChunckCoord = new float2(currentChunckCoordX + xOffset, currentChunckCoordY + yOffset);

                if (diccionary.ContainsKey(viewedChunckCoord))
                {

                }
                else
                {
                    Entity inst = ecb.Instantiate(help.help);
                    ecb.AddComponent<MeshData>(inst, new MeshData { 
                        onHeightMapGenerated = false,
                        onMeshGenerated = false,
                        myEntity = inst,
                        coord = viewedChunckCoord * 8,
                        size = chunckSize
                    });
                    ecb.AddBuffer<VerticeFloat3Buffer>(inst);
                    ecb.AddBuffer<TriangleIntBuffer>(inst);
                    ecb.AddBuffer<UvFloat2Buffer>(inst);
                    //Debug.Log("Estoy Creando entidades :D");

                    diccionary.Add(viewedChunckCoord, inst);
                }
            }
        }

        ecb.Playback(state.EntityManager);
    }


}
