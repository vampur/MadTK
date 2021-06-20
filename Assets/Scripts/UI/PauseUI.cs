using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseUI : MonoBehaviour
{
    public GameObject gobjUI;

    PlayerController playerController;
    bool bPaused;

    void Start()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            bPaused = !bPaused;
            playerController.bBlockInput = bPaused;
        }
        gobjUI.SetActive(bPaused);
        Cursor.lockState = bPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void ButtonResume()
    {
        bPaused = false;
        playerController.bBlockInput = bPaused;
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
