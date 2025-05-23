using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Random = UnityEngine.Random;

public class MagazineScript : MonoBehaviour {
    public GameObject codeObject;
    public GameObject pistol;
    private Main mainScript;
    private int roundCount = 10;//17;
    private Boolean isSetUp = true;
    private ConfigurableJoint configurableJoint;
    private Boolean magazineJustDropped = false;

    private String magazineId = "defaule";
    
    public void setIsSetUp(Boolean isSetUp) {
        this.isSetUp = isSetUp;
    }
    
    public Boolean getIsSetUp() {
        return isSetUp;
    }
    
    void Start() {
        if (codeObject == null) {
            codeObject = GameObject.Find("codeObject");
        }
        magazineId = Random.Range(0, 100).ToString();

        if (pistol == null) {
            pistol = GameObject.Find("Glock17");
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
        if (other.gameObject.name == "reloadPoint1" && !isSetUp && magazineJustDropped) {
            //if (transform.parent is not null) {
                ToggleColor();
                gameObject.GetComponent<Rigidbody>().isKinematic = false;//временно тру. сделать фолс.
                
                transform.SetParent(null);
                //transform.SetParent(pistol.transform);
                setUpJoint();

                mainScript.isHandKeepingMagazine = false;
                //gameObject.GetComponent<Rigidbody>().useGravity = false;
                isSetUp = true;
            //}
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject.name == "reloadPoint2" && !magazineJustDropped) {
            magazineJustDropped = true;
        }
    }

    //todo удалить позже
    private bool isColor1Active = true;
    //todo удалить после отладки
    private Color color1 = Color.red;
    private Color color2 = Color.magenta;
    
    private void ToggleColor() {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) {
            renderer.material.color = isColor1Active ? color1 : color2;
            isColor1Active = !isColor1Active;
        } else {
            Debug.LogWarning("У объекта нет компонента Renderer!");
        }
    }

    private void setUpJoint() {
        setConfigurableJoint();
        Rigidbody pistolRigidBody = pistol.GetComponent<Rigidbody>();
        configurableJoint.connectedBody = pistolRigidBody;
        configurableJoint.connectedAnchor = new Vector3(0.0036f, 0.0143734f, -0.0306f);
        configurableJoint.autoConfigureConnectedAnchor = true;
    }
    
    private void setConfigurableJoint() {
        configurableJoint.xMotion = ConfigurableJointMotion.Locked;
        configurableJoint.yMotion = ConfigurableJointMotion.Limited;
        configurableJoint.zMotion = ConfigurableJointMotion.Locked;
        configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
        configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
        configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
    }
}