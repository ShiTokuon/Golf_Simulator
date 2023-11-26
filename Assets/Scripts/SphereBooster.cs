using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SphereBooster : MonoBehaviour
{
    public float maxHitForce = 20f;
    public float hitForceMultiplier = 5f;

    private Vector3 dragStartPos;
    private Vector3 dragEndPos;
    private bool isDragging = false;

    private bool isBallFlying = false;

    private void Update()
    {
        if (isBallFlying)
        {
            // �{�[�������ł���ꍇ�̓}�E�X���͂��󂯕t���Ȃ�
            return;
        }

        if (Input.GetMouseButtonDown(0) && !isDragging)
        {
            StartDragging();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDragging();
        }

        if (isDragging)
        {
            UpdateDrag();
        }
    }

    private void StartDragging()
    {
        isDragging = true;
        dragStartPos = GetMouseWorldPosition();
    }

    private void EndDragging()
    {
        isDragging = false;
        dragEndPos = GetMouseWorldPosition();

        // �h���b�O���������Ɋ�Â��ė͂��v�Z���ă{�[���ɉ�����
        Vector3 dragDistance = dragEndPos - dragStartPos;
        float hitForce = Mathf.Min(dragDistance.magnitude * hitForceMultiplier, maxHitForce);

        // �{�[���ɗ͂�������
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(new Vector3(dragDistance.x, 0f, dragDistance.y).normalized * hitForce, ForceMode.Impulse);
        }

        // �{�[�������ł����Ԃɐݒ�
        isBallFlying = true;
    }

    private void UpdateDrag()
    {
        dragEndPos = GetMouseWorldPosition();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // �{�[�����n�ʂƐڐG��������ł����Ԃ�����
        if (collision.gameObject.CompareTag("Plane"))
        {
            isBallFlying = false;
        }
    }
}
