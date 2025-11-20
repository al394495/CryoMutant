using Unity.Entities;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Mathematics;

[UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
partial struct PlayerAnimationSystem : ISystem
{

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<PlayerGameObjectPrefab> playerGameObjectPrefab, Entity entity) in SystemAPI.Query<RefRO<PlayerGameObjectPrefab>>().WithNone<PlayerAnimatorReference>().WithEntityAccess())
        {
            Object gameObject = Object.Instantiate(playerGameObjectPrefab.ValueRO.prefab);
            ecb.AddComponent(entity, new PlayerAnimatorReference { animator = gameObject.GetComponent<Animator>() });
        }

        foreach ((RefRO<LocalTransform> transform, PlayerAnimatorReference animatorReference, RefRO<PlayerMoveInput> playerMoveInput) in SystemAPI.Query<RefRO<LocalTransform>, PlayerAnimatorReference, RefRO<PlayerMoveInput>>())
        {
            animatorReference.animator.SetBool("IsMoving", math.length(playerMoveInput.ValueRO.moveInput) > 0f);
            animatorReference.animator.transform.position = transform.ValueRO.Position;
            animatorReference.animator.transform.rotation = transform.ValueRO.Rotation;
        }

        ecb.Playback(state.EntityManager);
    }

}
