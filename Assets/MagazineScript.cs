using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class MagazineScript : MonoBehaviour {
    public GameObject codeObject;
    private Main mainScript;
    private int roundCount = 10;//17;
    public Boolean isSetUp = false;
    private ConfigurableJoint configurableJoint;
    
    void Start() {
        if (codeObject == null) {
            codeObject = GameObject.Find("codeObject");
        }
        mainScript = codeObject.GetComponent<Main>();
        configurableJoint = GetComponent<ConfigurableJoint>();
    } 
    
    public int getRoundCount() {
        return roundCount;
    }

    public void decrementRoundCount() {
        roundCount--;
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.name == "reloadPoint1" && !isSetUp) {
            if (transform.parent is not null) {
                ToggleColor();
                transform.SetParent(null);
                setUpJoint(other);

                mainScript.isHandKeepingMagazine = false;
                gameObject.GetComponent<Rigidbody>().isKinematic = false;
                gameObject.GetComponent<Rigidbody>().useGravity = true;
            }
        }
    }
    
    //todo удалить позже
    private bool isColor1Active = true;
    //todo удалить после отладки
    private Color color1 = Color.green;
    private Color color2 = Color.cyan;
    
    private void ToggleColor() {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) {
            renderer.material.color = isColor1Active ? color1 : color2;
            isColor1Active = !isColor1Active;
        } else {
            Debug.LogWarning("У объекта нет компонента Renderer!");
        }
    }

    private void setUpJoint(Collider reloadPoint1) {
        Rigidbody pistolRigidBody = reloadPoint1.transform.parent.GetComponent<Rigidbody>();
        configurableJoint.connectedBody = pistolRigidBody;
        configurableJoint.connectedAnchor = new Vector3(0.0036f, 0.0143734f, -0.0306f);
        configurableJoint.autoConfigureConnectedAnchor = true;
    }
}