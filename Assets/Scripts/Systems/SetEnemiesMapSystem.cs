using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

partial struct SetEnemiesMapSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        ComponentLookup<InfoToQuadrant> infoToQuadrantLookUp = SystemAPI.GetComponentLookup<InfoToQuadrant>();
        ComponentLookup<ChildContainer> childContainerLookUp = SystemAPI.GetComponentLookup<ChildContainer>();
        ComponentLookup<CoordInfo> coordInfoLookUp = SystemAPI.GetComponentLookup<CoordInfo>();
        BufferLookup<VerticeFloat3Buffer> verticesLookUp = SystemAPI.GetBufferLookup<VerticeFloat3Buffer>();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        GetVerticesJob getVerticesJob = new GetVerticesJob
        {
            infoToQuadrantLookUp = infoToQuadrantLookUp,
            childContainerLookUp = childContainerLookUp,
            coordInfoLookUp = coordInfoLookUp,
            verticesLookUp = verticesLookUp,
            ecb = ecb.AsParallelWriter()
        };

        getVerticesJob.ScheduleParallel(state.Dependency).Complete();

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

[BurstCompile]
public partial struct GetVerticesJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<InfoToQuadrant> infoToQuadrantLookUp;
    [ReadOnly] public ComponentLookup<ChildContainer> childContainerLookUp;
    [ReadOnly] public ComponentLookup<CoordInfo> coordInfoLookUp;
    [ReadOnly] public BufferLookup<VerticeFloat3Buffer> verticesLookUp;

    public EntityCommandBuffer.ParallelWriter ecb; 

    [BurstCompile]
    public void Execute([EntityIndexInQuery] int entityInQueryIndex, in StartChunck startChunck, ref DynamicBuffer<VerticesEnemies> verticesEnemies,  EnabledRefRO<VerticesNotFound> vNotFound, Entity entity)
    {
        //Get the top left entity
        Entity topLeft = startChunck.startChunckEntity;

        for (int i = 0; i < 3; i++)
        {
            if (topLeft == Entity.Null)
            {
                return;
            }
            topLeft = infoToQuadrantLookUp[topLeft].entityMiddleLeft;
        }

        for (int i = 0; i < 3; i++)
        {
            if (topLeft == Entity.Null)
            {
                return;
            }
            topLeft = infoToQuadrantLookUp[topLeft].entityTop;
        }

        if (topLeft == Entity.Null) return;

        //Create the grid of vertices
        Entity currentEntity = topLeft;
        if (!childContainerLookUp.HasComponent(currentEntity)) return;
        if (!verticesLookUp.HasBuffer(childContainerLookUp[currentEntity].child2)) return;
        DynamicBuffer<VerticeFloat3Buffer> currentBuffer = verticesLookUp[childContainerLookUp[currentEntity].child2];
        NativeList<VerticesEnemies> verticesEnemiesList = new NativeList<VerticesEnemies>(Allocator.Temp);
        Entity nextRowStart;
        Entity currentRowStart;

        for (int i = 0; i < 7; i++)
        {
            currentRowStart = currentEntity;
            nextRowStart = infoToQuadrantLookUp[currentEntity].entityBottom;
            if (i < 6 && nextRowStart == Entity.Null) return;

            for (int j = 0; j < 5; j++)
            {
                if (j == 0 && i != 0) continue;

                for (int k = 0; k < 7; k++)
                {
                    for (int h = 0; h < 5; h++)
                    {
                        if (h == 0 && k != 0) continue;

                        if (!childContainerLookUp.HasComponent(currentEntity)) return;
                        if (!verticesLookUp.HasBuffer(childContainerLookUp[currentEntity].child2)) return;
                        currentBuffer = verticesLookUp[childContainerLookUp[currentEntity].child2];
                        if (currentBuffer.Length <= 0) return;
                        float3 vertice = currentBuffer[j * 5 + h].value;
                        float2 coord = coordInfoLookUp[childContainerLookUp[currentEntity].child2].coord;
                        vertice = new float3(vertice.x + coord.x, vertice.y, vertice.z + coord.y) * 10f;
                        verticesEnemiesList.Add(new VerticesEnemies { value = vertice });
                    }

                    currentEntity = infoToQuadrantLookUp[currentEntity].entityMiddleRight;
                    if (k < 6 && currentEntity == Entity.Null) return;

                }
                currentEntity = currentRowStart;
            }
            currentEntity = nextRowStart;
        }

        ecb.SetBuffer<VerticesEnemies>(entityInQueryIndex, entity).CopyFrom(verticesEnemiesList.AsArray());
        ecb.SetComponentEnabled<VerticesNotFound>(entityInQueryIndex, entity, false);
        ecb.SetComponentEnabled<MovingEnemy>(entityInQueryIndex, entity, true);
    }
}
