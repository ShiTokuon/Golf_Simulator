using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testA : MonoBehaviour
{
    public static bool IsFlagName { private get; set; } = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) || IsFlagName)
        {
            IsFlagName = false;
            Debug.Log(IsFlagName);
        }
    }
}
