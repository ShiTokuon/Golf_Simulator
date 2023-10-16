using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    // �{�[���I�u�W�F�N�g�ւ̎Q��
    [SerializeField]
    GameObject player;

    // �{�[���I�u�W�F�N�g�ƃ��C���J�����̋���
    Vector3 offset;

    // �J�����̊g�嗦�̍ŏ��l�ƍő�l
    const float OffsetMin = 50f;
    const float OffsetMax = 150f;

    // �J�����g�嗦
    [SerializeField, Range(OffsetMin, OffsetMax)]
    float magnify = 100f;

    // Start is called before the first frame update
    void Start()
    {
        // �I�t�Z�b�g���v�Z����
        offset = gameObject.transform.position - player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        // �J�����̊g�嗦�ɉ������I�t�Z�b�g���擾
        Vector3 magnifiedOffset = GetMagnifiedOffset();

        // �{�[���I�u�W�F�N�g�ƃI�t�Z�b�g����J�����̌��݈ʒu���v�Z
        gameObject.transform.position = player.transform.position + magnifiedOffset;
    }

    Vector3 GetMagnifiedOffset()
    {
        // �K�i�����ꂽ�I�t�Z�b�g���擾
        Vector3 normalizedOffset = offset.normalized;

        // �{�[���I�u�W�F�N�g�ƃJ�����̋������擾
        float offsetDistance = offset.magnitude;

        // offsetDistance�Ɋg�嗦���|���ĕ␳��̋������v�Z
        float magnifiedDistance = offsetDistance * magnify / 100f;

        // �K�i�����ꂽ�x�N�g���Ɗg���̋�������I�t�Z�b�g��Ԃ�
        Vector3 magnifiedOffset = magnifiedDistance * normalizedOffset;
        return magnifiedOffset;
    }
}
