using Unity.Entities;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ChunckAuthoring : MonoBehaviour
{
    public GameObject child1;
    public GameObject child2;
    public GameObject child3;

    public class Baker : Baker<ChunckAuthoring>
    {
        public override void Bake(ChunckAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            //AddComponent(entity, new LocalTransform());
            AddComponent(entity, new ChildContainer
            {
                child1 = GetEntity(authoring.child1, TransformUsageFlags.Dynamic),
                child2 = GetEntity(authoring.child2, TransformUsageFlags.Dynamic),
                child3 = GetEntity(authoring.child3, TransformUsageFlags.Dynamic)
            });
            AddComponent(entity, new MeshData());
            AddComponent(entity, new MeshLODGroupComponent());
            AddComponent(entity, new VericesNotCreated());
            SetComponentEnabled<VericesNotCreated>(entity, false);
        }
    }
}
