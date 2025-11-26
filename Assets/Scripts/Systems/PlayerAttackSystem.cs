using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct PlayerAttackSystem : ISystem
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
        float3 playerLocation = SystemAPI.GetComponent<LocalTransform>(player).Position;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        ComponentLookup<BeeTag> beeLookUp = SystemAPI.GetComponentLookup<BeeTag>();
        ComponentLookup<RatTag> ratLookUp = SystemAPI.GetComponentLookup<RatTag>();
        PhysicsWorldSingleton physicsWorldSingelton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingelton.CollisionWorld;
        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

        foreach ((RefRW<PlayerBulletPrefab> bulletPrefab, EnabledRefRO<PlayerAttack> attack, RefRO<LocalTransform> localTransform, Entity entity) in SystemAPI.Query<RefRW<PlayerBulletPrefab>, EnabledRefRO<PlayerAttack>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            CollisionFilter collisionFilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << 7,
                GroupIndex = 0
            };
            Entity closerTarget = Entity.Null;

            if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position, 60, ref distanceHitList, collisionFilter))
            {
                foreach (DistanceHit distanceHit in distanceHitList)
                {
                    if (beeLookUp.HasComponent(distanceHit.Entity) || ratLookUp.HasComponent(distanceHit.Entity))
                    {
                        if (closerTarget == Entity.Null)
                        {
                            closerTarget = distanceHit.Entity;
                        }
                        else
                        {
                            if (math.distancesq(SystemAPI.GetComponent<LocalTransform>(distanceHit.Entity).Position, playerLocation) < math.distancesq(playerLocation, SystemAPI.GetComponent<LocalTransform>(closerTarget).Position))
                            {
                                closerTarget = distanceHit.Entity;
                            }
                        }

                    }
                }
            }

            if (closerTarget != Entity.Null)
            {
                Entity bullet = state.EntityManager.Instantiate(bulletPrefab.ValueRO.bullet);
                SystemAPI.SetComponent(bullet, new LocalTransform { Position = new float3(playerLocation.x, playerLocation.y + 15f, playerLocation.z), Rotation = quaternion.identity, Scale = 1f });
                PlayerBullet playerBullet = SystemAPI.GetComponent<PlayerBullet>(bullet);
                playerBullet.target = closerTarget;
                SystemAPI.SetComponent(bullet, playerBullet);
            }

            ecb.SetComponentEnabled<PlayerAttack>(entity, false);
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
