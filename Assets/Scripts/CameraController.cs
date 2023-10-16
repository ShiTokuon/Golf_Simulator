using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    // ボールオブジェクトへの参照
    [SerializeField]
    GameObject player;

    // スライダーオブジェクトへの参照
    [SerializeField]
    GameObject sliderObject;

    // ズームテキストオブジェクトへの参照
    [SerializeField]
    GameObject zoomTextObject;

  // ボールオブジェクトとメインカメラの距離
    Vector3 offset;

    // スライダーコンポーネントの参照
    Slider zoomSlider;

    // テキストコンポーネントの参照
    Text zoomText;

    // 拡大率のテキストで変数以外の文字を設定
    string zoomTextPrefix = "拡大率 : ";
    string zoomTextSuffix = "%";

    // 拡大率の最小値と最大値
    const int OffsetMin = 50;
    const int OffsetMax = 150;

    // カメラ拡大率
    [SerializeField, Range(OffsetMin, OffsetMax)]
    int magnify = 100;

    // Start is called before the first frame update
    void Start()
    {
        // オフセットを計算する
        offset = gameObject.transform.position - player.transform.position;

        // 参照を取得
        zoomSlider = sliderObject.GetComponent<Slider>();
        zoomText = zoomTextObject.GetComponent<Text>();
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
        float magnifiedDistance = offsetDistance * (200f - magnify) / 100f;

        // 規格化されたベクトルと拡大後の距離からオフセットを返す
        Vector3 magnifiedOffset = magnifiedDistance * normalizedOffset;
        return magnifiedOffset;
    }

    public void OnChangedMagnifyValue()
    {
        // スライダーの値を拡大率に反映
        magnify = (int)zoomSlider.value;

        // ZoomTextに文字列を設定
        zoomText.text = zoomTextPrefix + magnify.ToString() + zoomTextSuffix;
    }
}
