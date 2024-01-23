using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    private const bool V = false;

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
    private Pause pause;

    void Awake()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        this.pause = FindObjectOfType<Pause>();
        loadingcanvas = transform.GetChild(1).gameObject.GetComponent<Canvas>();
        loadingbar = loadingcanvas.transform.GetChild(0).GetComponent<Slider>();
        loadingtext = loadingcanvas.transform.GetChild(1).GetComponent<Text>();
    }

    // �ق��̃V�[�����[�g�����������̊֐����g��(�V�[���̃C���f�b�N�X)
    public void LoadScene(string sceneName)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        StartCoroutine(LoadLevel(sceneName));
        pause._isExec = true;
    }

    // �V�[�������[�g�����������̊֐����g��
    public void ReloadScene(string sceneName)
    {
        var acrtivesceneindex = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(LoadLevel(sceneName));
        pause._isExec = true;
    }

    // �����p���[�h�R���[�`��
    private IEnumerator LoadLevel(string sceneName)
    {
        // �x��
        yield return new WaitForSeconds(DelayActiveTime);
        // �t�F�[�h�A�E�g�X�^�[�g
        transition.SetTrigger("Start");
        // �҂�
        yield return new WaitForSeconds(transitionTime);
        // ���[�h�J�n
        StartCoroutine(LoadAsync(sceneName));
    }

    // �񓯊����[�h����
    private IEnumerator LoadAsync(string sceneName)
    {     
        LoadOperation = SceneManager.LoadSceneAsync(sceneName);
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
            ���[�h����������ۂɃV�[���J�ڂ܂ł̏��������ŏ���
            (�����̓��[�h������X�y�[�X�L�[��������J�ڂ̏����ɂ��܂�)
            */
            if (Input.GetKeyDown(KeyCode.Space))
            {
                LoadOperation.allowSceneActivation = true;
            }
        }
    }
}
