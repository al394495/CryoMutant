using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class EntitiesReferenceAuthoring : MonoBehaviour
{
    public GameObject terrainChunkPrefabGameObject;
    public GameObject treePrefabGameObject;
    public GameObject tree1PrefabGameObject;
    public GameObject tree2PrefabGameObject;
    public GameObject enemyRat;
    public GameObject enemyBee;
    public Material material;

    public class Baker : Baker<EntitiesReferenceAuthoring>
    {
        public override void Bake(EntitiesReferenceAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                terrainChunkPrefabEntity = GetEntity(authoring.terrainChunkPrefabGameObject, TransformUsageFlags.Renderable),
                treePrefabEntity = GetEntity(authoring.treePrefabGameObject, TransformUsageFlags.Renderable),
                tree1PrefabEntity = GetEntity(authoring.tree1PrefabGameObject, TransformUsageFlags.Renderable),
                tree2PrefabEntity = GetEntity(authoring.tree2PrefabGameObject, TransformUsageFlags.Renderable),
                enemyRat = GetEntity(authoring.enemyRat, TransformUsageFlags.Dynamic),
                enemyBee = GetEntity(authoring.enemyBee, TransformUsageFlags.Dynamic),
                material = authoring.material,
                example = entity
            });
        }
    }

}

public struct EntitiesReferences : IComponentData
{
    public Entity terrainChunkPrefabEntity;
    public Entity treePrefabEntity;
    public Entity tree1PrefabEntity;
    public Entity tree2PrefabEntity;
    public Entity enemyRat;
    public Entity enemyBee;
    public Entity example;
    public UnityObjectRef<Material> material;
}
