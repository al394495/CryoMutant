using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

partial struct EnemyAttackSystem : ISystem
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

        foreach ((RefRW<EnemyAttackTimer> attackTimer, EnabledRefRO<EnemyAttack> attack, RefRO<LocalTransform> localTransform, RefRO<BeeTag> beeTag, RefRO<BeeBulletPrefab> bulletPrefab, Entity entity) in SystemAPI.Query<RefRW<EnemyAttackTimer>, EnabledRefRO<EnemyAttack>, RefRO<LocalTransform>, RefRO<BeeTag>, RefRO<BeeBulletPrefab>>().WithEntityAccess())
        {
            attackTimer.ValueRW.attackTimer -= SystemAPI.Time.DeltaTime;
            if (attackTimer.ValueRO.attackTimer > 0f)
            {
                ecb.SetComponentEnabled<EnemyAttack>(entity, false);
                continue;
            }
            attackTimer.ValueRW.attackTimer = 1f;


            Entity bullet = state.EntityManager.Instantiate(bulletPrefab.ValueRO.beeBulletPrefab);
            float3 playerLocation = SystemAPI.GetComponent<LocalTransform>(player).Position;
            float3 recalculatedPositionPlayer = new float3(playerLocation.x, playerLocation.y + 15f, playerLocation.z);
            float3 position = new float3(localTransform.ValueRO.Position.x, localTransform.ValueRO.Position.y + 20f, localTransform.ValueRO.Position.z);
            float3 direction = recalculatedPositionPlayer - position;
            float3 normalizedDirection = math.normalize(direction);
            quaternion quaternionRotation = quaternion.LookRotation(direction, math.up());

            SystemAPI.SetComponent(bullet, new LocalTransform { Position = position, Rotation = quaternionRotation, Scale = 2f});
            BeeBullet beeBullet = SystemAPI.GetComponent<BeeBullet>(bullet);
            beeBullet.direction = normalizedDirection;
            SystemAPI.SetComponent(bullet, beeBullet);

            ecb.SetComponentEnabled<EnemyAttack>(entity, false);

        }

        foreach ((RefRW<EnemyAttackTimer> attackTimer, EnabledRefRO<EnemyAttack> attack, RefRO<RatDamage> ratDamage, RefRO<RatTag> ratTag, Entity entity) in SystemAPI.Query<RefRW<EnemyAttackTimer>, EnabledRefRO<EnemyAttack>, RefRO<RatDamage>, RefRO<RatTag>>().WithEntityAccess())
        {
            attackTimer.ValueRW.attackTimer -= SystemAPI.Time.DeltaTime;
            if (attackTimer.ValueRO.attackTimer > 0f)
            {
                ecb.SetComponentEnabled<EnemyAttack>(entity, false);
                continue;
            }
            attackTimer.ValueRW.attackTimer = 0.5f;

            int health = SystemAPI.GetComponent<PlayerHealth>(player).health;
            health = health - ratDamage.ValueRO.damage;
            SystemAPI.SetComponent(player, new PlayerHealth { health = health });
        }

        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
