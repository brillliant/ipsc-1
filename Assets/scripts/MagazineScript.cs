using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MagazineScript : MonoBehaviour {
    public GameObject codeObject;
    public GameObject pistol;
    [HideInInspector] public PistolScript pistolScript;
    [HideInInspector] public GameObject reloadPoint2;
    private Main mainScript;
    private int roundCount = 15;
    public Boolean isMagazineMovingInGun;

    private Vector3 localInitRotation0 = new(21.90f, 0.083f, 0.069f);
    [HideInInspector] public GameObject magazineRoot;
    private Rigidbody rb; 
    
    public GameObject handGrabInteraction;

    private Boolean enteredPoint1;
    private Boolean readyToLock;
    private Boolean magazineIsSetUp;
    private String magazineId = null;

    void Awake() {
        if (pistol == null) {
            pistol = GameObject.Find("Glock17");
        }
        reloadPoint2 = pistol.transform.Find("reloadPoint2").gameObject;
        
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

    public void magazineLock() {
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
            if (transform.localPosition.y < -0.032f) { //todo yp после полировки вынести в константу
                magazineFullEjection();
            }
        }
    }
    
    private void magazineFullEjection() {
        transform.SetParent(null);
        
        isMagazineMovingInGun = false;
        enteredPoint1 = false;
        handGrabInteraction.SetActive(true);
        rb.constraints = RigidbodyConstraints.None;
        pistolScript.removeMagazineLink();
    }
    
    public void startInsertion() {
        enteredPoint1 = true;
        handGrabInteraction.SetActive(false);
        mainScript.isHandKeepingMagazine = false;
        isMagazineMovingInGun = true;

        rb.isKinematic = false;
        rb.useGravity = true;

        if (ReferenceEquals(transform.parent, null) || !transform.parent.name.Contains("Root"))
            transform.SetParent(magazineRoot.transform);

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        pistolScript.setMagazineToPistolHierarchy(gameObject);
    }

    private void keepMagazineOnRailsIfNeeded() {
        if (isMagazineMovingInGun) {
            setLimitedPositionAndRotation();
        }
    }
    
    private void setLimitedPositionAndRotation() {
        transform.localScale = Vector3.one;
        
        transform.localPosition = new Vector3(
            reloadPoint2.transform.localPosition.x,
            transform.localPosition.y,
            reloadPoint2.transform.localPosition.z
        );
        transform.localEulerAngles = localInitRotation0;
    }

    void FixedUpdate() {
#if UNITY_EDITOR
        Debug.DrawRay(transform.position, Physics.gravity, Color.blue); // сила гравита
        Debug.DrawRay(transform.position, rb.velocity, Color.red); // линейная скорость
        Debug.DrawRay(transform.position, rb.angularVelocity, Color.green); // угловая скорость
        //Debug.Log($"Velocity: {rb.velocity}, Angular: {rb.angularVelocity}");
#endif
    }
    
#if UNITY_EDITOR
    //бьет по производительности
    void OnCollisionStay(Collision collision) {
        foreach (ContactPoint contact in collision.contacts) {
            Vector3 force = collision.impulse / Time.fixedDeltaTime;
            Debug.DrawRay(contact.point, force * 0.01f, Color.magenta);
            Debug.Log($"Force from {collision.collider.name}: {force}");
        }
    }

    void OnCollisionEnter(Collision col) {
        foreach (var c in col.contacts) {
            Debug.DrawRay(c.point, col.impulse, Color.magenta, 1f);
            Debug.Log("Impulse: " + col.impulse);
        }
    }
#endif
    
    public int getRoundCount() {
        return roundCount;
    }

    public void decrementRoundCount() {
        roundCount--;
        
        if (roundCount == 0) {
            Transform round = transform.Find("round");
            if (round != null) {
                Destroy(round.gameObject);
            }
        } else if (roundCount == 1) {
            Transform round_second = transform.Find("round_second");
            if (round_second != null) {
                Destroy(round_second.gameObject);
            }
        }
    }

    public void setIsMagazineMovingInGunTrue() {
        isMagazineMovingInGun = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }    
}