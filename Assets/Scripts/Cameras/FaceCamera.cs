using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{

    private Transform mainCameraTransform;

    void Start()
    {
        mainCameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        //Done this way to work with vertical changes
        gameObject.transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward);
    }
}
