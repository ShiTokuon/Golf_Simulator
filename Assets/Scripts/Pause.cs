using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public GameObject OnPanel;
    private bool IsOnPause = false;

    void Start()
    {

    }

    public void OnClick()
    {
        pauseGame();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            pauseGame();
        }
    }
    public void pauseGame()
    {
        if (IsOnPause)
        {
            Time.timeScale = 1;
            IsOnPause = false;
            OnPanel.SetActive(false);
        }
        else
        {
            Time.timeScale = 0;
            IsOnPause = true;
            OnPanel.SetActive(true);          
        }
    }
}
