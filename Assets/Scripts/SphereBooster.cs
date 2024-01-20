using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class SphereBooster : MonoBehaviour
{
    // DistanceTextオブジェクトへの参照
    [SerializeField]
    GameObject distanceTextObject;

    // HighScoreTextオブジェクトへの参照
    [SerializeField]
    GameObject highScoreTextObject;

    // ゴール用UIテキストオブジェクトへの参照
    [SerializeField]
    GameObject goalTextObject;

    // パーティクルオブジェクトへの参照   
    public GameObject particleObject;

    // 発射方向
    [SerializeField]
    LineRenderer line = null;

    [Range(0.1f, 2.0f)]
    public float speed = 2.0f;

    // スクリプトの参照
    public static SphereBooster instance;

    // 加える力の大きさ
    // ドラッグ最大付与力量
    private const float MaxMagnitude = 5f;

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

    // パーティクルへの参照
    ParticleSystem particle;

    // 飛行中フラグ
    public bool isFlying = true;

    // 距離測定中フラグ
    public bool isCheckingDistance = false;

    // ボタンを押せる状態かどうかのフラグ
    //bool canButtonPress = true;

    // ドラッグ開始フラグ
    public bool isDragStart = false;

    // ドラッグフラグ
    bool isDragging = false;

    // ドラッグ開始できる状態かどうかのフラグ
    public bool isDraggChecking = true;

    // ゴール接触フラグ
    bool isTouchingGoal = false;

    // ゴール済みフラグ
    bool hasReachedGoal = false;

    // ボールのオブジェクト停止位置格納用ベクトル
    Vector3 stopPosition = Vector3.zero;

    // 角度の上限と下限の定義
    const float MaxAngle = 180f;
    const float MinAngle = 0f;

    // 力を加える方向
    Vector3 forceDirection = new Vector3(1.0f, 1.0f, 0f);

    // ボールを発射する直前の位置
    Vector3 prePosition = Vector3.zero;

    // 発射方向の力
    Vector3 currentForce = Vector3.zero;

    // ボール位置
    Vector3 currentPosition = Vector3.zero;

    // メインカメラ
    private Camera mainCamera = null;

    // メインカメラ座標
    private Transform mainCameraPos = null;

    // ドラック開始点
    private Vector3 dragStart = Vector3.zero;

    // 固定フレームウェイト
    private static float DeltaTime;

    // 固定フレーム待ち時間
    private static readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    // ゴールのタグ
    string goalTag = "Finish";

    // 地面のタグ
    string Ground = "Ground";

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

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        Physics.autoSimulation = false;
    }

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        distanceText = distanceTextObject.GetComponent<Text>();
        highScoreText = highScoreTextObject.GetComponent<Text>();
        mainCamera = Camera.main;
        mainCameraPos = mainCamera.transform;
        currentPosition = rb.position;
        DeltaTime = Time.fixedDeltaTime;

        particle = particleObject.GetComponent<ParticleSystem>();

        // DistanceTextとHighScoreTextの初期値をセット
        SetDistanceText(0f);
        SetHighScoreText(0f);
    }

    void Update()
    {
        // マウス操作処理
        MouseDraggs();

        // forceAngleの変更を反映する
        CalcForceDirection();
    }

    void FixedUpdate()
    {
        Physics.Simulate(Time.fixedDeltaTime * speed);

        // 距離の測定
        CheckDistance();

        // ゴール判定
        CheckSphereState();

        if (!isDragging)
        {
            // キーまたはボタンが押されていなければ
            // 処理の切り替えをせず抜ける
            return;
        }

        // ボールの位置を記録
        SphereStorage();

        // 飛行中フラグの切り替え
        isFlying = true;

        // どちらの処理をしてもボタン押下フラグをfalseに
        // マウス用
        isDragging = false;
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

    void Stop_rb()
    {
        // 運動の停止
        rb.velocity = Vector3.zero;

        //  重力の停止
        rb.isKinematic = true;
    }

    void SphereStorage()
    {
        // ボールが発射される時の位置を記録
        prePosition = transform.position;

        // 距離測定中をTrueにセット
        isCheckingDistance = true;
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

        if (other.gameObject.tag == goalTag)
        {
            // 相手がゴールだった時の処理
            isTouchingGoal = true;

            // ボールの運動を強制的に停止
            Stop_rb();
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == Ground)
        {
            Instantiate(particle, this.transform.position, Quaternion.identity);
        }
    }

    void CheckSphereState()
    {

        if (!rb.IsSleeping() || hasReachedGoal)
        {
            return;
        }

        if (isTouchingGoal)
        {
            // ゴールで止まった時の処理
            StartCoroutine(GoalAnimation(0.3f));
            hasReachedGoal = true;
        }
        else
        {
            // ゴール以外で止まった時の処理
            ReadyToBoost();
        }
    }

    IEnumerator GoalAnimation(float time)
    {
        // ゴール時のアニメーション処理
        RectTransform rt = goalTextObject.GetComponent<RectTransform>();

        // ゴールテキストオブジェクトの初期位置と移動先位置
        Vector3 initTextPos = rt.localPosition;
        Vector3 targetPos = Vector3.zero;

        // 引数で渡された秒数を足した時間(現在時刻に足す)
        float finishTime = Time.time + time;

        while (true)
        {
            // 処理完了の時刻に達したかの確認
            float diff = finishTime - Time.time;
            if (diff <= 0)
            {
                break;
            }
            // Lerpを計算するために時間進行度を計算
            float rate = 1 - Mathf.Clamp01(diff / time);

            // 初期位置から移動先位置までの直線上で、割合に応じた位置をセット
            rt.localPosition = Vector3.Lerp(initTextPos, targetPos, rate);

            // 1フレーム待機
            yield return null;
        }
        // 移動先位置をセット
        rt.localPosition = targetPos;
    }

    void ReadyToBoost()
    {
        // 飛行中フラグをFalseにセット
        isFlying = false;

        // 距離測定中フラグをFalseにセット
        isCheckingDistance = false;

        // 現在の距離をリセット
        SetDistanceText(0f);

        isDragging = true;
        isDraggChecking = true;
    }

    Vector3 GetMousePosition()
    {
        // マウス座標を取得
        var position = Input.mousePosition;
        position.z = mainCameraPos.position.z;
        position = mainCamera.ScreenToWorldPoint(position);
        position.z = 0;

        return position;
    }

    void DragStart()
    {
        dragStart = GetMousePosition();
        currentPosition = rb.position;

        line.enabled = true;
        line.SetPosition(0, currentPosition);
        line.SetPosition(1, currentPosition);

    }

    void Drag()
    {
        currentPosition = rb.position;
        line.enabled = true;

        var position = GetMousePosition();

        currentForce = position - dragStart;
        if (currentForce.magnitude > MaxMagnitude)
        {
            currentForce *= MaxMagnitude / currentForce.magnitude;
        }

        line.SetPosition(0, currentPosition);
        line.SetPosition(1, currentPosition + currentForce);
    }

    void DragEnd()
    {
        line.enabled = false;
        isDraggChecking = false;
        isDragStart = false;
        Flip(currentForce * 3f);
    }

    void MouseDraggs()
    {
        // マウスをドラッグ中
        if (Input.GetMouseButton(0) && isDraggChecking)
        {
            if (isDragStart == false)
            {
                isDragStart = true;
                DragStart();
            }
            Drag();
        }

        // マウスクリックが解除した時
        if (Input.GetMouseButtonUp(0) && isDraggChecking)
        {
            DragEnd();
        }
    }

    public void Flip(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }
}