using Unity.Entities;
using UnityEngine;

public class AuxAuthoring : MonoBehaviour
{
    public GameObject aux;
    public GameObject quadrant;

    public class Baker : Baker<AuxAuthoring>
    {
        public override void Bake(AuxAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Aux
            {
                auxEntity = GetEntity(authoring.aux, TransformUsageFlags.Dynamic),
                quadrantEntity = GetEntity(authoring.quadrant, TransformUsageFlags.Dynamic)
            });
        }
    }
}



public struct Aux : IComponentData
{
    public Entity auxEntity;
    public Entity quadrantEntity;
}
