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
            Entity entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new ChildContainer
            {
                child1 = GetEntity(authoring.child1, TransformUsageFlags.Renderable),
                child2 = GetEntity(authoring.child2, TransformUsageFlags.Renderable),
                child3 = GetEntity(authoring.child3, TransformUsageFlags.Renderable)
            });
            AddComponent(entity, new InfoToQuadrant());
            AddComponent(entity, new MeshData());
            AddComponent(entity, new MeshLODGroupComponent());
            AddComponent(entity, new VerticesNotCreated());
            SetComponentEnabled<VerticesNotCreated>(entity, false);
        }
    }
}
