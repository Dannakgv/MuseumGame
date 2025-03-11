using UnityEngine;
using UnityEngine.SceneManagement;

public class Instructions : MonoBehaviour
{
public void GoBackToMenu()
{
    SceneManager.LoadScene("MainMenu");
}

}

