using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereBooster : MonoBehaviour
{
    bool isFlying = false;

    bool isBoostPressed = false;

    // Sphereオブジェクトの初期位置格納用ベクトル
    Vector3 initPosition = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        initPosition = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isBoostPressed = true;
        }
    }

    void FixedUpdate()
    {
        if (isBoostPressed)
        {
            if (isFlying)
            {

                Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                gameObject.transform.position = initPosition;
            }
            else
            {
                Vector3 forceDirection = new Vector3(1.0f, 1.0f, 0f);

                float forceMagnitude = 10.0f;

                Vector3 force = forceMagnitude * forceDirection;

                Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                rb.AddForce(force, ForceMode.Impulse);
            }

            isFlying = !isFlying;

            isBoostPressed = false;
        }
    }
}
