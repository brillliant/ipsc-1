using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class script1 : MonoBehaviour {
    public Transform cameraTransform; // Ссылка на трансформ камеры
    void Update() {
        // Установить позицию объекта под камерой
        Vector3 newPosition = cameraTransform.position;
        newPosition.y += -0.3f; // Смещаем объект вниз на заданное расстояние
        
        newPosition.z += -0.3f;
        newPosition.x += -0.1f;
        
        //transform.rotation = Quaternion.Euler(cameraTransform.eulerAngles.x, cameraTransform.eulerAngles.y, cameraTransform.eulerAngles.z);
        transform.position = newPosition;
    }
}
