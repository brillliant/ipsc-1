using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagazinesBagScript : MonoBehaviour {
    //public Transform cameraTransform; // Ссылка на трансформ камеры
    //public float verticalOffset = -0.5f; // Смещение по вертикали (например, для пояса)
    private float height = 0.7f;

    public GameObject camera; 
    
    private Color color1 = Color.red;
    private Color color2 = Color.blue;
    
    private bool isColor1Active = true;
    
    /*void LateUpdate() {
        Vector3 parentPosition = transform.parent.transform.position;
        
        //parentPosition.z += -0.5f;  //смещаем вперед
        //parentPosition.x += -1.00f;  //смещаем объект влево
        
        Quaternion currentRotation = transform.rotation;
        // Убираем наклон, оставляя вращение только по оси Y
        transform.rotation = Quaternion.Euler(0, currentRotation.eulerAngles.y, 0);
        // Восстанавливаем высоту (Y) объекта
        transform.position = new Vector3(transform.position.x, height, transform.position.z);
    }*/
    
    void LateUpdate() {
        Transform cameraTransform = camera.transform;

        //parentPosition.z += -0.5f;  //смещаем вперед
        //parentPosition.x += -1.00f;  //смещаем объект влево

        transform.rotation = Quaternion.Euler(0, cameraTransform.rotation.eulerAngles.y, 0);
        // Восстанавливаем высоту (Y) объекта
        transform.position = 
            new Vector3(
                cameraTransform.position.x, 
                cameraTransform.position.y - 0.7f, 
                cameraTransform.position.z - 0.1f
            );
    }
    
    void OnTriggerEnter(Collider other) {
        //if (collision.gameObject.name == "mixamorig:LeftHand") {
            Debug.Log("схватил магазин");
            ToggleColor();
            TakeMagazine();
        //}
    }

    private void TakeMagazine() {
        
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
