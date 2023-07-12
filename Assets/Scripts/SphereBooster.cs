using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereBooster : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector3 forceDirection = new Vector3(1.0f, 1.0f, 0f);

        float forceMagnitude = 10.0f;

        Vector3 force = forceMagnitude * forceDirection;

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();

        rb.AddForce(force, ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
