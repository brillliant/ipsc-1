using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempSCript : MonoBehaviour {
    
    private Color color1 = Color.red;
    private Color color2 = Color.blue;
    
    private bool isColor1Active = true;
    
    void OnTriggerEnter(Collider other) 
    {
        //if (collision.gameObject.name == "mixamorig:LeftHand") {
        Debug.Log("схватил магазин");
        ToggleColor();
        //}
    }
    
    /*void OnTriggerEnter(Collider other)
    {
        ToggleColor();
    }*/
    
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
