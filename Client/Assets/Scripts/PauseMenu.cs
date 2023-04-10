using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseUi;//esc ������ �� ������ ui

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    public void Pause()
    {
        pauseUi.SetActive(!pauseUi.activeSelf);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("StartScene");
    }

    public void Back()
    {
        pauseUi.SetActive(!pauseUi.activeSelf);
    }
}
