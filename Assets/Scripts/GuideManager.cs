using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideManager : MonoBehaviour
{
    // �{�[���I�u�W�F�N�g�ւ̎Q��
    [SerializeField]
    GameObject player;

    // �{�[���I�u�W�F�N�g���猩���K�C�h�̈ʒu
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
        // �K�C�h�̈ʒu���{�[���I�u�W�F�N�g�ֈړ�
        gameObject.transform.position = player.transform.position;

        // �K�C�h�̈ʒu��relativePos�𑫂���Prefab�̈ʒu������
        Vector3 guidePos = gameObject.transform.position + relativePos;

        // prefab�����[�h����
        GameObject guidePrefab = (GameObject)Resources.Load("Prefabs/Guide");

        // prefab���C���X�^���X��
        GameObject guideObject = (GameObject)Instantiate(guidePrefab, guidePos, Quaternion.identity);

        // �C���X�^���X�������I�u�W�F�N�g��GuideParent�̎q�ɂ���
        guideObject.transform.SetParent(gameObject.transform);

        // �I�u�W�F�N�g����ݒ�
        guideObject.name = "GuideScript";
    }
}
