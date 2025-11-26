using Unity.Entities;
using UnityEngine;

public class RatAuthoring : MonoBehaviour
{
    public int damage;
    public class Baker : Baker<RatAuthoring>
    {
        public override void Bake(RatAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new RatTag());
            AddComponent(entity, new RatDamage { damage = authoring.damage });
        }
    }
}

public struct RatTag : IComponentData
{

}

public struct RatDamage : IComponentData
{
    public int damage;
}
