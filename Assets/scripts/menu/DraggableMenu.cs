using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// Перетаскивание меню за ручку (CoinPointer) лучом правого контроллера: навёл луч на ручку, зажал grip или курок —
/// меню висит на конце луча и едет за контроллером. Поворачивается только вокруг вертикали, вслед за
/// горизонтальным направлением луча; наклон и крен меню остаются такими, как при открытии.
/// Ручка и меню связаны жёстко. BodyLockedFollow на время драга отключается, обратно включается при переоткрытии меню.
/// </summary>
public class DraggableMenu : MonoBehaviour {
    private const string HandleName = "CoinPointer";           // имя объекта-ручки в сцене

    // всё ниже находится кодом — в инспекторе не показывается и ничего проставлять не нужно
    private RayInteractor rayInteractor;                       // правый луч из MenuController
    private Collider handle;                                   // CoinPointer
    private BodyLockedFollow bodyLockedFollow;                 // на этом же объекте
    private Transform menuRoot;                                // этот объект

    private bool dragging;
    private float grabDistance;                // на какой дистанции по лучу схватили
    private Vector3 menuToHandleAtGrab;        // world-вектор «меню → ручка» в момент захвата
    private Quaternion menuRotAtGrab;
    private Quaternion rayToMenuAtGrab;        // ориентация меню относительно луча в момент захвата (само-калибровка)
    private Quaternion lastRayYaw = Quaternion.identity;   // последний горизонтальный поворот луча — на случай вертикального луча

    void Awake() {
        menuRoot = transform;
        bodyLockedFollow = GetComponent<BodyLockedFollow>();
    }

    // луч из инспектора, а если не задан — правый луч контроллера из MenuController
    private RayInteractor ResolveRay() {
        if (rayInteractor == null) {
            var mc = FindObjectOfType<MenuController>();
            if (mc != null) rayInteractor = mc.GetRightRayInteractor();
        }
        return rayInteractor;
    }

    // ручка из инспектора, а если не задана — ищем объект "CoinPointer" (в детях меню, затем в сцене)
    private Collider ResolveHandle() {
        if (handle != null) return handle;

        Transform found = null;
        foreach (var t in menuRoot.GetComponentsInChildren<Transform>(true))
            if (t.name == HandleName) { found = t; break; }
        if (found == null) {
            var go = GameObject.Find(HandleName);
            if (go != null) found = go.transform;
        }
        if (found != null) handle = found.GetComponentInChildren<Collider>();
        return handle;
    }

    void Update() {
        // тащить можно и grip'ом, и курком — любой из них
        bool held = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch) ||
                    OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        bool pressed = OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch) ||
                       OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        if (!dragging) {
            if (pressed) tryStartDrag();
        } else if (!held) {
            dragging = false;                  // отпустил — бросили (позиция остаётся)
        } else {
            drag();
        }
    }

    private void tryStartDrag() {
        RayInteractor ri = ResolveRay();
        Collider h = ResolveHandle();
        if (ri == null || h == null) return;

        // ищем ручку среди ВСЕХ попаданий — невидимые коллайдеры (боксы меню, рука) не должны перекрывать
        Ray ray = ri.Ray;
        float handleDist = -1f;
        foreach (var hit in Physics.RaycastAll(ray, 50f)) {
            if (hit.collider == h || hit.collider.transform.IsChildOf(h.transform)) {
                handleDist = hit.distance;
                break;
            }
        }
        if (handleDist < 0f) return;

        dragging = true;
        grabDistance = handleDist;
        menuToHandleAtGrab = h.transform.position - menuRoot.position;
        menuRotAtGrab = menuRoot.rotation;
        // запоминаем, как меню повёрнуто относительно луча прямо сейчас — эту ориентацию и держим
        rayToMenuAtGrab = Quaternion.Inverse(RayRotation(ray)) * menuRoot.rotation;

        if (bodyLockedFollow != null) bodyLockedFollow.enabled = false;   // на случай, если follow ещё где-то активен
    }

    /// <summary>Поворот луча только вокруг вертикали (yaw); у почти вертикального луча — предыдущее значение.</summary>
    private Quaternion RayRotation(Ray ray) {
        Vector3 dir = ray.direction;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return lastRayYaw;
        lastRayYaw = Quaternion.LookRotation(dir.normalized, Vector3.up);
        return lastRayYaw;
    }

    private void drag() {
        RayInteractor ri = ResolveRay();
        if (ri == null) return;
        Ray ray = ri.Ray;

        // держим ту же ориентацию относительно луча, что была при захвате — билборд без магических углов
        Quaternion targetRot = RayRotation(ray) * rayToMenuAtGrab;

        Vector3 targetHandlePos = ray.origin + ray.direction * grabDistance;

        // жёсткая связка «меню ↔ ручка»: смещение поворачивается вместе с меню
        Quaternion rotDelta = targetRot * Quaternion.Inverse(menuRotAtGrab);
        Vector3 handleOffset = rotDelta * menuToHandleAtGrab;

        menuRoot.rotation = targetRot;
        menuRoot.position = targetHandlePos - handleOffset;
    }
}
