using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideManager : MonoBehaviour
{
    // ボールオブジェクトへの参照
    [SerializeField]
    GameObject player;

    // 運動軌跡
    [SerializeField]
    private LineRenderer buideLine = null;

    // 固定フレームウェイとト
    private static float DeltaTime;

    // 固定フレームの待ち時間
    private static WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    // Start is called before the first frame update
    void Start()
    {
        InstantiateGuidePrefabs();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InstantiateGuidePrefabs()
    {
    }
}
