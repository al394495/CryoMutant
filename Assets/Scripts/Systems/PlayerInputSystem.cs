using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
partial class PlayerInputSystem : SystemBase
{
    private MovementActions movementActions;
    private Entity playerEntity;

    protected override void OnCreate()
    {
        RequireForUpdate<PlayerTag>();
        movementActions = new MovementActions();
    }

    protected override void OnStartRunning()
    {
        movementActions.Enable();

        playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
    }

    protected override void OnUpdate()
    {
        Vector2 moveInput = movementActions.Map.PlayerMovement.ReadValue<Vector2>();
        SystemAPI.SetSingleton(new PlayerMoveInput { moveInput = new float2(moveInput.x, moveInput.y) });
    }

    protected override void OnStopRunning()
    {
        movementActions.Disable();
    }

    protected override void OnDestroy()
    {

    }
}
