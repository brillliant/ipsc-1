using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class MagazineScript : MonoBehaviour {
    private int roundCount = 3;//17;
    public Boolean isSetUp = false;

    public int getRoundCount() {
        
        return roundCount;
    }

    public void decrementRoundCount() {
        roundCount--;
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.name == "reloadPoint1" && !isSetUp) {
            if (transform.parent is not null) {
                //ToggleColor();
                transform.SetParent(null);
                gameObject.GetComponent<Rigidbody>().isKinematic = false;
                gameObject.GetComponent<Rigidbody>().useGravity = true;
            }
        }
    }
    
    //todo удалить позже
    private bool isColor1Active = true;
    //todo удалить после отладки
    private Color color1 = Color.red;
    private Color color2 = Color.blue;
    
    private void ToggleColor() {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) {
            renderer.material.color = isColor1Active ? color1 : color2;
            isColor1Active = !isColor1Active;
        } else {
            Debug.LogWarning("У объекта нет компонента Renderer!");
        }
    }

    void Update() {
        
    }
}