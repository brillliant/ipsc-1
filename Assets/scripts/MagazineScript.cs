using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MagazineScript : MonoBehaviour {
    public GameObject codeObject;
    public GameObject pistol;
    private PistolScript pistolScript;
    private GameObject reloadPoint1;
    private Main mainScript;
    private int roundCount = 5;//17;
    private Boolean isMagazineMovingInGun;

    private Vector3 localInitRotation0 = new(21.90f, 0.083f, 0.069f);
    private GameObject magazineRoot;
    private Rigidbody rb; 
    
    private GameObject handGrabInteraction;

    private Boolean enteredPoint1;
    private Boolean readyToLock;
    private Boolean magazineIsSetUp;
    private String magazineId = null;

    void Awake() {
        if (pistol == null) {
            pistol = GameObject.Find("Glock17");
        }
        reloadPoint1 = pistol.transform.Find("reloadPoint1").gameObject;
        
        pistolScript = pistol.GetComponent<PistolScript>();
        magazineRoot = pistol.transform.Find("MagazineRoot").gameObject;
        
        rb = GetComponent<Rigidbody>();
        
        if (codeObject == null) {
            codeObject = GameObject.Find("codeObject");
        }
        mainScript = codeObject.GetComponent<Main>();
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
        pistolScript.setMagazineLocked(magazineIsSetUp);
        
        transform.localPosition = Vector3.zero;
        isMagazineMovingInGun = false;
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
        
        isMagazineMovingInGun = false;
        enteredPoint1 = false;
        handGrabInteraction.SetActive(true);
    }
    
    void OnTriggerEnter(Collider other) {
        if (!enteredPoint1 &&
            other.gameObject.name == "reloadPoint1" 
            && !isMagazineMovingInGun && magazineRoot.transform.childCount == 0) {

            enteredPoint1 = true;
            ToggleColor();
            handGrabInteraction.SetActive(false);
            mainScript.isHandKeepingMagazine = false;
            isMagazineMovingInGun = true;
            
            rb.isKinematic = false;
            rb.useGravity = true;
            
            if (ReferenceEquals(transform.parent, null) || !transform.parent.name.Contains("Root")) {
                transform.SetParent(magazineRoot.transform);
            }

            setLimitedPositionAndRotation();
        }
    }

    private void keepMagazineOnRailsIfNeeded() {
        if (isMagazineMovingInGun) {
            setLimitedPositionAndRotation();
        }
    }
    
    private void setLimitedPositionAndRotation() {
        transform.localScale = Vector3.one;

        transform.localPosition = new Vector3(
            reloadPoint1.transform.localPosition.x,
            transform.localPosition.y,
            reloadPoint1.transform.localPosition.z
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

    public void setIsMagazineMovingInGun(Boolean isMagazineMovingInGun) {
        this.isMagazineMovingInGun = isMagazineMovingInGun;
    }    
}