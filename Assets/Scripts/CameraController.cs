using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    // ボールオブジェクトへの参照
    [SerializeField]
    GameObject player;

    // ボールオブジェクトとメインカメラの距離
    Vector3 offset;

    // カメラの拡大率の最小値と最大値
    const float OffsetMin = 50f;
    const float OffsetMax = 150f;

    // カメラ拡大率
    [SerializeField, Range(OffsetMin, OffsetMax)]
    float magnify = 100f;

    // Start is called before the first frame update
    void Start()
    {
        // オフセットを計算する
        offset = gameObject.transform.position - player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        // カメラの拡大率に応じたオフセットを取得
        Vector3 magnifiedOffset = GetMagnifiedOffset();

        // ボールオブジェクトとオフセットからカメラの現在位置を計算
        gameObject.transform.position = player.transform.position + magnifiedOffset;
    }

    Vector3 GetMagnifiedOffset()
    {
        // 規格化されたオフセットを取得
        Vector3 normalizedOffset = offset.normalized;

        // ボールオブジェクトとカメラの距離を取得
        float offsetDistance = offset.magnitude;

        // offsetDistanceに拡大率を掛けて補正後の距離を計算
        float magnifiedDistance = offsetDistance * magnify / 100f;

        // 規格化されたベクトルと拡大後の距離からオフセットを返す
        Vector3 magnifiedOffset = magnifiedDistance * normalizedOffset;
        return magnifiedOffset;
    }
}
