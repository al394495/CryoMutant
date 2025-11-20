using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
partial struct PlayerMoveSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        new PlayerMoveJob
        {
            DeltaTime = deltaTime
        }.Schedule();
    }

}

[BurstCompile]
public partial struct PlayerMoveJob : IJobEntity
{
    public float DeltaTime;

    private void Execute(ref LocalTransform transform, in PlayerMoveInput moveInput, PlayerMoveSpeed moveSpeed)
    {
        transform.Position.xz += moveInput.moveInput * moveSpeed.moveSpeed * DeltaTime;

        if (math.lengthsq(moveInput.moveInput) > float.Epsilon)
        {
            float3 forward = new float3(moveInput.moveInput.x, 0f, moveInput.moveInput.y);
            transform.Rotation = quaternion.LookRotation(forward, math.up());
        }
    }
}
