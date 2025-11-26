using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public class BeeAuthoring : MonoBehaviour
{
    public GameObject beeBulletPrefab;
    public class Baker : Baker<BeeAuthoring>
    {
        public override void Bake(BeeAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BeeBulletPrefab { beeBulletPrefab = GetEntity(authoring.beeBulletPrefab, TransformUsageFlags.Dynamic) });
            AddComponent(entity, new BeeTag());
        }
    }
}

public struct BeeTag : IComponentData
{

}

public struct BeeBulletPrefab : IComponentData
{
    public Entity beeBulletPrefab;
}