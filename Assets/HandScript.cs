using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*todo на работает. создал. не пригодилось пока. идея чтобы рука знала, есть ли у нее касание с чем либо.*/
public class HandScript : MonoBehaviour {
    public GameObject cube;
    
    void OnTriggerEnter(Collider other) {
        //if (other.gameObject.name == "reloadPoint1" && !isSetUp) {
            //if (transform.parent is not null) {
                ToggleColor();
                transform.SetParent(null);
                gameObject.GetComponent<Rigidbody>().isKinematic = false;
                gameObject.GetComponent<Rigidbody>().useGravity = true;
            //}
        //}
    }
    
    //todo удалить позже
    private bool isColor1Active = true;
    private Color color1 = Color.magenta;
    private Color color2 = Color.white;
    
    private void ToggleColor() {
        Renderer renderer = cube.GetComponent<Renderer>();
        //Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) {
            renderer.material.color = isColor1Active ? color1 : color2;
            isColor1Active = !isColor1Active;
        } else {
            Debug.LogWarning("У объекта нет компонента Renderer!");
        }
    }
}
