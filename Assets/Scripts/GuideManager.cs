using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideManager : MonoBehaviour
{
    // ボールオブジェクトへの参照
    [SerializeField]
    GameObject player;

    // ボールオブジェクトから見たガイドの位置
    Vector3 relativePos = new Vector3(2.0f, 2.0f, 0);

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
        // ガイドの位置をボールオブジェクトへ移動
        gameObject.transform.position = player.transform.position;

        // ガイドの位置にrelativePosを足してPrefabの位置を決定
        Vector3 guidePos = gameObject.transform.position + relativePos;

        // prefabをロードする
        GameObject guidePrefab = (GameObject)Resources.Load("Prefabs/Guide");

        // prefabをインスタンス化
        GameObject guideObject = (GameObject)Instantiate(guidePrefab, guidePos, Quaternion.identity);

        // インスタンス化したオブジェクトをGuideParentの子にする
        guideObject.transform.SetParent(gameObject.transform);

        // オブジェクト名を設定
        guideObject.name = "GuideScript";
    }
}
