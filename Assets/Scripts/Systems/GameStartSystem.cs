using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

partial struct GameStartSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameStateTag>();
        state.RequireForUpdate<PlayerTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity gameState = SystemAPI.GetSingletonEntity<GameStateTag>();
        Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();

        if (SystemAPI.GetComponent<State>(gameState).gameStarted == false && SystemAPI.GetComponent<TerrainCreated>(gameState).terrainCount >= 10000)
        {
            //Empezar juego
            SystemAPI.SetComponent(player, new PhysicsGravityFactor { Value = 10 });
            LocalTransform localTransform = SystemAPI.GetComponent<LocalTransform>(player);
            localTransform.Position = new float3(0f, 200f, 0f);
            SystemAPI.SetComponent(player, localTransform);
            State stateGame = SystemAPI.GetComponent<State>(gameState);
            stateGame.gameStarted = true;
            SystemAPI.SetComponent(gameState, stateGame);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
