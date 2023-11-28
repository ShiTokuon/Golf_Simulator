using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SphereBooster : MonoBehaviour
{
    // DistanceTextオブジェクトへの参照
    [SerializeField]
    GameObject distanceTextObject;

    // HighScoreTextオブジェクトへの参照
    [SerializeField]
    GameObject highScoreTextObject;

    // 発射ボタンオブジェクトへの参照
    [SerializeField]
    GameObject boostButtonObject;

    // 発射方向
    [SerializeField]
    LineRenderer line;

    // 加える力の大きさ
    [SerializeField]
    float forceMagnitude = 10f;

    // X軸からの角度(90まで設定)
    [SerializeField, Range(0f, 90f)]
    float forceAngle = 45.0f;

    // Rigidbodyコンポーネントへの参照をキャッシュ
    Rigidbody rb;

    // DistanceTextオブジェクトのTextコンポーネントへの参照をキャッシュ
    Text distanceText;

    // HighScoereTextオブジェクトのTextコンポーネントへの参照をキャッシュ
    Text highScoreText;

    // 発射ボタンオブジェクトへの参照
    Button boostButton;

    // 飛行中フラグ
    bool isFlying = false;

    // ボタン押下フラグ
    bool isBoostPressed = false;

    // 距離測定中フラグ
    bool isCheckingDistance = false;

    // ボタンを押せる状態かどうかのフラグ
    bool canButtonPress = true;

    // ドラッグ開始フラグ
    bool isDragging = false;

    // ボールのオブジェクト停止位置格納用ベクトル
    Vector3 stopPosition = Vector3.zero;

    // 角度の上限と下限の定義
    const float MaxAngle = 180f;
    const float MinAngle = 0f;

    // 力を加える方向
    Vector3 forceDirection = new Vector3(1.0f, 1.0f, 0f);

    // ドラッグ最大付与力量
    public float dragLimit = 1f;

    // 加える力の大きさ
    //public float forceToAdd = 10f;

    // ボールを発射する直前の位置
    Vector3 prePosition = Vector3.zero;

    // 発射方向の力
    Vector3 currentForce = Vector3.zero;

    // メインカメラ
    private Camera mainCamera = null;

    // メインカメラ座標
    private Transform mainCameraPos = null;

    // ドラック開始点
    private Vector3 dragStart = Vector3.zero;

    // UIテキストのプレフィックス
    string distancePrefix = "飛距離: ";

    // UIテキストのサフィックス
    string distanceSuffix = " m";

    // ハイスコア表示のプレフィックス
    string highScorePrefix = "ハイスコア: ";

    // ハイスコア表示のサフィックス
    string highScoreSuffix = " m";

    // ハイスコアの距離
    float highScoreDistance = 0f;

    // 落下判定用のタグ
    string fallCheckerTag = "FallChecker";


    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        distanceText = distanceTextObject.GetComponent<Text>();
        highScoreText = highScoreTextObject.GetComponent<Text>();
        mainCamera = Camera.main;
        mainCameraPos = mainCamera.transform;

        // boostボタンのコンポーネントへの参照を追加
        boostButton = boostButtonObject.GetComponent<Button>();

        // DistanceTextとHighScoreTextの初期値をセット
        SetDistanceText(0f);
        SetHighScoreText(0f);
    }

    void Update()
    {
        // キーボードからの入力を監視
        CheckInput();

        // forceAngleの変更を反映する
        CalcForceDirection();
    }

    void FixedUpdate()
    {
        // 距離の測定
        CheckDistance();

        // ゴール判定
        CheckSphereState();

        if (!isBoostPressed)
        {
            // キーまたはボタンが押されていなければ
            // 処理の切り替えをせず抜ける
            return;
        }

        // Boostボタンが押された場合に以下の処理を行う
        canButtonPress = false;
        boostButton.interactable = canButtonPress;

        // ボールの発射処理
        BoostSphere();

        // 飛行中フラグの切り替え
        isFlying = true;

        // どちらの処理をしてもボタン押下フラグをfalseに
        isBoostPressed = false;
    }

    void StopFlying()
    {
        // 運動の停止
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // ボール発射前の位置に移動させる
        gameObject.transform.position = prePosition;

        // ボール停止後の処理を呼ぶ
        ReadyToBoost();
    }

    void BoostSphere()
    {
        // ボールが発射される時の位置を記録
        prePosition = transform.position;

        //// 向きと力の計算
        //Vector3 force = forceMagnitude * forceDirection;

        //// 力を加えるメソッド
        //rb.AddForce(force, ForceMode.Impulse);

        OnMouseUp();

        // 距離測定中をTrueにセット
        isCheckingDistance = true;

        // 角度矢印を非表示にする
        //angleArrowObject.SetActive(false);

        // ボタンを押せないようにする
        //SetAngleButtonState(false);

        // ガイドを非表示にする
        //guideManager.SetGuidesState(false);
    }

    public void OnPressedBoostButton()
    {
        isBoostPressed = true;
    }

    void CalcForceDirection()
    {
        // 入力された角度をラジアンに変換
        float rad = forceAngle * Mathf.Deg2Rad;

        // それぞれの軸の成分を計算
        float x = Mathf.Cos(rad);
        float y = Mathf.Sin(rad);
        float z = 0f;

        // Vector3型に格納
        forceDirection = new Vector3(x, y, z);
    }

    void CheckDistance()
    {
        if (!isCheckingDistance)
        {
            // 距離測定中なら何もしない
            return;
        }

        // 現在距離までの距離を計算する
        Vector3 currentPosition = gameObject.transform.position;
        float distance = GetDistanceInXZ(prePosition, currentPosition);

        // UIに表示
        SetDistanceText(distance);

        if (rb.IsSleeping())
        {
            // ハイスコアのチェック
            stopPosition = currentPosition;
            float currentDistance = GetDistanceInXZ(prePosition, stopPosition);

            if (currentDistance > highScoreDistance)
            {
                highScoreDistance = currentDistance;
            }
            SetHighScoreText(highScoreDistance);

            // 距離測定中をオフにする
            isCheckingDistance = false;
        }
    }
    float GetDistanceInXZ(Vector3 startPos, Vector3 stopPos)
    {
        // 開始位置、停止位置それぞれ、Y軸を除いてVector3を制作
        Vector3 startPosCalc = new Vector3(startPos.x, 0f, startPos.z);
        Vector3 stopPosCalc = new Vector3(stopPos.x, 0f, stopPos.z);

        // 2つのVector3から距離を算出
        float distance = Vector3.Distance(startPosCalc, stopPosCalc);
        return distance;
    }

    void SetDistanceText(float distance)
    {
        // 受け取った距離の値を使って画面に表示するテキストをセット
        distanceText.text = distancePrefix + distance.ToString("F2") + distanceSuffix;
    }

    void SetHighScoreText(float distance)
    {
        // 受け取ったハイスコアの値を使って画面に表示するテキストをセット
        highScoreText.text = highScorePrefix + distance.ToString("F2") + highScoreSuffix;
    }
    void OnTriggerEnter(Collider other)
    {
        // 衝突した相手のタグを確認する
        if (other.gameObject.tag == fallCheckerTag)
        {
            // 相手が落下判定用オブジェクトだった時の処理
            StopFlying();
        }
    }

    void CheckInput()
    {
        // キーが押された時の動きを定義

        // Input.GetKeyUpはキーが一度押された後、それが離された時にTrueを返す
        if (Input.GetKeyUp(KeyCode.B) && canButtonPress)
        {
            isBoostPressed = true;
        }

        // 上キーが押された時は、角度変更ボタン(上)が押された時の処理を呼ぶ
        //if (Input.GetKeyDown(KeyCode.UpArrow))
        //{
        //    OnPressedAngleUpButton();
        //}
        //else if (Input.GetKeyUp(KeyCode.UpArrow))
        //{
        //    OnReleasedAngleButton();
        //}

        //// 下キーが押された時は、角度変更ボタン(下)が押された時の処理を呼ぶ
        //if (Input.GetKeyDown(KeyCode.DownArrow))
        //{
        //    OnPressedAngleDownButton();
        //}
        //else if (Input.GetKeyUp(KeyCode.DownArrow))
        //{
        //    OnReleasedAngleButton();
        //}
    }


    //void SetAngleButtonState(bool isInteractable)
    //{
    //    // ボタンが押せるかどうかの状態をセットする
    //    angleUpButton.interactable = isInteractable;
    //    angleDownButton.interactable = isInteractable;
    //}

    void CheckSphereState()
    {
        // ゴールを実装したらコメント解除

        //if (!rb.IsSleeping() || hasReachedGoal)
        //{
        //    return;
        //}
        if (!rb.IsSleeping())
        {
            return;
        }
        //if (isTouchingGoal)
        //{
        //    // ゴールで止まった時の処理
        //    StartCoroutine(GoalAnimation(0.8f));
        //    hasReachedGoal = true;
        //}
        else
        {
            // ゴール以外で止まった時の処理
            ReadyToBoost();
        }
    }

    void ReadyToBoost()
    {
        // 飛行中フラグをFalseにセット
        isFlying = false;

        // 距離測定中フラグをFalseにセット
        isCheckingDistance = false;

        // 現在の距離をリセット
        SetDistanceText(0f);

        // 角度矢印を表示する
        //angleArrowObject.SetActive(true);

        // ボタンを押せるようにする
        //SetAngleButtonState(true);

        // ガイドを表示する
        //guideManager.SetGuidesState(true);

        // ボールの運動状態を確認
        canButtonPress = true;
        boostButton.interactable = canButtonPress;
    }

    Vector3 GetMousePosition()
    {
        // マウス座標を取得
        Vector3 position = Input.mousePosition;
        position.z = mainCameraPos.position.z;
        position = mainCamera.ScreenToWorldPoint(position);

        return position;
    }

    private void OnMouseDown()
    {
        dragStart = GetMousePosition();

        line.enabled = true;
        isDragging = true;
        line.SetPosition(0, rb.position);
        line.SetPosition(1, rb.position);

    }

    private void OnMouseDrag()
    {
            Vector3 position = GetMousePosition();

            currentForce = position - dragStart;
            if (currentForce.magnitude > dragLimit * dragLimit)
            {
                currentForce *= dragLimit / currentForce.magnitude;
            }

            line.SetPosition(0, rb.position);
            line.SetPosition(1, rb.position + currentForce);
    }

    public void OnMouseUp()
    {
        if (isDragging)
        {
            line.enabled = false;
            isDragging = false;
            Flip(currentForce * 2);
        }

    }

    public void Flip(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }

}
