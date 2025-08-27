using System;
using UnityEngine;
using UnityEngine.Serialization;

public class InsertPositionValidationScript : MonoBehaviour {
    private MagazineScript magazineScript;
    private PistolScript pistolScript;
    private GameObject reloadPoint1;

    private void Start() {
        magazineScript = GetComponentInParent<MagazineScript>();
        reloadPoint1 = magazineScript.reloadPoint1;
        pistolScript = magazineScript.pistolScript;
        
        magazineRootTransform = magazineScript.magazineRoot.transform;
    }

    void OnTriggerEnter(Collider other) {
        if (!magazineScript.isMagazineMovingInGun && other.gameObject.name == "pre-entry") {
            // гарантируем старт «снаружи» (d < 0)
            hasPrevDist = true;
            prevDist = -0.002f;
            gateCrossed = false;
            approachStableTimer = 0f;
        }
    }
    
    void OnTriggerStay(Collider other) {
        if (insertionStarted) return;
        if (other.gameObject.name != "reloadPoint1") { return; }
        if (magazineScript.isMagazineMovingInGun || pistolScript.hasMagazineChild()) { approachStableTimer = 0f; return; }
    
        if (isMagazineApproachValid() 
            && CrossedEntryFromOutside()
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

        Vector3 offset = transform.position - reloadPoint1.transform.position;
        Vector3 allowedWorld = magazineRootTransform.TransformDirection(allowedApproachDirectionLocal).normalized;
        if (Vector3.Dot(offset, allowedWorld) <= 0f) return false;

        Vector3 lateral = offset - Vector3.Project(offset, magazineRootTransform.forward);
        if (lateral.sqrMagnitude > maximumLateralOffsetMeters * maximumLateralOffsetMeters) return false;

        return true;
    }

    private bool gateCrossed = false;  // была ли уже пересечена плоскость
    private bool hasPrevDist = false;  // инициализирован ли prevDist
    private float prevDist;            // прошлое скалярное расстояние
    
    /// Проверка: магазин пересёк входную плоскость шахты с правильной стороны.
    /// Работает по принципу "снаружи → внутрь".
    /// <param name="insertPointAndAxisMag">Точка на магазине (insertPointAndAxisMag)</param>
    /// <param name="reloadPoint1">Вход шахты (reloadPoint1)</param>
    /// <param name="reloadAxisTransform">Трансформ шахты, forward должен смотреть внутрь</param>
    /// <param name="gateCrossed">Флаг-защёлка: однажды пересёк → остаётся true</param>
    /// <param name="hasPrevDist">Был ли уже рассчитан предыдущий d</param>
    /// <param name="prevDist">Предыдущее расстояние до плоскости</param>
    /// <returns>true, если пересечение снаружи → внутрь уже произошло</returns>
    public bool CrossedEntryFromOutside() {
        // ось шахты
        Vector3 axis = magazineRootTransform.up.normalized;
        // скалярное расстояние точки магазина до входной плоскости
        float d = Vector3.Dot(transform.position - reloadPoint1.transform.position, axis);
        // первый кадр — только инициализация
        if (!hasPrevDist) { 
            prevDist = d; 
            hasPrevDist = true; 
            return false; 
        }
        // если раньше было "снаружи" (d < 0), а теперь "внутри" (d >= 0) → засчитываем пересечение
        if (!gateCrossed && prevDist < 0f && d >= 0f) 
            gateCrossed = true; // ⚠ если forward у axisTransform наружу, условие инвертировать
        // обновляем кэш
        prevDist = d;
        return gateCrossed;
    }
    
    void OnDrawGizmos() {
        if (magazineRootTransform == null || reloadPoint1 == null) return;

        Vector3 n = magazineRootTransform.up.normalized; // нормаль
        Vector3 center = reloadPoint1.transform.position;

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
        Vector3 pTip = transform.position - reloadPoint1.transform.position;
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
        if (other.gameObject.name != "reloadPoint1") {
            approachStableTimer = 0f; // если используешь Stay+таймер
            
            //резет флаги для пересейчения плоскости
            hasPrevDist = false; 
            gateCrossed = false; 
            prevDist = 0;
            
            insertionStarted = false;
        }
    }
}
