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
    private Boolean isSetUpInProcess = false;
    private ConfigurableJoint configurableJoint;
    private Boolean magazineIsOut = true;

    //рельсы при вставке
    private Vector3 magazineOnRailStartPosition = new Vector3(0.0f, -0.02527f, -0.0008f);	// original local position
    private Vector3 localInitRotation0 = new Vector3(25.338f, 0.085f, 0.07f);
    private GameObject magazineRoot;
    private Vector3 magazineRootInitialPosition;
    private Vector3 magazineRootInitialRotation;
    private Rigidbody rootRb;
    private Rigidbody rb; 

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

    }

    private void Update() {
        keepMagazineOnRailsIfNeeded();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.name == "reloadPoint1" 
            && !isSetUpInProcess 
            && magazineIsOut) {
            
            ToggleColor();
            /*gameObject.GetComponent<Rigidbody>().isKinematic = false;//временно тру. сделать фолс.
            gameObject.GetComponent<Rigidbody>().useGravity = true;*/

            // - СТАРТ точно надо и точно работет 
            transform.SetParent(null); //открепить от руки
            rb.isKinematic = false;
            rb.useGravity = true;
        
            //убрать хватательность магазина
            var iSDKHandGrabInteractionGameObject = transform.Find("ISDK_HandGrabInteraction").gameObject;
            iSDKHandGrabInteractionGameObject.GetComponent<Grabbable>().gameObject.SetActive(false);
            // - ФИНИШ

            transform.position = magazineOnRailStartPosition;
            
            mainScript.isHandKeepingMagazine = false;
            isSetUpInProcess = true;
            //isSetUp = true; 
            //todo продумать зашелку. когда до конца вставил.
        } else if (other.gameObject.name == "LeftHandCollider" && !isSetUp) {
            Debug.Log($" ===== имя руки: {other.gameObject.name}");
        }
    }

    private void keepMagazineOnRailsIfNeeded() {
        if (isSetUpInProcess) {
            //transform.localScale = Vector3.one;
            /*transform.SetParent(magazineRoot.transform);
            Vector3 local = magazineRoot.transform.InverseTransformPoint(transform.position);
            local.x = 0f;
            local.z = 0f;
            transform.position = magazineRoot.transform.TransformPoint(local);
            
            transform.localEulerAngles = localInitRotation0;*/
            
            // transform.localPosition = Vector3.zero;
            //magazineRoot.transform.localPosition = new Vector3(magazineRootInitialPosition.x, magazineRoot.transform.localPosition.y, magazineRootInitialPosition.z);
            //magazineRoot.transform.localEulerAngles = magazineRootInitialRotation;
            //rootRb.isKinematic = false;
            //rootRb.useGravity = true;
            
            //тест лижбы работало.
            transform.localScale = Vector3.one;
            transform.SetParent(magazineRoot.transform);
            float maxOffset = 0.026f;
            Vector3 currentLocal = transform.localPosition;
            float shift = currentLocal.y - magazineOnRailStartPosition.y;
            shift = Mathf.Clamp(shift, 0f, maxOffset); // только вперёд
            transform.localPosition = new Vector3(magazineOnRailStartPosition.x, magazineOnRailStartPosition.y + shift, magazineOnRailStartPosition.z);
            transform.localEulerAngles = localInitRotation0;
            
        }    
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

    void OnTriggerExit(Collider other) {
        if (other.gameObject.name == "reloadPoint2" && !magazineIsOut) {
            magazineIsOut = true;
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

    public void setIsSetUpInProcess(Boolean isSetUpInProcess) {
        this.isSetUpInProcess = isSetUpInProcess;
    }
    
    public Boolean getIsSetUpInProcess() {
        return this.isSetUpInProcess;
    }
    
    public void setIsSetUp(Boolean isSetUp) {
        this.isSetUp = isSetUp;
    }
    
    public Boolean getIsSetUp() {
        return isSetUp;
    }
}