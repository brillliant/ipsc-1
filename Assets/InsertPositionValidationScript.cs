using System;
using UnityEngine;
using UnityEngine.Serialization;

public class InsertPositionValidationScript : MonoBehaviour {
    private MagazineScript magazineScript;
    private PistolScript pistolScript;
    private GameObject reloadPoint2;

    private void Start() {
        magazineScript = GetComponentInParent<MagazineScript>();
        reloadPoint2 = magazineScript.reloadPoint2;
        pistolScript = magazineScript.pistolScript;
        
        magazineRootTransform = magazineScript.magazineRoot.transform;
    }

    void OnTriggerEnter(Collider other) {
        if (!magazineScript.isMagazineMovingInGun && other.gameObject.name == "reloadPoint1") {
            if (!reloadPoint2Touched) {
                reloadPoint1Touched = true;
            }

            approachStableTimer = 0f;
        }
    }
    
    void OnTriggerStay(Collider other) {
        if (insertionStarted) return;
        if (other.gameObject.name != "reloadPoint2") { return; }
        if (magazineScript.isMagazineMovingInGun || pistolScript.hasMagazineChild()) { approachStableTimer = 0f; return; }
    
        if (isMagazineApproachValid() 
            && touchedReloadPoint2After1()
            && entryInRadius()) {
            
            approachStableTimer += Time.fixedDeltaTime;
            if (approachStableTimer >= approachStabilitySeconds) {
                magazineScript.startInsertion();
                insertionStarted = true;
            }
        } else {
            approachStableTimer = 0f;
        }
    }
    
    float orientationToleranceDegrees = 20f;
    float maximumLateralOffsetMeters = 0.055f;
    [SerializeField] Vector3 allowedApproachDirectionLocal = new Vector3(0f, 1f, 0f);
    [FormerlySerializedAs("reloadAxisTransform")] [SerializeField] private Transform magazineRootTransform;
    
    private bool isMagazineApproachValid() {
        float cosTol = Mathf.Cos(orientationToleranceDegrees * Mathf.Deg2Rad);

        if (Vector3.Dot(transform.forward, magazineRootTransform.forward) < cosTol) return false;
        if (Vector3.Dot(transform.up,      magazineRootTransform.up)      < cosTol) return false;

        Vector3 offset = transform.position - reloadPoint2.transform.position;
        Vector3 allowedWorld = magazineRootTransform.TransformDirection(allowedApproachDirectionLocal).normalized;
        if (Vector3.Dot(offset, allowedWorld) <= 0f) return false;

        Vector3 lateral = offset - Vector3.Project(offset, magazineRootTransform.forward);
        if (lateral.sqrMagnitude > maximumLateralOffsetMeters * maximumLateralOffsetMeters) return false;

        return true;
    }

    private bool reloadPoint1Touched = false;  // инициализирован ли prevDist
    private bool reloadPoint2Touched = false;  // инициализирован ли prevDist
    
    public bool touchedReloadPoint2After1() {
        if (reloadPoint1Touched) {
            reloadPoint2Touched = true;
        }
        return reloadPoint2Touched;
    }
    
    void OnDrawGizmos() {
        if (magazineRootTransform == null || reloadPoint2 == null) return;

        Vector3 n = magazineRootTransform.up.normalized; // нормаль
        Vector3 center = reloadPoint2.transform.position;

        // ищем два вектора, лежащие в плоскости (любые перпендикулярные к n)
        Vector3 tangent = Vector3.Cross(n, Vector3.right);
        if (tangent.sqrMagnitude < 0.001f) tangent = Vector3.Cross(n, Vector3.forward);
        tangent.Normalize();
        Vector3 bitangent = Vector3.Cross(n, tangent);

        float size = 0.05f; // половина стороны квадрата
        Vector3[] corners = new Vector3[4];
        corners[0] = center + (tangent + bitangent) * size;
        corners[1] = center + (tangent - bitangent) * size;
        corners[2] = center + (-tangent - bitangent) * size;
        corners[3] = center + (-tangent + bitangent) * size;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(center, n * 0.2f); // сама нормаль
        Gizmos.color = Color.cyan;
        for (int i = 0; i < 4; i++) {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
    }

    public float radiusMeters = 0.01f;
    
    // 2) ВОРОНКА ВХОДА (отсечь «сбоку»): оба контрольных пункта должны быть в цилиндре у оси
    public bool entryInRadius() {
        Vector3 axis = magazineRootTransform.forward.normalized;
        Vector3 pTip = transform.position - reloadPoint2.transform.position;
        float dTip = Vector3.Dot(pTip,  axis);                  // проекция на ось (глубина)
        Vector3 rTip = pTip  - dTip  * axis;                    // радиальная компонента
        float r2 = radiusMeters * radiusMeters;
        // оба пункта внутри радиуса
        if (rTip.sqrMagnitude  > r2) return false;
        return true;
    }
    
    float approachStabilitySeconds = 0.006f; // таймер
    float approachStableTimer = 0f;
    bool insertionStarted = false;
    
    void OnTriggerExit(Collider other) {
        if (other.gameObject.name != "reloadPoint2") {
            approachStableTimer = 0f;
            //резет флаги для пересейчения плоскости
            reloadPoint1Touched = false;
            reloadPoint2Touched = false;
            //todo вот тут можно не убирать.посомтрим. а то гистерезис получился
            
            insertionStarted = false;
        }
    }
}
