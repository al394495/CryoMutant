using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class CheckEndGame : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<State>().Build(entityManager);

        NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
        for (int i = 0; i < entityArray.Length; i++)
        {
            State state = entityManager.GetComponentData<State>(entityArray[i]);
            if (state.endGame == true && state.freedMemory == true)
            {
                //cambio de escena
            }
        }

    }
}
