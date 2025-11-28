using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    public Slider healthSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlayerHealth>().Build(entityManager);

        NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
        for (int i = 0; i < entityArray.Length; i++)
        {
            PlayerHealth playerHealth = entityManager.GetComponentData<PlayerHealth>(entityArray[i]);


            healthSlider.value = (float)(playerHealth.health / 100f);
            
        }
    }
}
