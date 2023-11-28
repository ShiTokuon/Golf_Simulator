using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragController : MonoBehaviour
{

    // ���˕���
    public LineRenderer line;

    // Rigidbody�R���|�[�l���g�ւ̎Q�Ƃ��L���b�V��
    public Rigidbody rb;

    // �h���b�O�ő�t�^�͗�
    public float dragLimit = 5f;

    // ������͂̑傫��
    public float forctToAdd = 10f;

    // �J����
    private Camera camera;
    // �h���b�O���t���O

    private bool isDragging;

    // �}�E�X���W�擾
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

        // Line�̏����l���Z�b�g
        line.positionCount = 2;
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, Vector3.zero);
        line.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        // Input.GetMouseButonDown�̓L�[����x�����ꂽ����True��Ԃ�
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
