using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    public void GoBackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

}

