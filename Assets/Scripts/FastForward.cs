using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    public void PauseGame()
    {
        //âüÇµÇΩéû1Ç©0Ç©Ç≈êÿÇËë÷Ç¶ÇÈ
        if (Time.timeScale == 1)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }
}
