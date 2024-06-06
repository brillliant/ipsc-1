using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePrefabSpawner : MonoBehaviour {
    public GameObject prefab;
    public GameObject previewPrefab;
    private GameObject currentPreview;

    void Start() {
        currentPreview = Instantiate(previewPrefab);
    }

    void Update() {
        Ray ray = new Ray(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch),
            OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward);

        if (Physics.Raycast(ray, out RaycastHit hit)) {
            currentPreview.transform.position = hit.point;
            currentPreview.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            
            // Поворот модели лицом к камере
            Vector3 cameraPosition = Camera.main.transform.position;
            currentPreview.transform.LookAt(new Vector3(cameraPosition.x, currentPreview.transform.position.y, cameraPosition.z));
            
            if (OVRInput.GetDown(OVRInput.Button.One)) {
                Instantiate(prefab, hit.point, currentPreview.transform.rotation);
            }
        }
    }
}
