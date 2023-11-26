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
            // ボールが飛んでいる場合はマウス入力を受け付けない
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

        // ドラッグした距離に基づいて力を計算してボールに加える
        Vector3 dragDistance = dragEndPos - dragStartPos;
        float hitForce = Mathf.Min(dragDistance.magnitude * hitForceMultiplier, maxHitForce);

        // ボールに力を加える
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(new Vector3(dragDistance.x, 0f, dragDistance.y).normalized * hitForce, ForceMode.Impulse);
        }

        // ボールが飛んでいる状態に設定
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
        // ボールが地面と接触したら飛んでいる状態を解除
        if (collision.gameObject.CompareTag("Plane"))
        {
            isBallFlying = false;
        }
    }
}
