using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SyncPlayerControllerWithCameraRig : MonoBehaviour {
    public GameObject cameraRig;
    private Vector3 cameraPosition;
    private Transform cameraTransform;
    private void Start() {
        cameraTransform = cameraRig.transform.Find("TrackingSpace/CenterEyeAnchor");
    }

    void Update() {
        // Обновляем позицию OVRPlayerController в соответствии с позицией OVRCameraRig
        cameraPosition = cameraTransform.position;
        
        
        Vector3 forward = cameraTransform.forward;
        Vector3 up = cameraTransform.up;
        
        transform.position = new Vector3(cameraPosition.x, cameraPosition.y, cameraPosition.z);
        //transform.rotation = new Quaternion(transform.rotation.x, cameraTransform.rotation.y, cameraTransform.rotation.z, transform.rotation.w);
        
        transform.rotation = Quaternion.LookRotation(forward, up);
        Debug.Log("b");
    }
}