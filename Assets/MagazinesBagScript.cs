using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagazinesBagScript : MonoBehaviour {
    public Transform cameraTransform; // Ссылка на трансформ камеры
    public float verticalOffset = -0.5f; // Смещение по вертикали (например, для пояса)
    
    private Color color1 = Color.red;
    private Color color2 = Color.blue;
    
    private bool isColor1Active = true;

    void Update() {
        // Установить позицию объекта под камерой
        Vector3 newPosition = cameraTransform.position;
        newPosition.y += verticalOffset; // Смещаем объект вниз на заданное расстояние
        transform.position = newPosition;
    }
    
    void OnTriggerEnter(Collider other) {
        //if (collision.gameObject.name == "mixamorig:LeftHand") {
            Debug.Log("схватил магазин");
            ToggleColor();
        //}
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
