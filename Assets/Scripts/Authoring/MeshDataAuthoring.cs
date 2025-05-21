using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class MeshDataAuthoring : MonoBehaviour
{
    public bool onHeightMapGenerated;

    public class Baker : Baker<MeshDataAuthoring>
    {
        public override void Bake(MeshDataAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MeshData
            {
                onHeightMapGenerated = authoring.onHeightMapGenerated,
                myEntity = entity,
            });
            AddBuffer<FloatBuffer>(entity);
        }
    }
}

public struct MeshData : IComponentData
{
    //public NativeArray<float3> vertices;
    //public NativeArray<int> triangles;
    //public NativeArray<float2> uvs;
    //public 

    //public NativeArray<float> heightMap;

    public bool onHeightMapGenerated;

    public Entity myEntity;
}

public struct FloatBuffer : IBufferElementData
{
    public float value;
}
