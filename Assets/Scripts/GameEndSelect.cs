using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitSelect : MonoBehaviour
{
    [Header("�Q�[���I���p�l��")]
    public GameObject ExitPanel;
    [Header("�͂��{�^��")]
    public Button YesBotton;
    [Header("�������{�^��")]
    public Button NoBotton;

    //private Action YesAction;
    //private Action NoAction;

    void Start()
    {
        // �{�^���Ƀ��X�i�[��ǉ�
        YesBotton.onClick.AddListener(onYesClick);
        NoBotton.onClick.AddListener(onNoClick);
    }

    public void onExitClick()
    {
        ExitPanel.SetActive(true);
    }

    public void onYesClick()
    {
        UnityEditor.EditorApplication.isPlaying = false;
    }

    public void onNoClick()
    {
        ExitPanel.SetActive(false);
    }
}
