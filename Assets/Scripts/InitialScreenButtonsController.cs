using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialScreenButtonsController : MonoBehaviour
{
    public GameObject instructionsContainer;


    public void OnPlayPressed()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnInstructionsPressed()
    {
        instructionsContainer.SetActive(true);
    }

    public void OnExitInstructionsPressed()
    {
        instructionsContainer.SetActive(false);
    }

    public void OnExitPressed()
    {
        Application.Quit();
    }

}
