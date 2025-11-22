using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(PhysicsSystemGroup), OrderLast = true)]
partial struct PlayerMoveSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new PlayerMoveJob().Schedule();
    }

}

[BurstCompile]
public partial struct PlayerMoveJob : IJobEntity
{

    private void Execute(ref LocalTransform transform, in PlayerMoveInput moveInput, in PlayerMoveSpeed moveSpeed, ref PhysicsVelocity physicsVelocity, ref PhysicsMass physicsMass)
    {
        float velocityY = physicsVelocity.Linear.y;
        physicsVelocity.Linear = new float3(moveInput.moveInput.x * moveSpeed.moveSpeed, -10f, moveInput.moveInput.y * moveSpeed.moveSpeed);
        physicsVelocity.Angular = float3.zero;
        physicsMass.InverseInertia = float3.zero;

        if (math.lengthsq(moveInput.moveInput) > float.Epsilon)
        {
            float3 forward = new float3(moveInput.moveInput.x, 0f, moveInput.moveInput.y);
            transform.Rotation = quaternion.LookRotation(forward, math.up());
        }
    }
}
