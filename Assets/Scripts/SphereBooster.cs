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

    // 加える力の大きさ
    [SerializeField]
    float forceMagnitude = 10.0f;

    // X軸からの角度(90まで設定)
    [SerializeField, Range(0f, 90f)]
    float forceAngle = 45.0f;

    // 力を加える方向
    Vector3 forceDirection = new Vector3(1.0f, 1.0f, 0f);

    // 飛行中フラグ
    bool isFlying = false;

    // ボタン押下フラグ
    bool isBoostPressed = false;

    // 距離測定中フラグ
    bool isCheckingDistance = false;

    // ボールのオブジェクト停止位置格納用ベクトル
    Vector3 stopPosition = Vector3.zero;

    // Sphereオブジェクトの初期位置格納用ベクトル
    Vector3 initPosition = Vector3.zero;

    // Rigidbodyコンポーネントへの参照をキャッシュ
    Rigidbody rb;

    // DistanceTextオブジェクトのTextコンポーネントへの参照をキャッシュ
    Text distanceText;

    // UIテキストのプレフィックス
    string distancePrefix = "飛距離: ";

    // UIテキストのサフィックス
    string distanceSuffix = " m";

    // HighScoereTextオブジェクトのTextコンポーネントへの参照をキャッシュ
    Text highScoreText;

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
        initPosition = gameObject.transform.position;
        rb = gameObject.GetComponent<Rigidbody>();
        distanceText = distanceTextObject.GetComponent<Text>();
        highScoreText = highScoreTextObject.GetComponent<Text>();

        // DistanceTextとHighScoreTextの初期値をセット
        SetDistanceText(0f);
        SetHighScoreText(0f);
    }

    void Update()
    {
        // Input.GetKeyUpはキーが一度押された後、それが離された時にTrueを返す
        if (Input.GetKeyUp(KeyCode.B))
        {
            isBoostPressed = true;
        }

        // forceAngleの変更を反映する
        CalcForceDirection();
    }

    void FixedUpdate()
    {
        CheckDistance();

        if (!isBoostPressed)
        {
            // キーまたはボタンが押されていなければ
            // 処理の切り替えをせず抜ける
            return;
        }
        if (isFlying)
        {
            // 飛行中の処理
            StopFlying();
        }
        else
        {
            // ボールを飛ばす処理
            BoostSphere();
        }

        // 飛行中フラグの切り替え
        isFlying = !isFlying;

        // どちらの処理をしてもボタン押下フラグをfalseに
        isBoostPressed = false;
    }

    void StopFlying()
    {
        // 運動の停止
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 初期位置に移動させる
        gameObject.transform.position = initPosition;

        // 距離測定中をFalseにセット
        isCheckingDistance = false;

        // 現在の距離をリセット
        SetDistanceText(0f);
    }

    void BoostSphere()
    {
        // 向きと力の計算
        Vector3 force = forceMagnitude * forceDirection;

        // 力を加えるメソッド
        rb.AddForce(force, ForceMode.Impulse);

        // 距離測定中をTrueにセット
        isCheckingDistance = true;
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
        float distance = GetDistanceInXZ(initPosition, currentPosition);

        // UIに表示
        SetDistanceText(distance);

        if (rb.IsSleeping())
        {
            // ハイスコアのチェック
            stopPosition = currentPosition;
            float currentDistance = GetDistanceInXZ(initPosition, stopPosition);

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
            // ボールが落下判定用のオブジェクトだった時の処理
            isBoostPressed = true;
        }
    }
}
