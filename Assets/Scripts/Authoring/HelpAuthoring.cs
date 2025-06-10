using Unity.Entities;
using UnityEngine;

public class HelpAuthoring : MonoBehaviour
{
    public GameObject help;

    public class Baker : Baker<HelpAuthoring>
    {
        public override void Bake(HelpAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Help
            {
                help = GetEntity(authoring.help, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct Help : IComponentData
{
    public Entity help;
}
