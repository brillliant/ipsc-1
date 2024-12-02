using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MagazinesBagScript : MonoBehaviour {
    public GameObject magazineSpawn; 
    public GameObject magazinePrefub;
    public Boolean isHandKeepingMagazine = false;

    public GameObject camera;
    private GameObject magazine;
    
    //todo удалить после отладки
    private Color color1 = Color.red;
    private Color color2 = Color.blue;
    
    private float change = 0.01f;
    
    private bool isColor1Active = true;

    //private Quaternion offsetRotation = Quaternion.Euler(230f, 100f, 160f); 
    void LateUpdate() {
        Transform cameraTransform = camera.transform;

        transform.rotation = Quaternion.Euler(0, cameraTransform.rotation.eulerAngles.y, 0);
        transform.position = 
            new Vector3(
                cameraTransform.position.x, 
                cameraTransform.position.y - 0.7f, 
                cameraTransform.position.z
            );
        
        if (!ReferenceEquals(magazine, null) && magazine.transform.parent is not null) {
            magazine.transform.position = new Vector3(
                magazineSpawn.transform.position.x,
                magazineSpawn.transform.position.y,
                magazineSpawn.transform.position.z
            );
            magazine.transform.rotation = magazineSpawn.transform.rotation;
        }
    }
    
    void OnTriggerEnter(Collider other) {
        //todo сделать для руки.
        //if (collision.gameObject.name == "mixamorig:LeftHand") {
            Debug.Log("схватил магазин");
            
            //todo удалить после отладки
            ToggleColor();
            
            TakeMagazine();
        //}
    }

    private void TakeMagazine() {
        if (!isHandKeepingMagazine) {
            magazine = Instantiate(
                magazinePrefub,
                new Vector3(
                    magazineSpawn.transform.position.x,
                    magazineSpawn.transform.position.y,
                    magazineSpawn.transform.position.z
                ),
                Quaternion.Euler(
                    magazineSpawn.transform.rotation.eulerAngles.x, 
                    magazineSpawn.transform.rotation.eulerAngles.y, 
                    magazineSpawn.transform.rotation.eulerAngles.z
                ),
                magazineSpawn.transform
            );
            isHandKeepingMagazine = true;
        }
    }

    //todo удалить позже
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
