using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void ButtonStart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MadTK");
    }

    public void ButtonQuit()
    {
        Application.Quit();
    }
}
