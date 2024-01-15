using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
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

    // ほかのシーンロードしたい時
    public void LoadScene(int index)
    {
        StartCoroutine(LoadLevel(index));
    }

    // シーンリロードしたい時
    public void ReloadScene()
    {
        var acrtivesceneindex = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(LoadLevel(acrtivesceneindex));
    }

    // 内部用ロードコルーチン
    private IEnumerator LoadLevel(int scenemane)
    {
        // 遅延
        yield return new WaitForSeconds(DelayActiveTime);
        // フェードアウトスタート
        transition.SetTrigger("Start");
        // 待ち
        yield return new WaitForSeconds(transitionTime);
        // ロード開始
        StartCoroutine(LoadAsync(scenemane));
    }

    // 非同期ロード処理
    private IEnumerator LoadAsync(int s_index)
    {
        LoadOperation = SceneManager.LoadSceneAsync(s_index);
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
            ロード完成から実際にシーン遷移までの処理
            */
            LoadOperation.allowSceneActivation = true;
        }
    }

}
