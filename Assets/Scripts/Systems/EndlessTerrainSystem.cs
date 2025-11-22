using System.Drawing;
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

    int chunckSize;
    int chunckVisibleInViewDistance;
    float maxViewDst;
    float3 previousPosition;

    Entity player;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();

        diccionary.Dispose();
        diccionary = new NativeParallelHashMap<float2, Entity>(10000, Allocator.Persistent);
        quadrantDiccionary = new NativeParallelHashMap<float2, Entity>(10000, Allocator.Persistent);
 
        chunckSize = 8;

        maxViewDst = 160;

        chunckVisibleInViewDistance = (int)math.round(maxViewDst / chunckSize);

        previousPosition = new float3(float.MaxValue, float.MaxValue, float.MaxValue);


    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        player = SystemAPI.GetSingletonEntity<PlayerTag>();

        Aux aux = SystemAPI.GetSingleton<Aux>();
        LocalTransform viewerPosition = SystemAPI.GetComponent<LocalTransform>(player);

        if (math.distancesq(previousPosition, viewerPosition.Position) > 100f)
        {
            previousPosition = viewerPosition.Position;

            int currentChunckCoordX = (int)math.round(viewerPosition.Position.x / chunckSize);
            int currentChunckCoordY = (int)math.round(viewerPosition.Position.z / chunckSize);

            for (int yOffset = -chunckVisibleInViewDistance; yOffset <= chunckVisibleInViewDistance; yOffset++)
            {
                for (int xOffset = -chunckVisibleInViewDistance; xOffset <= chunckVisibleInViewDistance; xOffset++)
                {
                    EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

                    float2 viewedChunckCoord = new float2(currentChunckCoordX + xOffset, currentChunckCoordY + yOffset);

                    if (!diccionary.ContainsKey(viewedChunckCoord))
                    {
                        float2 quadrantCoord = new float2((int)(math.round(viewedChunckCoord.x / 10)), (int)(math.round(viewedChunckCoord.y / 10)));
                        Entity quadrantEntity;

                        if (!quadrantDiccionary.ContainsKey(quadrantCoord))
                        {
                            quadrantEntity = state.EntityManager.Instantiate(aux.quadrantEntity);
                            ecb.AddBuffer<ChunckEntityBuffer>(quadrantEntity);
                            ecb.AddComponent(quadrantEntity, new EnemiesNotCreated());
                            quadrantDiccionary.Add(quadrantCoord, quadrantEntity);
                        }
                        else
                        {
                            quadrantEntity = quadrantDiccionary[quadrantCoord];
                        }

                        Entity inst = state.EntityManager.Instantiate(aux.auxEntity);
                        diccionary.Add(viewedChunckCoord, inst);
                        ecb.SetComponent(inst, new MeshData
                        {
                            onHeightMapGenerated = false,
                            onMeshGenerated = false,
                            coord = viewedChunckCoord * chunckSize,
                            size = chunckSize
                        });


                        Entity entityTopLeft = Entity.Null;
                        Entity entityTop = Entity.Null;
                        Entity entityTopRight = Entity.Null;
                        Entity entityMiddleLeft = Entity.Null;
                        Entity entityMiddleRight = Entity.Null;
                        Entity entityBottomLeft = Entity.Null;
                        Entity entityBottom = Entity.Null;
                        Entity entityBottomRight = Entity.Null;

                        float2 coord = viewedChunckCoord;

                        float2 check = new float2(coord.x - 1, coord.y + 1);
                        if (diccionary.ContainsKey(check))
                        {
                            entityTopLeft = diccionary[check];
                            InfoToQuadrant infoToQuadrant = SystemAPI.GetComponent<InfoToQuadrant>(entityTopLeft);
                            if (infoToQuadrant.entityBottomRight == Entity.Null)
                            {
                                infoToQuadrant.entityBottomRight = inst;
                                ecb.SetComponent(entityTopLeft, infoToQuadrant);
                            }
                        }
                        
                        check = new float2(coord.x, coord.y + 1);
                        if (diccionary.ContainsKey(check))
                        {
                            entityTop = diccionary[check];
                            InfoToQuadrant infoToQuadrant = SystemAPI.GetComponent<InfoToQuadrant>(entityTop);
                            if (infoToQuadrant.entityBottom == Entity.Null)
                            {
                                infoToQuadrant.entityBottom = inst;
                                ecb.SetComponent(entityTop, infoToQuadrant);
                            }
                        }

                        check = new float2(coord.x + 1, coord.y + 1);
                        if (diccionary.ContainsKey(check))
                        {
                            entityTopRight = diccionary[check];
                            InfoToQuadrant infoToQuadrant = SystemAPI.GetComponent<InfoToQuadrant>(entityTopRight);
                            if (infoToQuadrant.entityBottomLeft == Entity.Null)
                            {
                                infoToQuadrant.entityBottomLeft = inst;
                                ecb.SetComponent(entityTopRight, infoToQuadrant);
                            }
                        }

                        check = new float2(coord.x - 1, coord.y);
                        if (diccionary.ContainsKey(check))
                        {
                            entityMiddleLeft = diccionary[check];
                            InfoToQuadrant infoToQuadrant = SystemAPI.GetComponent<InfoToQuadrant>(entityMiddleLeft);
                            if (infoToQuadrant.entityMiddleRight == Entity.Null)
                            {
                                infoToQuadrant.entityMiddleRight = inst;
                                ecb.SetComponent(entityMiddleLeft, infoToQuadrant);
                            }
                        }

                        check = new float2(coord.x + 1, coord.y);
                        if (diccionary.ContainsKey(check))
                        {
                            entityMiddleRight = diccionary[check];
                            InfoToQuadrant infoToQuadrant = SystemAPI.GetComponent<InfoToQuadrant>(entityMiddleRight);
                            if (infoToQuadrant.entityMiddleLeft == Entity.Null)
                            {
                                infoToQuadrant.entityMiddleLeft = inst;
                                ecb.SetComponent(entityMiddleRight, infoToQuadrant);
                            }
                        }

                        check = new float2(coord.x - 1, coord.y - 1);
                        if (diccionary.ContainsKey(check))
                        {
                            entityBottomLeft = diccionary[check];
                            InfoToQuadrant infoToQuadrant = SystemAPI.GetComponent<InfoToQuadrant>(entityBottomLeft);
                            if (infoToQuadrant.entityTopRight == Entity.Null)
                            {
                                infoToQuadrant.entityTopRight = inst;
                                ecb.SetComponent(entityBottomLeft, infoToQuadrant);
                            }
                        }

                        check = new float2(coord.x, coord.y - 1);
                        if (diccionary.ContainsKey(check))
                        {
                            entityBottom = diccionary[check];
                            InfoToQuadrant infoToQuadrant = SystemAPI.GetComponent<InfoToQuadrant>(entityBottom);
                            if (infoToQuadrant.entityTop == Entity.Null)
                            {
                                infoToQuadrant.entityTop = inst;
                                ecb.SetComponent(entityBottom, infoToQuadrant);
                            }
                        }

                        check = new float2(coord.x + 1, coord.y - 1);
                        if (diccionary.ContainsKey(check))
                        {
                            entityBottomRight = diccionary[check];
                            InfoToQuadrant infoToQuadrant = SystemAPI.GetComponent<InfoToQuadrant>(entityBottomRight);
                            if (infoToQuadrant.entityTopLeft == Entity.Null)
                            {
                                infoToQuadrant.entityTopLeft = inst;
                                ecb.SetComponent(entityBottomRight, infoToQuadrant);
                            }
                        }

                        ecb.SetComponent(inst, new InfoToQuadrant
                        {
                            quadrant = quadrantEntity,
                            entityTopLeft = entityTopLeft,
                            entityTop = entityTop,
                            entityTopRight = entityTopRight,
                            entityMiddleLeft = entityMiddleLeft,
                            entityMiddleRight = entityMiddleRight,
                            entityBottomLeft = entityBottomLeft,
                            entityBottom = entityBottom,
                            entityBottomRight = entityBottomRight
                        });


                        ecb.SetComponentEnabled<VerticesNotCreated>(inst, true);

                        ecb.Playback(state.EntityManager);
                    }
                }
            }
        }
    }


}
