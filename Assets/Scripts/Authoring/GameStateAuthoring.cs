using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GameStateAuthoring : MonoBehaviour
{
    public class Baker : Baker<GameStateAuthoring>
    {
        public override void Bake(GameStateAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new TerrainCreated { terrainCount = 0 });
            AddComponent(entity, new State { endGame = false, freedMemory = false, gameStarted = false });
            AddComponent(entity, new GameStateTag());
        }
    }
}

public struct TerrainCreated : IComponentData
{
    public int terrainCount;
}

public struct State : IComponentData
{
    public bool gameStarted;
    public bool endGame;
    public bool freedMemory;
}

public struct GameStateTag : IComponentData 
{
    
}
