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

        // �{�[�������ł����Unity���E�̎��Ԃ𑁂�����
        if (!SphereBooster.instance.isDraggChecking)
        {
            newTimeScale = scale;
        }

        // Unity���E�̎��ԂɃX�P�[����K�p
        Time.timeScale = newTimeScale;
    }
}
