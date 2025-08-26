using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MagazineScript : MonoBehaviour {
    public GameObject codeObject;
    public GameObject pistol;
    private PistolScript pistolScript;
    private GameObject reloadPoint1;
    private Main mainScript;
    private int roundCount = 8;
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
        rb.constraints = RigidbodyConstraints.None;
        pistolScript.removeMagazineLink();
        insertionStarted = false;
    }
    
    [SerializeField] Transform insertPointAndAxisMag;
    float orientationToleranceDegrees = 15f;
    float maximumLateralOffsetMeters = 0.055f;
    float minimumApproachSpeedMetersPerSecond = 0.000000005f; //todo можно выпилить
    [SerializeField] Vector3 allowedApproachDirectionLocal = new Vector3(0f, -1f, 0f);
    [SerializeField] private Transform reloadAxisTransform;
    
    private float _prevAxisDist;
    private bool  _hasPrevAxisDist;
    private bool isMagazineApproachValid() {
        float cosTol = Mathf.Cos(orientationToleranceDegrees * Mathf.Deg2Rad);

        if (Vector3.Dot(insertPointAndAxisMag.forward, reloadAxisTransform.forward) < cosTol) return false;
        if (Vector3.Dot(insertPointAndAxisMag.up,      reloadAxisTransform.up)      < cosTol) return false;

        Vector3 offset = insertPointAndAxisMag.position - reloadPoint1.transform.position;
        Vector3 allowedWorld = reloadAxisTransform.TransformDirection(allowedApproachDirectionLocal).normalized;
        if (Vector3.Dot(offset, allowedWorld) <= 0f) return false;

        Vector3 lateral = offset - Vector3.Project(offset, reloadAxisTransform.forward);
        if (lateral.sqrMagnitude > maximumLateralOffsetMeters * maximumLateralOffsetMeters) return false;

        return true;
    }

    private bool gateCrossed = false;  // была ли уже пересечена плоскость
    private bool hasPrevDist = false;  // инициализирован ли prevDist
    private float prevDist;            // прошлое скалярное расстояние
    
    /// <summary>
    /// Проверка: магазин пересёк входную плоскость шахты с правильной стороны.
    /// Работает по принципу "снаружи → внутрь".
    /// </summary>
    /// <param name="insertPointAndAxisMag">Точка на магазине (insertPointAndAxisMag)</param>
    /// <param name="reloadPoint1">Вход шахты (reloadPoint1)</param>
    /// <param name="reloadAxisTransform">Трансформ шахты, forward должен смотреть внутрь</param>
    /// <param name="gateCrossed">Флаг-защёлка: однажды пересёк → остаётся true</param>
    /// <param name="hasPrevDist">Был ли уже рассчитан предыдущий d</param>
    /// <param name="prevDist">Предыдущее расстояние до плоскости</param>
    /// <returns>true, если пересечение снаружи → внутрь уже произошло</returns>
    public bool CrossedEntryFromOutside(
        Transform insertPointAndAxisMag,
        GameObject reloadPoint1,
        Transform reloadAxisTransform
    ) {
        // ось шахты
        Vector3 axis = reloadAxisTransform.forward.normalized;

        // скалярное расстояние точки магазина до входной плоскости
        float d = Vector3.Dot(insertPointAndAxisMag.position - reloadPoint1.transform.position, axis);

        // первый кадр — только инициализация
        if (!hasPrevDist) { 
            prevDist = d; 
            hasPrevDist = true; 
            return false; 
        }

        // если раньше было "снаружи" (d > 0), а теперь "внутри" (d <= 0) → засчитываем пересечение
        if (!gateCrossed && prevDist > 0f && d <= 0f) 
            gateCrossed = true; // ⚠ если forward у axisTransform наружу, условие инвертировать

        // обновляем кэш
        prevDist = d;

        // возвращаем состояние защёлки
        return gateCrossed;
    }

    //public Transform heel;
    public float radiusMeters = 0.01f;
    //private float minDepthMeters = 0.01f;
    
    // 2) ВОРОНКА ВХОДА (отсечь «сбоку»): оба контрольных пункта должны быть в цилиндре у оси
    public bool entryInRadius() {
        Vector3 axis = reloadAxisTransform.forward.normalized;

        Vector3 pTip = insertPointAndAxisMag.position - reloadPoint1.transform.position;
        //Vector3 pHeel = heel.position - reloadPoint1.transform.position;

        float dTip = Vector3.Dot(pTip,  axis);                  // проекция на ось (глубина)
        //float dHeel = Vector3.Dot(pHeel, axis);

        Vector3 rTip = pTip  - dTip  * axis;                    // радиальная компонента
        //Vector3 rHeel = pHeel - dHeel * axis;

        float r2 = radiusMeters * radiusMeters;

        // оба пункта внутри радиуса
        if (rTip.sqrMagnitude  > r2) return false;
        //if (rHeel.sqrMagnitude > r2) return false;

        // оба пункта достаточно глубоко «внутри» (при соглашении d<=0 — внутри)
        //if (dTip  > -minDepthMeters)  return false;
        //if (dHeel > -minDepthMeters)  return false;

        return true;
    }
    
    float approachStabilitySeconds = 0.006f; // таймер
    float approachStableTimer = 0f;
    bool insertionStarted = false;
    
    void OnTriggerStay(Collider other) {
        if (insertionStarted) return;
        if (other.gameObject.name != "reloadPoint1") { return; }
        if (isMagazineMovingInGun || pistolScript.hasMagazineChild()) { approachStableTimer = 0f; return; }
    
        if (isMagazineApproachValid() 
            && CrossedEntryFromOutside(insertPointAndAxisMag, reloadPoint1, reloadAxisTransform)
            && entryInRadius()) {
            
            approachStableTimer += Time.fixedDeltaTime; // физика → fixed
            if (approachStableTimer >= approachStabilitySeconds) {
                startInsertion();
                insertionStarted = true;
            }
        } else {
            approachStableTimer = 0f;
        }
    }
    
    void startInsertion() {
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
        insertionStarted = false;
    }
    
    void OnTriggerExit(Collider other) {
        if (other.gameObject.name != "reloadPoint1") {
            approachStableTimer = 0f; // если используешь Stay+таймер
            _hasPrevAxisDist = false;
            _prevAxisDist = 0f; // опционально
        }
    }
    
    /*void OnTriggerEnter(Collider other) {
        if (!enteredPoint1 &&
            other.gameObject.name == "reloadPoint1" 
            && !isMagazineMovingInGun && !pistolScript.hasMagazineChild()) {

            enteredPoint1 = true;
            handGrabInteraction.SetActive(false);
            mainScript.isHandKeepingMagazine = false;
            isMagazineMovingInGun = true;
            
            rb.isKinematic = false;
            rb.useGravity = true;
            
            if (ReferenceEquals(transform.parent, null) || !transform.parent.name.Contains("Root")) {
                transform.SetParent(magazineRoot.transform);
            }

            rb.constraints = RigidbodyConstraints.FreezeRotation;
            pistolScript.setMagazineToPistolHierarchy(gameObject);
        }
    }*/

    private void keepMagazineOnRailsIfNeeded() {
        if (isMagazineMovingInGun) {
            setLimitedPositionAndRotation();
        }
    }
    
    private void setLimitedPositionAndRotation() {
        transform.localScale = Vector3.one;
        
        Debug.Log($"local Y {transform.localPosition.y}");
        
        transform.localPosition = new Vector3(
            reloadPoint1.transform.localPosition.x,
            transform.localPosition.y,
            reloadPoint1.transform.localPosition.z
        );
        transform.localEulerAngles = localInitRotation0;
    }

    void FixedUpdate() {
        Debug.DrawRay(transform.position, Physics.gravity, Color.blue); // сила гравита
        Debug.DrawRay(transform.position, rb.velocity, Color.red); // линейная скорость
        Debug.DrawRay(transform.position, rb.angularVelocity, Color.green); // угловая скорость
        //Debug.Log($"Velocity: {rb.velocity}, Angular: {rb.angularVelocity}");
    }
    
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