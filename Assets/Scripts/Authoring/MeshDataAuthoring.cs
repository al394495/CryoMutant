using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class MeshDataAuthoring : MonoBehaviour
{
    public bool onHeightMapGenerated;
    public bool onMeshGenerated;

    public class Baker : Baker<MeshDataAuthoring>
    {
        public override void Bake(MeshDataAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MeshData
            {
                onHeightMapGenerated = authoring.onHeightMapGenerated,
                onMeshGenerated = authoring.onMeshGenerated,
                myEntity = entity,
            });
            AddBuffer<VerticeFloat3Buffer>(entity);
            AddBuffer<TriangleIntBuffer>(entity);
            AddBuffer<UvFloat2Buffer>(entity);
        }
    }
}

public struct MeshData : IComponentData
{
    public bool onHeightMapGenerated;
    public bool onMeshGenerated;

    public Entity myEntity;

    public float2 coord;
    public int size;
}

public struct CoordInfo : IComponentData
{
    public float2 coord;
    public int size;
}

public struct VericesNotCreated : IComponentData, IEnableableComponent
{

}

public struct MeshNotCreated : IComponentData, IEnableableComponent
{

}

//[InternalBufferCapacity(81)]
public struct VerticeFloat3Buffer : IBufferElementData
{
    public float3 value;
}

//[InternalBufferCapacity(486)]
public struct TriangleIntBuffer : IBufferElementData
{
    public int value;
}

//[InternalBufferCapacity(81)]
public struct UvFloat2Buffer : IBufferElementData
{
    public float2 value;
}

//[InternalBufferCapacity(81)]
public struct NormalFloat3Buffer : IBufferElementData
{
    public float3 value;
}
