using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    private const bool V = false;

    // シーン遷移のアニメーション
    [Header("シーン遷移アニメーション")]
    public Animator transition;
    // フェイドアウト開始から実際にシーン遷移開始までの時間
    [Header("シーン遷移時間")]
    [Range(0.0f, 5.0f)]
    public float transitionTime = 1f;
    // フェイドアウト開始までの遅延時間
    [Header("遅延実行")]
    [Range(0.0f, 5.0f)]
    public float DelayActiveTime = 0.5f;
    // ロード完成になったらすぐ遷移するかどうか
    [Header("ロード完成の時自動シーン遷移")]
    public bool TransitionAuto_LoadFinish = false;

    private Canvas loadingcanvas;
    private Slider loadingbar;
    private Text loadingtext;
    // 非同期ロード用
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

    // ほかのシーンロートしたい時この関数を使う(シーンのインデックス)
    public void LoadScene(string sceneName)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        StartCoroutine(LoadLevel(sceneName));
        pause._isExec = true;
    }

    // シーンリロートしたい時この関数を使う
    public void ReloadScene(string sceneName)
    {
        var acrtivesceneindex = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(LoadLevel(sceneName));
        pause._isExec = true;
    }

    // 内部用ロードコルーチン
    private IEnumerator LoadLevel(string sceneName)
    {
        // 遅延
        yield return new WaitForSeconds(DelayActiveTime);
        // フェードアウトスタート
        transition.SetTrigger("Start");
        // 待ち
        yield return new WaitForSeconds(transitionTime);
        // ロード開始
        StartCoroutine(LoadAsync(sceneName));
    }

    // 非同期ロード処理
    private IEnumerator LoadAsync(string sceneName)
    {     
        LoadOperation = SceneManager.LoadSceneAsync(sceneName);
        LoadOperation.allowSceneActivation = TransitionAuto_LoadFinish;
        // ロード画面表示
        loadingcanvas.enabled = true;
        
        while (!LoadOperation.isDone)
        {
            InLoading();
            yield return null;
        }
    }

    // ロード画面内処理
    private void InLoading()
    {
        /* 
        ロード画面内したい処理ここで書く
        */

        // プログレスバーと文字を更新する
        float progress = Mathf.Clamp01(LoadOperation.progress / .9f);
        loadingbar.value = progress;
        loadingtext.text = progress * 100f + " %";

        if (!LoadOperation.allowSceneActivation)
        {
            /*
            ロード完成から実際にシーン遷移までの処理ここで書く
            (ここはロード完成後スペースキー押したら遷移の処理にします)
            */
            if (Input.GetKeyDown(KeyCode.Space))
            {
                LoadOperation.allowSceneActivation = true;
            }
        }
    }
}
