using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

partial struct SetEnemiesMapSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
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

    [BurstCompile]
    public void Execute([EntityIndexInQuery] int entityInQueryIndex, in StartChunck startChunck, ref DynamicBuffer<VerticesEnemies> verticesEnemies, Entity entity)
    {
        //Get the top left entity
        Entity topLeft = startChunck.startChunckEntity;

        for (int i = 0; i < 3; i++)
        {
            topLeft = infoToQuadrantLookUp[topLeft].entityMiddleLeft;
        }

        for (int i = 0; i < 3; i++)
        {
            topLeft = infoToQuadrantLookUp[topLeft].entityTop;
        }

        //
    }
}
