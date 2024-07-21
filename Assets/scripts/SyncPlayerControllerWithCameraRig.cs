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
        transform.position = new Vector3(cameraPosition.x, transform.position.y, cameraPosition.z);
    }
}