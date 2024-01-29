using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ExitSelect : MonoBehaviour
{
    [Header("ゲーム終了パネル")]
    public GameObject ExitPanel;
    [Header("はいボタン")]
    public Button YesBotton;
    [Header("いいえボタン")]
    public Button NoBotton;

    //private Action YesAction;
    //private Action NoAction;

    void Start()
    {
        // ボタンにリスナーを追加
        YesBotton.onClick.AddListener(onYesClick);
        NoBotton.onClick.AddListener(onNoClick);
    }

    public void onExitClick()
    {
        ExitPanel.SetActive(true);
    }

    public void onYesClick()
    {
        // はいが押されたときゲームを閉じる
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void onNoClick()
    {
        ExitPanel.SetActive(false);
    }
}
