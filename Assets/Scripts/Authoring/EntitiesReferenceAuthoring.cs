using Unity.Entities;
using UnityEngine;

public class EntitiesReferenceAuthoring : MonoBehaviour
{
    public GameObject terrainChunkPrefabGameObject;

    public class Baker : Baker<EntitiesReferenceAuthoring>
    {
        public override void Bake(EntitiesReferenceAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                terrainChunkPrefabEntity = GetEntity(authoring.terrainChunkPrefabGameObject, TransformUsageFlags.Dynamic)
            });
        }
    }

}

public struct EntitiesReferences : IComponentData
{
    public Entity terrainChunkPrefabEntity;
}
