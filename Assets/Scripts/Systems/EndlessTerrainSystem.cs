using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial struct EndlessTerrainSystem : ISystem
{
    NativeParallelHashMap<float2, Entity> diccionary;
    NativeParallelHashMap<float2, Entity> quadrantDiccionary;
    //EntitiesReferences entitiesReferences;
    //MapGeneratorData mapGenerator;
    Help help;
    int chunckSize;
    int chunckVisibleInViewDistance;
    float maxViewDst;
    float3 previousPosition;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        diccionary.Dispose();
        diccionary = new NativeParallelHashMap<float2, Entity>(10000, Allocator.Persistent);
        quadrantDiccionary = new NativeParallelHashMap<float2, Entity>(10000, Allocator.Persistent);
        //entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        //mapGenerator = SystemAPI.GetSingleton<MapGeneratorData>();
        //help = SystemAPI.GetSingleton<Help>();
        chunckSize = 8;

        maxViewDst = 240;

        chunckVisibleInViewDistance = (int)math.round(maxViewDst / chunckSize);


    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        help = SystemAPI.GetSingleton<Help>();
        Aux aux = SystemAPI.GetSingleton<Aux>();
        LocalTransform viewerPosition = SystemAPI.GetComponent<LocalTransform>(help.help);
        ecb.SetComponent<LocalTransform>(help.help, new LocalTransform { Position = new float3(viewerPosition.Position.x + 0.01f, 0 , viewerPosition.Position.z + 0.01f), Rotation = viewerPosition.Rotation, Scale = viewerPosition.Scale });

        if (math.distancesq(previousPosition, viewerPosition.Position) > 100f)
        {
            previousPosition = viewerPosition.Position;

            int currentChunckCoordX = (int)math.round(viewerPosition.Position.x / chunckSize);
            int currentChunckCoordY = (int)math.round(viewerPosition.Position.z / chunckSize);

            for (int yOffset = -chunckVisibleInViewDistance; yOffset <= chunckVisibleInViewDistance; yOffset++)
            {
                for (int xOffset = -chunckVisibleInViewDistance; xOffset <= chunckVisibleInViewDistance; xOffset++)
                {
                    float2 viewedChunckCoord = new float2(currentChunckCoordX + xOffset, currentChunckCoordY + yOffset);

                    if (!diccionary.ContainsKey(viewedChunckCoord))
                    {
                        Entity inst = ecb.Instantiate(aux.auxEntity);
                        ecb.SetComponent<MeshData>(inst, new MeshData
                        {
                            onHeightMapGenerated = false,
                            onMeshGenerated = false,
                            coord = viewedChunckCoord * chunckSize,
                            size = chunckSize
                        });
                        ecb.SetComponentEnabled<VericesNotCreated>(inst, true);

                        diccionary.Add(viewedChunckCoord, inst);
                    }
                }
            }
        }

        ecb.Playback(state.EntityManager);
    }


}
