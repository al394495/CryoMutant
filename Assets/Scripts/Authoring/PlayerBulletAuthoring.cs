using Unity.Entities;
using UnityEngine;

public class PlayerBulletAuthoring : MonoBehaviour
{
    public int damage;
    public float speed;
    public class Baker : Baker<PlayerBulletAuthoring>
    {
        public override void Bake(PlayerBulletAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerBullet { damage = authoring.damage, speed = authoring.speed});
        }
    }
}

public struct PlayerBullet : IComponentData
{
    public Entity target;
    public int damage;
    public float speed;
}
