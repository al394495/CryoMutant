using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    public class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new VerticesNotFound());
            AddComponent(entity, new StartChunck());
            AddBuffer<VerticesEnemies>(entity);
        }
    }
}

public struct VerticesNotFound : IComponentData, IEnableableComponent
{

}

public struct StartChunck : IComponentData
{
    public Entity startChunckEntity;
}

[InternalBufferCapacity(1225)]
public struct VerticesEnemies : IBufferElementData
{
    public float3 value;
}
