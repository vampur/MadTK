using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{// yes I know this code can be recycled but I literally don't have anymore time

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public void ButtonRestart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MadTK");
    }

    public void ButtonQuit()
    {
        Application.Quit();
    }
}
