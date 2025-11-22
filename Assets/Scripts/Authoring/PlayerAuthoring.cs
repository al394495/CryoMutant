using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAuthoring : MonoBehaviour
{
    public GameObject playerGameObjectPrefab;
    public float moveSpeed;
    public class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerTag());
            AddComponent(entity, new PlayerMoveInput());
            AddComponent(entity, new PlayerMoveSpeed { moveSpeed = authoring.moveSpeed });
            AddComponent(entity, new PlayerGameObjectPrefab { prefab = authoring.playerGameObjectPrefab });
            AddComponent(entity, new PhysicsGravityFactor { Value = 5f });
        }
    }
}

public struct PlayerTag : IComponentData
{

}

public struct PlayerMoveInput : IComponentData
{
    public float2 moveInput;
}

public struct PlayerMoveSpeed : IComponentData
{
    public float moveSpeed;
}

public struct PlayerGameObjectPrefab : IComponentData
{
    public UnityObjectRef<GameObject> prefab;
}

public class PlayerAnimatorReference : IComponentData
{
    public Animator animator;
}
