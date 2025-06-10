using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class EntitiesReferenceAuthoring : MonoBehaviour
{
    public GameObject terrainChunkPrefabGameObject;
    public Material material;
    public Transform viewer;

    public class Baker : Baker<EntitiesReferenceAuthoring>
    {
        public override void Bake(EntitiesReferenceAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                terrainChunkPrefabEntity = GetEntity(authoring.terrainChunkPrefabGameObject, TransformUsageFlags.Dynamic),
                material = authoring.material,
                viewer = authoring.viewer,
                example = entity
            });
        }
    }

}

public struct EntitiesReferences : IComponentData
{
    public Entity terrainChunkPrefabEntity;
    public Entity example;
    public UnityObjectRef<Material> material;
    public UnityObjectRef<Transform> viewer;
}
