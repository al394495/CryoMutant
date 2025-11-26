using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

partial struct MoveBulletsSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach ((RefRW<BeeBullet> beeBullet, RefRW<LocalTransform> localTransform, Entity entity) in SystemAPI.Query<RefRW<BeeBullet>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            float3 playerLocation = SystemAPI.GetComponent<LocalTransform>(player).Position;
            float3 recalculatedPositionPlayer = new float3(playerLocation.x, 0f, playerLocation.z);
            float3 recalculatedPositionBullet = new float3(localTransform.ValueRO.Position.x, 0f, localTransform.ValueRO.Position.z);

            if (math.distance(recalculatedPositionBullet, recalculatedPositionPlayer) < 4f)
            {
                int health = SystemAPI.GetComponent<PlayerHealth>(player).health;
                health = health - beeBullet.ValueRO.damage;
                SystemAPI.SetComponent(player, new PlayerHealth { health = health });
                ecb.DestroyEntity(entity);
                continue;
            }
            
            beeBullet.ValueRW.deathTimer -= SystemAPI.Time.DeltaTime;

            if (beeBullet.ValueRO.deathTimer <= 0f)
            {
                ecb.DestroyEntity(entity);
                continue;
            }


            localTransform.ValueRW.Position.xyz += beeBullet.ValueRO.direction * beeBullet.ValueRO.speed * SystemAPI.Time.DeltaTime;
        }

        foreach ((RefRW<PlayerBullet> playerBullet, RefRW<LocalTransform> localTransform, Entity entity) in SystemAPI.Query<RefRW<PlayerBullet>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            if (playerBullet.ValueRO.target != Entity.Null)
            {
                float3 targetPosition = SystemAPI.GetComponent<LocalTransform>(playerBullet.ValueRO.target).Position;
                float3 recalculatedTargetPosition = new float3(targetPosition.x, 0f, targetPosition.z);
                float3 recalculatedPositionBullet = new float3(localTransform.ValueRO.Position.x, 0f, localTransform.ValueRO.Position.z);

                if (math.distance(recalculatedTargetPosition, recalculatedPositionBullet) < 5f) 
                {
                    int targetHealth = SystemAPI.GetComponent<EnemyHealth>(playerBullet.ValueRO.target).health;
                    targetHealth -= playerBullet.ValueRO.damage;
                    SystemAPI.SetComponent(playerBullet.ValueRO.target, new EnemyHealth { health = targetHealth });
                    ecb.DestroyEntity(entity);
                    continue;
                }

                float3 direction = targetPosition - localTransform.ValueRO.Position;
                float3 normalizeDirection = math.normalize(direction);

                localTransform.ValueRW.Position.xyz += normalizeDirection * playerBullet.ValueRO.speed * SystemAPI.Time.DeltaTime;
            }
            else
            {
                ecb.DestroyEntity (entity);
            }
        }

        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
