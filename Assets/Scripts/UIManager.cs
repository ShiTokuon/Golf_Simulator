using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("���j���[�p�l��")]
    public GameObject menuPanel;
    [Header("��������p�l��")]
    public GameObject descriptionPanel;
    [Header("�ݒ�p�l��")]
    public GameObject settingPanel;

    // Start is called before the first frame update
    void Start()
    {
        BackToMenu();
    }

    // ��������p�l���{�^�����N���b�N�����Ƃ�
    public void SelectDescriptionPanel()
    {
        menuPanel.SetActive(false);
        descriptionPanel.SetActive(true);
    }

    // �ݒ��ʃp�l���{�^�����N���b�N�����Ƃ�
    public void SelectSettingPanel()
    {
        menuPanel.SetActive(false);
        settingPanel.SetActive(true);
    }

    // 2�̃p�l�����Ŗ߂�{�^���������ꂽ�Ƃ�
    public void BackToMenu()
    {
        menuPanel.SetActive(true);
        descriptionPanel.SetActive(false);
        settingPanel.SetActive(false);
    }
}
