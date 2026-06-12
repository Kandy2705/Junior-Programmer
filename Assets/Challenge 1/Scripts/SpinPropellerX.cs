using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpinPropellerX : MonoBehaviour
{
    public float rotateSpeed = 1000.0f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
    }
}
