using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Oculus.Interaction;
using UnityEngine;
using Random = UnityEngine.Random;

public class MagazineScript : MonoBehaviour {
    public GameObject codeObject;
    public GameObject pistol;
    private Main mainScript;
    private int roundCount = 10;//17;
    private Boolean isSetUp = false;
    private ConfigurableJoint configurableJoint;
    private Boolean magazineIsOut = true;

    private String magazineId = null;

    void Awake() {
        if (codeObject == null) {
            codeObject = GameObject.Find("codeObject");
        }
        mainScript = codeObject.GetComponent<Main>();
        configurableJoint = GetComponent<ConfigurableJoint>();
        
        //generate id
        String magazineIdNumber = Random.Range(0, 100).ToString();
        if (transform.parent.name == "MagazineRoot") {
            isSetUp = true;
            magazineIsOut = false;
            magazineId = "FromPistol_" + magazineIdNumber;
        } else {
            isSetUp = false;
            magazineIsOut = true;
            magazineId = "FromBag_" + magazineIdNumber;
        }
    }
    
    void Start() {
        if (pistol == null) {
            pistol = GameObject.Find("Glock17");
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.name == "reloadPoint1" && !isSetUp) {
            if (transform.parent is not null) {
                ToggleColor();
                transform.SetParent(null);
                gameObject.GetComponent<Rigidbody>().isKinematic = false;
                gameObject.GetComponent<Rigidbody>().useGravity = true;
            }
        }
    }
    
    /*void OnTriggerEnter(Collider other) {
        if (other.gameObject.name == "reloadPoint1" && !isSetUp && magazineIsOut) {
            ToggleColor();
            gameObject.GetComponent<Rigidbody>().isKinematic = false;//временно тру. сделать фолс.
            gameObject.GetComponent<Rigidbody>().useGravity = true;
            transform.SetParent(null);

            //setUpJoint();

            mainScript.isHandKeepingMagazine = false;
            //gameObject.GetComponent<Rigidbody>().useGravity = false;
            isSetUp = true; 
            //todo продумать зашелку. когда до конца вставил.
        } else if (other.gameObject.name == "LeftHandCollider" && !isSetUp) {
            Debug.Log($" ===== имя руки: {other.gameObject.name}");
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject.name == "reloadPoint2" && !magazineIsOut) {
            magazineIsOut = true;
        }
    }*/

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

    /*private void setUpJoint() {
        setConfigurableJoint();
        Rigidbody pistolRigidBody = pistol.GetComponent<Rigidbody>();
        configurableJoint.connectedBody = pistolRigidBody;
        configurableJoint.connectedAnchor = new Vector3(0.0036f, 0.0143734f, -0.0306f);
        configurableJoint.autoConfigureConnectedAnchor = true;

        //add one grab transformer
        var iSDKHandGrabInteractionGameObject = transform.Find("ISDK_HandGrabInteraction").gameObject;
        var oneGrabTranslateTransformer = iSDKHandGrabInteractionGameObject.AddComponent<OneGrabTranslateTransformer>();
        var constraint = new FloatConstraint {
            Constrain = true
        };
        oneGrabTranslateTransformer.Constraints.MinX = constraint;
        oneGrabTranslateTransformer.Constraints.MaxX = constraint;
        oneGrabTranslateTransformer.Constraints.MaxY = constraint;
        oneGrabTranslateTransformer.Constraints.MinZ = constraint;
        oneGrabTranslateTransformer.Constraints.MaxZ = constraint;

        
        /*var grabbable = iSDKHandGrabInteractionGameObject.GetComponent<Grabbable>();
        grabbable.InjectOptionalOneGrabTransformer(oneGrabTranslateTransformer);
        grabbable.InjectOptionalTwoGrabTransformer(null);#1#
    }*/
    
    /*private void setConfigurableJoint() {
        configurableJoint.xMotion = ConfigurableJointMotion.Locked;
        configurableJoint.yMotion = ConfigurableJointMotion.Limited;
        configurableJoint.zMotion = ConfigurableJointMotion.Locked;
        configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
        configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
        configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
    }*/
        
    public int getRoundCount() {
        return roundCount;
    }

    public void decrementRoundCount() {
        roundCount--;
    }
    
    public void setIsSetUp(Boolean isSetUp) {
        this.isSetUp = isSetUp;
    }
    
    public Boolean getIsSetUp() {
        return isSetUp;
    }
}