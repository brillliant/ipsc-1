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
    private Boolean isMagazineMovingInGun = false;

    private Boolean magazineIsOut = true;

    //рельсы при вставке
    private Vector3 magazineOnRailStartPosition = new Vector3(0.0f, -0.02527f, -0.0008f);	// original local position
    private Vector3 localInitRotation0 = new Vector3(25.338f, 0.085f, 0.07f);
    private GameObject magazineRoot;
    private Vector3 magazineRootInitialPosition;
    private Vector3 magazineRootInitialRotation;
    private Rigidbody rootRb;
    private Rigidbody rb; 
    
    //joint for magazine
    private ConfigurableJoint configurableJoint;
    private GameObject handGrabInteraction;

    private String magazineId = null;

    void Awake() {
        if (pistol == null) {
            pistol = GameObject.Find("Glock17");
        }
        magazineRoot = pistol.transform.Find("MagazineRoot").gameObject;
        rootRb = magazineRoot.GetComponent<Rigidbody>();
        rb = GetComponent<Rigidbody>();
        
        magazineRootInitialPosition = magazineRoot.transform.localPosition;
        magazineRootInitialRotation = magazineRoot.transform.localEulerAngles;
        
        if (codeObject == null) {
            codeObject = GameObject.Find("codeObject");
        }
        mainScript = codeObject.GetComponent<Main>();
        configurableJoint = GetComponent<ConfigurableJoint>();

        handGrabInteraction = transform.Find("ISDK_HandGrabInteraction").gameObject;
        
        String magazineIdNumber = Random.Range(0, 100).ToString();
        if (transform.parent.name == "MagazineRoot") {
            magazineIsOut = false;
            magazineId = "FromPistol_" + magazineIdNumber;
        } else {
            magazineIsOut = true;
            magazineId = "FromBag_" + magazineIdNumber;
        }
    }

    private void Update() {
        keepMagazineOnRailsIfNeeded();
        checkPositionIfNeeded();
    }

    //т.к. я жестко телепортирую магазин куда надо. тригеры срабатывают на enter даже если я на этом тригере и стоял все время.
    //из-за ручной директивной корректции transform 
    //рушение: переписать все тригеры на ручную проверку координаты и выставление флага.
    private void checkPositionIfNeeded() {
        if (!magazineIsOut && !rb.isKinematic && rb.useGravity) {
            if (transform.localPosition.y < -0.03f) { //todo yp после полировки вынести в константу
                magazineFullEjection();
            }
        }
    }
    
    private void magazineFullEjection() {
        transform.SetParent(null);
        Destroy(configurableJoint);
        
        isMagazineMovingInGun = false;
        magazineIsOut = true;
        
        rb.isKinematic = false;
        rb.useGravity = true;
        handGrabInteraction.SetActive(true);
    }

    private Boolean enteredPoint1 = false;
    
    /*void OnTriggerEnter(Collider other) {
        if (!enteredPoint1 &&
            other.gameObject.name == "reloadPoint1" 
            && !isMagazineMovingInGun 
            && magazineIsOut) {

            enteredPoint1 = true;
            
            ToggleColor();
            /*gameObject.GetComponent<Rigidbody>().isKinematic = false;//временно тру. сделать фолс.
            gameObject.GetComponent<Rigidbody>().useGravity = true;#1#
            
            rb.isKinematic = false;
            rb.useGravity = true;
            var iSDKHandGrabInteractionGameObject = transform.Find("ISDK_HandGrabInteraction").gameObject;
            iSDKHandGrabInteractionGameObject.GetComponent<Grabbable>().gameObject.SetActive(false);

            transform.position = magazineOnRailStartPosition;
            mainScript.isHandKeepingMagazine = false;
            isMagazineMovingInGun = true;
            //isSetUp = true; 
            //todo продумать зашелку. когда до конца вставил.
        } else if (other.gameObject.name == "LeftHandCollider") {
            Debug.Log($" ===== имя руки: {other.gameObject.name}");
        }
    }*/

    private void keepMagazineOnRailsIfNeeded() {
        if (isMagazineMovingInGun) {
            setLimitedPositionAndRotation();
            /*if (ReferenceEquals(magazineJoint, null)) {
                //setUpConfiguredJoint();
            }*/
        }
    }
    
    private void setLimitedPositionAndRotation() {
        transform.localScale = Vector3.one;
        //transform.SetParent(magazineRoot.transform);

        float maxOffsetUp = 0.026f;
        float maxOffsetDown = 0.005f;

        Vector3 currentLocal = transform.localPosition;
        float minY = magazineOnRailStartPosition.y - maxOffsetDown;
        float maxY = magazineOnRailStartPosition.y + maxOffsetUp;

        float clampedY = Mathf.Clamp(currentLocal.y, minY, maxY);

        transform.localPosition = new Vector3(
            magazineOnRailStartPosition.x,
            clampedY,
            magazineOnRailStartPosition.z
        );

        transform.localEulerAngles = localInitRotation0;
    }
    
    /*private void setUpJoint2() {
        ConfigurableJoint joint = gameObject.AddComponent<ConfigurableJoint>();
        Rigidbody rbRoot = magazineRoot.GetComponent<Rigidbody>();
        joint.connectedBody = rbRoot;
        
        // --- вычисление осей по MagazineRoot ---
        Vector3 motionAxis = magazineRoot.transform.up; // направление вдоль шахты
        Vector3 localAxisY = transform.InverseTransformDirection(motionAxis);
        Vector3 localAxisX = transform.InverseTransformDirection(Vector3.Cross(motionAxis, transform.forward));
        
        joint.axis = localAxisX;           // задаёт X джойнта
        joint.secondaryAxis = localAxisY;  // задаёт Y джойнта (вдоль направляющей)

        joint.configuredInWorldSpace = false;
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = Vector3.zero;
        
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Locked;

        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;

        SoftJointLimit limit = new SoftJointLimit { limit = 0.2f };
        joint.linearLimit = limit;
        
        JointDrive drive = new JointDrive {
            positionSpring = 20000f,
            positionDamper = 500f,
            maximumForce = Mathf.Infinity
        };
        joint.yDrive = drive;
    }*/

    /*void OnTriggerExit(Collider other) {
        if (other.gameObject.name == "reloadPoint1" && enteredPoint1) {
            
            magazineFullEjection();
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

    public void setIsMagazineMovingInGun(Boolean isMagazineMovingInGun) {
        this.isMagazineMovingInGun = isMagazineMovingInGun;
    }
    
    public Boolean getIsSetUpInProcess() {
        return isMagazineMovingInGun;
    }

    public Boolean isEnteredPoint1() {
        return enteredPoint1;
    }

    public void setEnteredPoint1(Boolean enteredPoint1) {
        this.enteredPoint1 = enteredPoint1;
    }
    
    /*private void setUpConfiguredJoint() { 
        configurableJoint = gameObject.AddComponent<ConfigurableJoint>();
        configurableJoint.connectedBody = rootRb;
        configurableJoint.autoConfigureConnectedAnchor = true;
        
        configurableJoint.configuredInWorldSpace = false;
        configurableJoint.xMotion = ConfigurableJointMotion.Locked;
        configurableJoint.yMotion = ConfigurableJointMotion.Limited;
        configurableJoint.zMotion = ConfigurableJointMotion.Locked;
    }*/
}