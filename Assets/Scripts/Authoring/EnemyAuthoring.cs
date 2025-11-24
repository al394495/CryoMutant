using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics;

public class EnemyAuthoring : MonoBehaviour
{
    public float enemyRange;
    public float enemyAttackRange;
    public float enemySpeed;

    public class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new VerticesNotFound());
            AddComponent(entity, new RequestPath());
            SetComponentEnabled<RequestPath>(entity, false);
            AddComponent(entity, new MovingEnemy());
            SetComponentEnabled<MovingEnemy>(entity, false);
            AddComponent(entity, new InRange());
            SetComponentEnabled<InRange>(entity, false);
            AddComponent(entity, new EnemyImportantNodes());
            AddComponent(entity, new StartChunck());
            AddComponent(entity, new EnemyRange { range = authoring.enemyRange, attackRange = authoring.enemyAttackRange });
            AddComponent(entity, new EnemySpeed { speed = authoring.enemySpeed });
            AddComponent(entity, new EnemyTargetPosition { position = float3.zero });
            AddBuffer<VerticesEnemies>(entity);
            AddBuffer<NodesPath>(entity);
        }
    }
}

public struct VerticesNotFound : IComponentData, IEnableableComponent
{

}

public struct RequestPath : IComponentData, IEnableableComponent
{

}

public struct MovingEnemy : IComponentData, IEnableableComponent
{

}

public struct InRange : IComponentData, IEnableableComponent
{

}

public struct StartChunck : IComponentData
{
    public Entity startChunckEntity;
}

public struct EnemyImportantNodes : IComponentData
{
    public int2 originNode;
    public int2 endNode;
    public int2 currentNode;
    public int2 targetNode;
}

public struct EnemyTargetPosition : IComponentData
{
    public Vector3 position;
}

public struct EnemyRange : IComponentData
{
    public float range;
    public float attackRange;
}

public struct EnemySpeed : IComponentData 
{ 
    public float speed;
}

[InternalBufferCapacity(841)]
public struct VerticesEnemies : IBufferElementData
{
    public float3 value;
}

[InternalBufferCapacity(200)]
public struct NodesPath : IBufferElementData
{
    public int2 value;
}
