using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MapGeneratorAuthoring : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public float2 offset;

    public class Baker : Baker<MapGeneratorAuthoring>
    {
        public override void Bake(MapGeneratorAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MapGeneratorData
            {
                mapWidth = authoring.mapWidth,
                mapHeight = authoring.mapHeight,
                noiseScale = authoring.noiseScale,
                octaves = authoring.octaves,
                persistance = authoring.persistance,
                lacunarity = authoring.lacunarity,
                seed = authoring.seed,
                offset = authoring.offset,
            });
        }
    }
}


public struct MapGeneratorData : IComponentData
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public float2 offset;
}
