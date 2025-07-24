using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MagazinesBagScript : MonoBehaviour {
    public GameObject magazineSpawn; 
    public GameObject magazinePrefub;
    private GameObject codeObject;
    private Main mainScript;
    
    public GameObject camera;
    private GameObject magazine;
    
    //todo удалить после отладки
    private Color color1 = Color.red;
    private Color color2 = Color.blue;
    
    private float change = 0.01f;
    
    private bool isColor1Active = true;

    void Start() {
        codeObject = GameObject.Find("codeObject");
        mainScript = codeObject.GetComponent<Main>();
    }

    //private Quaternion offsetRotation = Quaternion.Euler(230f, 100f, 160f); 
    void LateUpdate() {
        //положение бокса для магазинов
        Transform cameraTransform = camera.transform;

        transform.rotation = Quaternion.Euler(0, cameraTransform.rotation.eulerAngles.y, 0);
        transform.position = 
            new Vector3(
                cameraTransform.position.x, 
                cameraTransform.position.y - 0.9f, 
                cameraTransform.position.z
            );
        
        if (magazine != null && magazine.transform.parent is not null && magazine.transform.parent.name == "magazineSpawn") {
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

            
            //todo удалить после отладки
            ToggleColor();
            
            takeMagazine(other);
        //}
    }

    private void takeMagazine(Collider other) {
        //Debug.Log("========" + other.gameObject.name);
        
        //todo доработать условие. чтобы тольео если ЛЕВАЯ рука попадает в куб - тогда в ней появится магазин.
        //пока, что даже пистолет.
        //other.gameObject.name = "leftHand";
        //if (!isHandKeepingMagazine) {
        /*if (other.gameObject.name.Contains("Hand") || other.gameObject.name.Contains("Rigidbody")
                                                   || other.gameObject.name.Contains("PinchArea")
                                                   || other.gameObject.name.Contains("Visuals")
                                                   || other.gameObject.name.Contains("a")
                                                   || true
                                                   ) {*/
        
        if (other.gameObject.name == "LeftHandCollider") {
            if (!mainScript.isHandKeepingMagazine) {
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
                mainScript.isHandKeepingMagazine = true;
                Debug.Log("схватил магазин");
            }
            else {
                Destroy(magazine);
                mainScript.isHandKeepingMagazine = false;
            }
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
