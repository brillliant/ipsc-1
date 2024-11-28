using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagazinesBagScript : MonoBehaviour {
    private float height = 0.7f;
    public GameObject leftHand; 
    public GameObject magazinePrefub;
    public Boolean isHandKeepingMagazine = false;

    public GameObject camera;
    private GameObject magazine;
    
    private Color color1 = Color.red;
    private Color color2 = Color.blue;
    
    private float change = 0.01f;
    
    private bool isColor1Active = true;

    private Quaternion offsetRotation = Quaternion.Euler(230f, 100f, 160f); 
    
    void LateUpdate() {
        Transform cameraTransform = camera.transform;

        transform.rotation = Quaternion.Euler(0, cameraTransform.rotation.eulerAngles.y, 0);
        transform.position = 
            new Vector3(
                cameraTransform.position.x, 
                cameraTransform.position.y - 0.7f, 
                cameraTransform.position.z
            );
        if (magazine != null) {
            magazine.transform.position = new Vector3(
                leftHand.transform.position.x,// + 0.02f,
                leftHand.transform.position.y,// + 0.02f, //+0.01
                leftHand.transform.position.z// + 0.02f 
            );
            magazine.transform.rotation =
            leftHand.transform.rotation * offsetRotation;
        }
    }
    
    void OnTriggerEnter(Collider other) {
        //if (collision.gameObject.name == "mixamorig:LeftHand") {
            Debug.Log("схватил магазин");
            ToggleColor();
            TakeMagazine();
        //}
    }

    private void TakeMagazine() {
        if (!isHandKeepingMagazine) {
            magazine = Instantiate(
                magazinePrefub,
                new Vector3(
                    leftHand.transform.position.x,
                    leftHand.transform.position.y,
                    leftHand.transform.position.z
                ),
                //leftHand.transform.rotation,
                Quaternion.Euler(
                    leftHand.transform.rotation.eulerAngles.x + 180, 
                    leftHand.transform.rotation.eulerAngles.y, 
                    leftHand.transform.rotation.eulerAngles.z
                ),
                leftHand.transform
            );
            
            isHandKeepingMagazine = true;
        }
    }

    private void ToggleColor() {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) {
            renderer.material.color = isColor1Active ? color1 : color2;
            isColor1Active = !isColor1Active;
        } else {
            Debug.LogWarning("У объекта нет компонента Renderer!");
        }
    }
}
