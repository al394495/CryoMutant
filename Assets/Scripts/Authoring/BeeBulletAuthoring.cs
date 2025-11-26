using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BeeBulletAuthoring : MonoBehaviour
{
    public int damage;
    public float speed;
    public float deathTimer;
    public class Baker : Baker<BeeBulletAuthoring>
    {
        public override void Bake(BeeBulletAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BeeBullet {damage = authoring.damage, speed = authoring.speed, deathTimer = authoring.deathTimer });
        }
    }
}

public struct BeeBullet : IComponentData
{
    public float3 direction;
    public int damage;
    public float speed;
    public float deathTimer;
}
