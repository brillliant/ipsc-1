using Oculus.Interaction;
using UnityEngine;

// Перетаскивание меню за ручку (CoinPointer) лучом правого контроллера.
// Наводишь луч на ручку, зажимаешь grip → меню виснет на конце луча,
// развёрнутое перпендикулярно лучу (лицом к игроку), и едет за контроллером.
// Ручка и меню связаны жёстко: тащим за ручку — меню следует, сохраняя свою
// позу относительно неё. BodyLockedFollow на время драга отключается, чтобы не
// возвращал меню на место; обратно включается при переоткрытии меню (в MenuController).
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
        bool grip = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);

        if (!dragging) {
            if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
                tryStartDrag();
        } else if (!grip) {
            dragging = false;                  // отпустил grip — бросили (позиция остаётся)
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

    // поворот вдоль полного направления луча (с наклоном вверх/вниз)
    private Quaternion RayRotation(Ray ray) {
        Vector3 dir = ray.direction;
        if (dir.sqrMagnitude < 0.0001f) dir = menuRoot.forward;
        dir.Normalize();
        // up-подсказка: у почти вертикального луча берём up самого меню, чтобы LookRotation не вырождался
        Vector3 up = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.99f ? menuRoot.up : Vector3.up;
        return Quaternion.LookRotation(dir, up);
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
