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
    private PistolScript pistolScript;
    private Main mainScript;
    private int roundCount = 10;//17;
    private Boolean isMagazineMovingInGun;

    //рельсы при вставке
    private Vector3 magazineOnRailStartPosition = new(0.0f, -0.02f, -0.0008f);	// original local position
    private Vector3 localInitRotation0 = new(25.338f, 0.085f, 0.07f);
    private GameObject magazineRoot;
    private Rigidbody rootRb;
    private Rigidbody rb; 
    
    //joint for magazine
    private ConfigurableJoint configurableJoint;
    private GameObject handGrabInteraction;

    private Boolean enteredPoint1;
    private Boolean readyToLock;
    //todo использовать это при досыле патрона в патронник. в будущем.
    private Boolean magazineIsSetUp;
    private String magazineId = null;

    void Awake() {
        if (pistol == null) {
            pistol = GameObject.Find("Glock17");
        }

        pistolScript = pistol.GetComponent<PistolScript>();
        magazineRoot = pistol.transform.Find("MagazineRoot").gameObject;
        rootRb = magazineRoot.GetComponent<Rigidbody>();
        rb = GetComponent<Rigidbody>();
        
        if (codeObject == null) {
            codeObject = GameObject.Find("codeObject");
        }
        mainScript = codeObject.GetComponent<Main>();
        configurableJoint = GetComponent<ConfigurableJoint>();
        Destroy(configurableJoint); //todo yp убрать надо будет везде джоинты
        handGrabInteraction = transform.Find("ISDK_HandGrabInteraction").gameObject;
        
        String magazineIdNumber = Random.Range(0, 100).ToString();
        if (transform.parent.name == "MagazineRoot") {
            enteredPoint1 = true;
            magazineIsSetUp = true;
            readyToLock = false;
            magazineId = "FromPistol_" + magazineIdNumber;
        } else {
            enteredPoint1 = false;
            magazineIsSetUp = false;
            readyToLock = true;
            magazineId = "FromBag_" + magazineIdNumber;
        }
    }

    private void Update() {
        keepMagazineOnRailsIfNeeded();
        checkPositionIfNeeded();
        lockMagazineIfNeeded();
    }

    private void lockMagazineIfNeeded() {
        if (isMagazineMovingInGun && !readyToLock) {
            if (transform.localPosition.y <= -0.0021f) {
                readyToLock = true;
                magazineIsSetUp = false;
            }
        }    
        
        if (isMagazineMovingInGun && readyToLock) {
            if (transform.localPosition.y >= -7e-05f) { //todo yp после полировки вынести в константу
                magazineLock();
            }
        }
    }

    private void magazineLock() {
        rb.isKinematic = true;
        rb.useGravity = false;

        magazineIsSetUp = true;
        readyToLock = false;
        pistolScript.setMagazineIn(magazineIsSetUp);
    }

    //т.к. я жестко телепортирую магазин куда надо. тригеры срабатывают на enter даже если я на этом тригере и стоял все время.
    //из-за ручной директивной корректции transform 
    //рушение: переписать все тригеры на ручную проверку координаты и выставление флага.
    private void checkPositionIfNeeded() {
        if (isMagazineMovingInGun && !rb.isKinematic && rb.useGravity) {
            if (transform.localPosition.y < -0.035f) { //todo yp после полировки вынести в константу
                magazineFullEjection();
            }
        }
    }
    
    private void magazineFullEjection() {
        transform.SetParent(null);
        Destroy(configurableJoint);
        
        isMagazineMovingInGun = false;
        enteredPoint1 = false;
        handGrabInteraction.SetActive(true);
    }
    
    void OnTriggerEnter(Collider other) {
        if (!enteredPoint1 &&
            other.gameObject.name == "reloadPoint1" 
            && !isMagazineMovingInGun) {

            enteredPoint1 = true;
            ToggleColor();
            handGrabInteraction.SetActive(false);
            mainScript.isHandKeepingMagazine = false;
            isMagazineMovingInGun = true;
            
            if (ReferenceEquals(transform.parent, null) || !transform.parent.name.Contains("Root") 
                && magazineRoot.transform.childCount == 0) {
                transform.SetParent(magazineRoot.transform);
            }
            
            /*if (configurableJoint == null) {
                setUpConfiguredJoint();
            }*/
            
            //todo продумать зашелку. когда до конца вставил.
        } else if (other.gameObject.name == "LeftHandCollider") {
            Debug.Log($" ===== имя руки: {other.gameObject.name}");
        }
    }

    private void keepMagazineOnRailsIfNeeded() {
        if (isMagazineMovingInGun) {
            setLimitedPositionAndRotation();
        }
    }
    
    private void setLimitedPositionAndRotation() {
        transform.localScale = Vector3.one;

        float maxOffsetUp = 0.021f;
        float maxOffsetDown = 0.02f;
        
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
    
    void FixedUpdate() {
        Debug.DrawRay(transform.position, rb.velocity, Color.red);
        Debug.DrawRay(transform.position, rb.angularVelocity, Color.green);
    }
    
    void OnCollisionEnter(Collision col) {
        foreach (var c in col.contacts) {
            Debug.DrawRay(c.point, col.impulse, Color.magenta, 1f);
            Debug.Log("Impulse: " + col.impulse);
        }
    }
    
    private void setUpConfiguredJoint() { 
        configurableJoint = gameObject.AddComponent<ConfigurableJoint>();
        configurableJoint.connectedBody = rootRb;
        configurableJoint.autoConfigureConnectedAnchor = true;
        
        configurableJoint.configuredInWorldSpace = false;
        configurableJoint.xMotion = ConfigurableJointMotion.Free;
        configurableJoint.yMotion = ConfigurableJointMotion.Locked;
        configurableJoint.zMotion = ConfigurableJointMotion.Locked;
        
        configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
        configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
        configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
    }
      
    private bool isColor1Active = true;
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
  
    public int getRoundCount() {
        return roundCount;
    }

    public void decrementRoundCount() {
        roundCount--;
    }

    public void setMagazineIsSetUp(bool isSetUp) {
        this.magazineIsSetUp = isSetUp;
    }
    
    /*public Boolean getMagazineIsSetUp() {
        return magazineIsSetUp;
    }*/
    
    public void setIsMagazineMovingInGun(Boolean isMagazineMovingInGun) {
        this.isMagazineMovingInGun = isMagazineMovingInGun;
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

        SoftJointLimit limit = new SoftJointLimit { limit = 0.2f };
        joint.linearLimit = limit;

        JointDrive drive = new JointDrive {
            positionSpring = 20000f,
            positionDamper = 500f,
            maximumForce = Mathf.Infinity
        };
        joint.yDrive = drive;
    }*/
        
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
}