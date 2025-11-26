using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameButtonsController : MonoBehaviour
{
    public GameObject menuContainer;

    public void OnReanudeButtonPressed()
    {
        menuContainer.SetActive(false);
    }

    public void OnExitButtonPressed()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<State>().Build(entityManager);

        NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
        for (int i = 0; i < entityArray.Length; i++)
        {
            State state = entityManager.GetComponentData<State>(entityArray[i]);

            state.endGame = true;
            entityManager.SetComponentData(entityArray[i], state);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuContainer.SetActive(true);
        }
    }
}
