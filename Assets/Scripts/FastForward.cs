using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastForward : MonoBehaviour
{
    [SerializeField]
    float scale = 2.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ChecTimeScale();
    }

    void ChecTimeScale()
    {
        float newTimeScale = 1.0f;

        // ボールが飛んでいる間Unity世界の時間を早くする
        if (!SphereBooster.instance.isDraggChecking)
        {
            newTimeScale = scale;
        }

        // Unity世界の時間にスケールを適用
        Time.timeScale = newTimeScale;
    }
}
