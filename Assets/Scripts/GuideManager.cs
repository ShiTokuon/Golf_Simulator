using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideManager : MonoBehaviour
{
    // �{�[���I�u�W�F�N�g�ւ̎Q��
    [SerializeField]
    GameObject player;

    // �^���O��
    [SerializeField]
    private LineRenderer buideLine = null;

    // �Œ�t���[���E�F�C�ƃg
    private static float DeltaTime;

    // �Œ�t���[���̑҂�����
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
