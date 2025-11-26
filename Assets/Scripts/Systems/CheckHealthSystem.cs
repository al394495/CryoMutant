using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

partial struct CheckHealthSystem : ISystem
{

    Entity player;
    Entity gameState;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
        state.RequireForUpdate<GameStateTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        player = SystemAPI.GetSingletonEntity<PlayerTag>();
        gameState = SystemAPI.GetSingletonEntity<GameStateTag>();

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        /*foreach ((RefRW<EnemyHealth> enemyHealth, Entity entity) in SystemAPI.Query<RefRW<EnemyHealth>>().WithEntityAccess())
        {
            if (enemyHealth.ValueRO.health <= 0)
            {
                ecb.DestroyEntity(entity);
            }
        }*/

        ecb.Playback(state.EntityManager);

        if (SystemAPI.GetComponent<PlayerHealth>(player).health <= 0)
        {
            State stateGame = SystemAPI.GetComponent<State>(gameState);
            stateGame.endGame = true;
            SystemAPI.SetComponent(gameState, stateGame);
        }

    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
