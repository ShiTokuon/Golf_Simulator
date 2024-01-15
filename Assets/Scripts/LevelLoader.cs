using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    // �V�[���J�ڂ̃A�j���[�V����
    [Header("�V�[���J�ڃA�j���[�V����")]
    public Animator transition;
    // �t�F�C�h�A�E�g�J�n������ۂɃV�[���J�ڊJ�n�܂ł̎���
    [Header("�V�[���J�ڎ���")]
    [Range(0.0f, 5.0f)]
    public float transitionTime = 1f;
    // �t�F�C�h�A�E�g�J�n�܂ł̒x������
    [Header("�x�����s")]
    [Range(0.0f, 5.0f)]
    public float DelayActiveTime = 0.5f;
    // ���[�h�����ɂȂ����炷���J�ڂ��邩�ǂ���
    [Header("���[�h�����̎������V�[���J��")]
    public bool TransitionAuto_LoadFinish = false;

    private Canvas loadingcanvas;
    private Slider loadingbar;
    private Text loadingtext;
    // �񓯊����[�h�p
    private AsyncOperation LoadOperation;

    void Awake()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        loadingcanvas = transform.GetChild(1).gameObject.GetComponent<Canvas>();
        loadingbar = loadingcanvas.transform.GetChild(0).GetComponent<Slider>();
        loadingtext = loadingcanvas.transform.GetChild(1).GetComponent<Text>();
    }

    // �ق��̃V�[�����[�h��������
    public void LoadScene(int index)
    {
        StartCoroutine(LoadLevel(index));
    }

    // �V�[�������[�h��������
    public void ReloadScene()
    {
        var acrtivesceneindex = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(LoadLevel(acrtivesceneindex));
    }

    // �����p���[�h�R���[�`��
    private IEnumerator LoadLevel(int scenemane)
    {
        // �x��
        yield return new WaitForSeconds(DelayActiveTime);
        // �t�F�[�h�A�E�g�X�^�[�g
        transition.SetTrigger("Start");
        // �҂�
        yield return new WaitForSeconds(transitionTime);
        // ���[�h�J�n
        StartCoroutine(LoadAsync(scenemane));
    }

    // �񓯊����[�h����
    private IEnumerator LoadAsync(int s_index)
    {
        LoadOperation = SceneManager.LoadSceneAsync(s_index);
        LoadOperation.allowSceneActivation = TransitionAuto_LoadFinish;
        // ���[�h��ʕ\��
        loadingcanvas.enabled = true;

        while (!LoadOperation.isDone)
        {
            InLoading();
            yield return null;
        }
    }

    // ���[�h��ʓ�����
    private void InLoading()
    {
        /* 
        ���[�h��ʓ����������������ŏ���
        */

        // �v���O���X�o�[�ƕ������X�V����
        float progress = Mathf.Clamp01(LoadOperation.progress / .9f);
        loadingbar.value = progress;
        loadingtext.text = progress * 100f + " %";


        if (!LoadOperation.allowSceneActivation)
        {
            /*
            ���[�h����������ۂɃV�[���J�ڂ܂ł̏���
            */
            LoadOperation.allowSceneActivation = true;
        }
    }

}
