using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Rendering;
using System;

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
                min = new float3(10000, 10000, 10000),
                max = new float3(-10000, -10000, -10000)
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

    public UnityObjectRef<Mesh> mesh;

    public float3 min;
    public float3 max;

    public Entity myEntity;
}

public struct VerticeFloat3Buffer : IBufferElementData
{
    public float3 value;
}

public struct TriangleIntBuffer : IBufferElementData
{
    public int value;
}

public struct UvFloat2Buffer : IBufferElementData
{
    public float2 value;
}
