using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("メニューパネル")]
    public GameObject menuPanel;
    [Header("操作説明パネル")]
    public GameObject descriptionPanel;
    [Header("設定パネル")]
    public GameObject settingPanel;

    // Start is called before the first frame update
    void Start()
    {
        BackToMenu();
    }

    // 操作説明パネルボタンをクリックしたとき
    public void SelectDescriptionPanel()
    {
        menuPanel.SetActive(false);
        descriptionPanel.SetActive(true);
    }

    // 設定画面パネルボタンをクリックしたとき
    public void SelectSettingPanel()
    {
        menuPanel.SetActive(false);
        settingPanel.SetActive(true);
    }

    // 2つのパネル内で戻るボタンが押されたとき
    public void BackToMenu()
    {
        menuPanel.SetActive(true);
        descriptionPanel.SetActive(false);
        settingPanel.SetActive(false);
    }
}
