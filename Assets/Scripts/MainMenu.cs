using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("LevelManager");
    }

    public void OpenInstructions()
    {
        SceneManager.LoadScene("InstructionsScene");
    }

    public void OpenGallery()
    {
        SceneManager.LoadScene("LevelManager");
    }
}

