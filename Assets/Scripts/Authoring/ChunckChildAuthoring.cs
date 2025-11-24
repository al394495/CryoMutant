using Unity.Entities;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ChunckChildAuthoring : MonoBehaviour
{
    public GameObject chunckParent;

    public class Baker : Baker<ChunckChildAuthoring>
    {
        public override void Bake(ChunckChildAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable);
            Entity chunckParent = GetEntity(authoring.chunckParent, TransformUsageFlags.Renderable);
            AddComponent(entity, new Parent { Value = chunckParent});
            AddComponent(entity, new MeshLODComponent { ParentGroup = chunckParent, Group = chunckParent});
            AddComponent(entity, new CoordInfo());
            AddBuffer<VerticeFloat3Buffer>(entity);
            AddBuffer<UvFloat2Buffer>(entity);
            AddBuffer<TriangleIntBuffer>(entity);
            AddBuffer<NormalFloat3Buffer>(entity);
            AddComponent(entity, new MeshNotCreated());
            AddComponent(entity, new DecorationsNotCreated());
            SetComponentEnabled<MeshNotCreated>(entity, false);
            SetComponentEnabled<DecorationsNotCreated>(entity, false);
        }
    }
}
