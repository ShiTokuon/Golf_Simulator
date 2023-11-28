using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragController : MonoBehaviour
{

    // 発射方向
    public LineRenderer line;

    // Rigidbodyコンポーネントへの参照をキャッシュ
    public Rigidbody rb;

    // ドラッグ最大付与力量
    public float dragLimit = 5f;

    // 加える力の大きさ
    public float forctToAdd = 10f;

    // カメラ
    private Camera camera;
    // ドラッグ中フラグ

    private bool isDragging;

    // マウス座標取得
    Vector3 MousePosition
    {
        get
        {
            Vector3 pos = camera.ScreenToViewportPoint(Input.mousePosition);
            pos.z = 0;
            return pos;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;

        // Lineの初期値をセット
        line.positionCount = 2;
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, Vector3.zero);
        line.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        // Input.GetMouseButonDownはキーが一度押された時にTrueを返す
        if (Input.GetMouseButtonDown(0) && !isDragging)
        {
            DragStart();
        }

        if (isDragging)
        {
            Drag();
        }

        if (Input.GetMouseButtonDown(0) && isDragging)
        {
            DragEnd();
        }
    }

    void DragStart()
    {
        line.enabled = true;
        isDragging = true;
        line.SetPosition(0, MousePosition);
    }

    void Drag()
    {
        Vector3 startpos = line.GetPosition(0);
        Vector3 currentpos = MousePosition;

        line.SetPosition(1, currentpos);
    }

    void DragEnd()
    {
        isDragging = false;
        line.enabled = false;
    }
}
