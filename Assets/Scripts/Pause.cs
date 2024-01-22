using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour 
{
    [System.NonSerialized] public bool IsOnPause;
    [Header("�|�[�Y���ɕ\������p�l����")]public GameObject [] pauseEffects;
    [Header("�|�[�Y���B�������I�u�W�F�N�g")] public GameObject [] HideObjects;
    //[Header("�|�[�Y��ʂɓ�����ʉ�")]public AudioClip pauseOnSE;
    //[Header("�|�[�Y��ʂ��甲������ʉ�")] public AudioClip pauseOffSE;
    //SoundManager soundManager;
    //public GameContoroller gameContoroller;//�X�e�[�W�N���A�ォ�ǂ������肷�邽�߂�gamecontoroller��o�^�B�Q�[�����e�ɂ���Ă͕s�v�B
    public static bool _isExec { private get; set; } = false;


    private void Start() {
        //GameObject gameObject = GameObject.FindGameObjectWithTag("SoundManager");
        //soundManager = gameObject.GetComponent<SoundManager>();
    }

    //��ʏ�ɐݒ肵�Ă���{�^�����N���b�N�����ꍇ
    public void OnClick() {
        pauseTheGame();
    }

    //�}�E�X�A�L�[�{�[�h�A�R���g���[���[���̃{�^�����������ꍇ
    public void Update() {
        //////////////////���ŏd�v���|�[�Y��ʂɂȂ����///////////////////
       if (Input.GetKeyDown(KeyCode.Escape)/* &&!gameContoroller.isEnd*/|| _isExec) {
            _isExec = false;
            pauseTheGame();
        }
    }

    public void pauseTheGame(){
        if (IsOnPause) {
            Time.timeScale = 1;
            IsOnPause = false;
            //soundManager.PlaySe(pauseOffSE);
            for (int i = 0; i < pauseEffects.Length; i++) {
                pauseEffects[i].SetActive(false);
            }
            for (int i = 0; i < HideObjects.Length; i++) {
                HideObjects[i].SetActive(true);
            }
        } else {
            //soundManager.PlaySe(pauseOnSE);
            Time.timeScale = 0;
            IsOnPause = true;
            for (int i = 0; i < pauseEffects.Length; i++) {
                pauseEffects[i].SetActive(true);
            }
            for (int i = 0; i < HideObjects.Length; i++) {
                HideObjects[i].SetActive(false);
            }
        }
    }
}
