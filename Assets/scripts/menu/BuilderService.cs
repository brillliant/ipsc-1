using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class BuilderService : MonoBehaviour {
    [Header("Prefabs")]
    public GameObject ipscTargetPreview;
    public GameObject ipscTargetPrefab;
    public GameObject ipscTargetNoShotPreview;
    public GameObject ipscTargetNoShotPrefab;
    public GameObject uspsaTargetPreview;
    public GameObject uspsaTargetPrefab;
    public GameObject uspsaTargetNoShotPreview;
    public GameObject uspsaTargetNoShotPrefab;
    public GameObject barrelPreview;
    public GameObject barrelPrefab;
    public GameObject wallPreview;
    public GameObject wallPrefab;

    [SerializeField] private Transform readyStagesContainer;
    [SerializeField] private Transform stageRoot;
    
    [SerializeField] private GameObject deleteConfirmDialog;
    private string stageToDelete;
    
    public GameObject loadStageButtonPrefub;

    [SerializeField] private RayInteractor rayInteractor;

    [SerializeField] private Material shootingZoneMaterial;
    [SerializeField] private GameObject zoneHintMessage;   // тот же текстовый объект, что reset_floor_message

    private CompetitionModeService competitionModeService;
    private readonly List<GameObject> установленныеМишени = new List<GameObject>();
    public readonly List<GameObject> пробоины = new List<GameObject>();

    private const float SnapRadius   = 0.15f;   // 15 см — магнит к уже поставленной точке
    private const float LineWidth    = 0.05f;   // 5 см
    private const float FloorOffset  = 0.005f;  // против z-fighting с полом
    private const float ZonePickThreshold = 0.12f;  // радиус попадания лучом по линии в режиме Remove

    private readonly List<GameObject> зоныСтрельбы = new List<GameObject>();
    private readonly List<Vector3> zonePoints = new List<Vector3>(); // world-space пока рисуем
    private GameObject currentZone;        // активная рисуемая
    private LineRenderer zoneLine;          // зафиксированные сегменты
    private LineRenderer zonePreviewLine;   // от последней точки к курсору
    private bool inZoneMode;
    private bool zoneTriggerPressed;

    private GameObject hoveredZone;        // зона под лучом в режиме Remove
    private int hoveredEdge = -1;          // индекс наведённого ребра в hoveredZone
    private LineRenderer edgeHighlight;    // подсветка наведённого ребра

    private const float RepeatDelay    = 0.4f;   // задержка перед авто-повтором удержания стика
    private const float RepeatInterval = 0.08f;  // интервал авто-повтора
    private OVRInput.Button repeatButton = OVRInput.Button.None;
    private float nextRepeatTime;

    private GameObject currentPreview;
    private List<ObjectData> objectDataList = new List<ObjectData>();
    private bool triggerPressed;
    private GameObject hoveredObject;
    private Dictionary<string, GameObject> prefabMap;
    public GameObject saveStageDialog;
    
    [SerializeField] private TMP_InputField nameField;

    void Start() {
        competitionModeService = GetComponent<CompetitionModeService>();
        prefabMap = new Dictionary<string, GameObject> {
            { ipscTargetPrefab.name,      ipscTargetPrefab      },
            { ipscTargetNoShotPrefab.name, ipscTargetNoShotPrefab },
            { uspsaTargetPrefab.name,      uspsaTargetPrefab      },
            { uspsaTargetNoShotPrefab.name, uspsaTargetNoShotPrefab },
            { barrelPrefab.name,           barrelPrefab           },
            { wallPrefab.name,             wallPrefab             },
        };
    }
    
    private void OnDisable() {
        competitionModeService.stateEnum = StateEnum.Idle;
    }
    
    public void cancelSaveStage() {
        nameField.text = "";
        saveStageDialog.SetActive(false);
    }

    // сдвинуть весь стейдж по высоте (объекты и линии — дети stageRoot, едут вместе)
    public void ShiftByFloorDelta(float deltaY) {
        stageRoot.position += Vector3.up * deltaY;
    }

    void Update() {
        if (competitionModeService.isShootMode()) return;

        if (inZoneMode && competitionModeService.stateEnum != StateEnum.DrawShootingZone) exitZoneMode();

        if (competitionModeService.stateEnum == StateEnum.IPSC_target) {
            buildWith(ipscTargetPreview, ipscTargetPrefab);
        } else if (competitionModeService.stateEnum == StateEnum.IPSC_noshot) {
            buildWith(ipscTargetNoShotPreview, ipscTargetNoShotPrefab);
        } else if (competitionModeService.stateEnum == StateEnum.USPSA_target) {
            buildWith(uspsaTargetPreview, uspsaTargetPrefab);
        } else if (competitionModeService.stateEnum == StateEnum.USPSA_noshot) {
            buildWith(uspsaTargetNoShotPreview, uspsaTargetNoShotPrefab);
        } else if (competitionModeService.stateEnum == StateEnum.Barrel) {
            buildWith(barrelPreview, barrelPrefab);
        } else if (competitionModeService.stateEnum == StateEnum.Wall) {
            buildWith(wallPreview, wallPrefab);
        } else if (competitionModeService.stateEnum == StateEnum.Remove) {
            updateRemoveHighlight();
            tryRemoveHovered();
        } else if (competitionModeService.stateEnum == StateEnum.MoveStage) {
            moveStage();
        } else if (competitionModeService.stateEnum == StateEnum.DrawShootingZone) {
            drawShootingZone();
        } else {
            clearHoverHighlight();
        }
    }

    private void buildWith(GameObject preview, GameObject prefab) {
        setUpObject(preview, prefab);
        competitionModeService.hideCommandsText();
    }

    public void clearPreview() {
        if (!currentPreview) return;
        currentPreview.SetActive(false);
        Destroy(currentPreview);
        currentPreview = null;
    }

    private void setUpObject(GameObject preview, GameObject prefab) {
        if (rayInteractor.State != InteractorState.Normal) {
            clearPreview();
            return;
        }

        if (!currentPreview) currentPreview = Instantiate(preview);
        if (currentPreview && !currentPreview.activeSelf)
            currentPreview.SetActive(true);

        Ray ray = rayInteractor.Ray;

        if (Physics.Raycast(ray, out RaycastHit hit) && !hit.collider.gameObject.name.Equals("emptyObjectForCollider")
                                                     && !hit.collider.gameObject.name.Equals("Glock17")) {
            placeToSurface(currentPreview, hit);

            if (competitionModeService.stateEnum == StateEnum.Wall) {
                if (ThumbstickStep(OVRInput.Button.PrimaryThumbstickLeft)) rotateLeft();
                if (ThumbstickStep(OVRInput.Button.PrimaryThumbstickRight)) rotateRight();
            } else {
                Vector3 cameraPosition = Camera.main.transform.position;
                currentPreview.transform.LookAt(new Vector3(
                    cameraPosition.x, currentPreview.transform.position.y, cameraPosition.z));
            }

            if (OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) == 0 || Keyboard.current.spaceKey.wasPressedThisFrame)
                triggerPressed = false;

            bool overUI = rayInteractor.State != InteractorState.Normal;
            if (!triggerPressed && !overUI &&
                (Keyboard.current.spaceKey.wasPressedThisFrame || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)))
                placeATarget(currentPreview, prefab);
        }

        if (OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger) > 0.5) SaveObjects();
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) RemoveAllObjects();
    }

    private void updateRemoveHighlight() {
        Ray ray = rayInteractor.Ray;
        GameObject newObj = null;          // мишень/бочка/стена
        GameObject newZone = null;         // зона-линия
        int newEdge = -1;                  // наведённое ребро
        float bestRayDist = float.PositiveInfinity;

        // обычные объекты — по коллайдерам
        if (Physics.Raycast(ray, out RaycastHit hit, 50f)) {
            Transform t = hit.collider.transform;
            while (t != null && newObj == null) {
                foreach (var obj in установленныеМишени) {
                    if (t.gameObject == obj) { newObj = obj; break; }
                }
                t = t.parent;
            }
            if (newObj != null) bestRayDist = hit.distance;
        }

        // рёбра линий — без коллайдеров, расстояние от луча до сегментов (только в режиме Remove)
        foreach (var zone in зоныСтрельбы) {
            var lr = zone.GetComponent<LineRenderer>();
            for (int i = 0; i < lr.positionCount - 1; i++) {
                Vector3 a = zone.transform.TransformPoint(lr.GetPosition(i));
                Vector3 b = zone.transform.TransformPoint(lr.GetPosition(i + 1));
                if (DistanceRayToSegment(ray, a, b, out float rayDist) <= ZonePickThreshold && rayDist < bestRayDist) {
                    bestRayDist = rayDist;
                    newObj = null;          // ребро ближе обычного объекта
                    newZone = zone;
                    newEdge = i;
                }
            }
        }

        if (newObj == hoveredObject && newZone == hoveredZone && newEdge == hoveredEdge) return;

        if (hoveredObject != null) setHighlight(hoveredObject, false);
        hideEdgeHighlight();

        hoveredObject = newObj;
        hoveredZone = newZone;
        hoveredEdge = newEdge;

        if (hoveredObject != null) setHighlight(hoveredObject, true);
        if (hoveredZone != null) showEdgeHighlight(hoveredZone, hoveredEdge);
    }

    private void tryRemoveHovered() {
        if (!OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)) return;

        if (hoveredObject != null) {
            установленныеМишени.Remove(hoveredObject);
            Destroy(hoveredObject);
            hoveredObject = null;
        } else if (hoveredZone != null && hoveredEdge >= 0) {
            removeEdge(hoveredZone, hoveredEdge);
            hoveredZone = null;
            hoveredEdge = -1;
            hideEdgeHighlight();
        }
    }

    private void clearHoverHighlight() {
        if (hoveredObject != null) { setHighlight(hoveredObject, false); hoveredObject = null; }
        if (hoveredZone != null) { hideEdgeHighlight(); hoveredZone = null; hoveredEdge = -1; }
    }

    private void showEdgeHighlight(GameObject zone, int edge) {
        var lr = zone.GetComponent<LineRenderer>();
        Vector3 a = zone.transform.TransformPoint(lr.GetPosition(edge));
        Vector3 b = zone.transform.TransformPoint(lr.GetPosition(edge + 1));

        if (edgeHighlight == null) {
            var go = new GameObject("EdgeRemoveHighlight");
            edgeHighlight = go.AddComponent<LineRenderer>();
            edgeHighlight.material = shootingZoneMaterial;
            edgeHighlight.startWidth = LineWidth * 2.5f;
            edgeHighlight.endWidth = LineWidth * 2.5f;
            edgeHighlight.useWorldSpace = true;
            edgeHighlight.positionCount = 2;
        }
        edgeHighlight.SetPosition(0, a + Vector3.up * 0.004f);   // чуть выше линии, чтобы было видно поверх
        edgeHighlight.SetPosition(1, b + Vector3.up * 0.004f);
        edgeHighlight.enabled = true;
    }

    private void hideEdgeHighlight() {
        if (edgeHighlight != null) edgeHighlight.enabled = false;
    }

    // удаляем одно ребро: полилиния делится на части [p0..p_edge] и [p_edge+1..pN]
    private void removeEdge(GameObject zone, int edge) {
        var lr = zone.GetComponent<LineRenderer>();
        int n = lr.positionCount;
        var pts = new List<Vector3>(n);
        for (int i = 0; i < n; i++) pts.Add(lr.GetPosition(i));   // локальные

        var left = pts.GetRange(0, edge + 1);
        var right = pts.GetRange(edge + 1, n - edge - 1);

        if (left.Count >= 2) {
            lr.positionCount = left.Count;
            for (int i = 0; i < left.Count; i++) lr.SetPosition(i, left[i]);
        } else {
            зоныСтрельбы.Remove(zone);
            Destroy(zone);
        }

        if (right.Count >= 2) {
            // переводим в систему stageRoot через мир (локальный трансформ зоны может быть не единичным после сохранения)
            var rightLocal = new List<Vector3>(right.Count);
            foreach (var p in right)
                rightLocal.Add(stageRoot.InverseTransformPoint(zone.transform.TransformPoint(p)));
            spawnZoneFromLocalPoints(rightLocal);
        }
    }

    private GameObject spawnZoneFromLocalPoints(List<Vector3> localPoints) {
        GameObject zone = new GameObject("ShootingZone");
        zone.transform.SetParent(stageRoot, false);
        zone.AddComponent<ShootingZoneMarker>();

        var lr = zone.AddComponent<LineRenderer>();
        configureZoneLine(lr);
        lr.useWorldSpace = false;
        lr.loop = false;
        lr.positionCount = localPoints.Count;
        for (int i = 0; i < localPoints.Count; i++) lr.SetPosition(i, localPoints[i]);

        зоныСтрельбы.Add(zone);
        return zone;
    }

    // кратчайшее расстояние от луча до отрезка [a,b]; rayDist — дистанция вдоль луча до точки сближения
    private static float DistanceRayToSegment(Ray ray, Vector3 a, Vector3 b, out float rayDist) {
        Vector3 d1 = ray.direction;
        Vector3 d2 = b - a;
        Vector3 r = ray.origin - a;
        float aa = Vector3.Dot(d1, d1);
        float bb = Vector3.Dot(d1, d2);
        float cc = Vector3.Dot(d2, d2);
        float dd = Vector3.Dot(d1, r);
        float ee = Vector3.Dot(d2, r);
        float denom = aa * cc - bb * bb;

        float t = denom < 1e-6f ? 0f : Mathf.Clamp01((aa * ee - bb * dd) / denom);
        float s = Mathf.Max(0f, (bb * t - dd) / aa);

        Vector3 pRay = ray.origin + d1 * s;
        Vector3 pSeg = a + d2 * t;
        rayDist = s;
        return Vector3.Distance(pRay, pSeg);
    }

    public void showSaveDialog() {
        saveStageDialog.SetActive(true);
    } 

    public void SaveObjects() {
        string fileName = nameField.text.Trim();
        if (string.IsNullOrEmpty(fileName)) return;

        string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
        if (File.Exists(path)) {
            Debug.LogWarning($"Файл '{fileName}' уже существует");
            // TODO: показать пользователю предупреждение
            return;
        }

        saveStageDialog.SetActive(false);

        // отвязываем от stageRoot, чтобы их world-позиции не сдвинулись при движении stageRoot
        foreach (var obj in установленныеМишени)
            obj.transform.SetParent(null);
        foreach (var zone in зоныСтрельбы)
            zone.transform.SetParent(null);

        PlaceStageRootInFrontOfPlayer();

        objectDataList.Clear();
        foreach (GameObject obj in установленныеМишени) {
            Vector3 localPos = stageRoot.InverseTransformPoint(obj.transform.position);
            Quaternion localRot = Quaternion.Inverse(stageRoot.rotation) * obj.transform.rotation;
            objectDataList.Add(new ObjectData(obj.name, localPos, localRot));
        }

        // возвращаем обратно под stageRoot
        foreach (var obj in установленныеМишени)
            obj.transform.SetParent(stageRoot);
        foreach (var zone in зоныСтрельбы)
            zone.transform.SetParent(stageRoot);
        ObjectDataList wrapper = new ObjectDataList {
            objectDataList = objectDataList,
            shootingZones = CollectShootingZones()
        };
        File.WriteAllText(path, JsonUtility.ToJson(wrapper));
        PopulateReadyStagesMenu();
        nameField.text = "";
    }

    public void RemoveAllObjects() {
        foreach (GameObject obj in установленныеМишени)
            Destroy(obj);
        установленныеМишени.Clear();

        foreach (GameObject zone in зоныСтрельбы)
            Destroy(zone);
        зоныСтрельбы.Clear();
        if (currentZone != null) { Destroy(currentZone); currentZone = null; zoneLine = null; }
        inZoneMode = false;

        clearHoles();
    }

    public void clearHoles() {
        foreach (GameObject пробоина in пробоины)
            Destroy(пробоина);
        пробоины.Clear();
    }
    
    public void PopulateReadyStagesMenu() {
        // очистить старые кнопки
        foreach (Transform child in readyStagesContainer) {
            Destroy(child.gameObject);
        }

        HashSet<string> userStageNames = new HashSet<string>();

        string[] files = Directory.GetFiles(Application.persistentDataPath, "*.json");
        foreach (string file in files) {
            string stageName = Path.GetFileNameWithoutExtension(file);
            userStageNames.Add(stageName);
            AddStageButton(stageName, isPreset: false);
        }

        TextAsset[] presets = Resources.LoadAll<TextAsset>("PresetStages");
        foreach (TextAsset preset in presets) {
            if (userStageNames.Contains(preset.name)) continue;
            AddStageButton(preset.name, isPreset: true);
        }
    }

    private void AddStageButton(string stageName, bool isPreset) {
        GameObject btn = Instantiate(loadStageButtonPrefub, readyStagesContainer);

        var label = btn.GetComponentInChildren<TMP_Text>();
        if (label != null) label.text = stageName;

        var toggle = btn.GetComponentInChildren<UnityEngine.UI.Toggle>();
        if (toggle != null) {
            toggle.onValueChanged.AddListener(isOn => {
                if (isOn) LoadObjects(stageName);
            });
        }

        var deleteBtn = btn.transform.Find("DeleteButton")?.GetComponent<UnityEngine.UI.Button>();
        if (deleteBtn != null) {
            if (isPreset) {
                deleteBtn.gameObject.SetActive(false);
            } else {
                deleteBtn.onClick.AddListener(() => RequestDeleteStage(stageName));
            }
        }
    }
    
    public void RequestDeleteStage(string fileName) {
        stageToDelete = fileName;
        deleteConfirmDialog.SetActive(true);
    }
    
    public void ConfirmDeleteStage() {
        if (string.IsNullOrEmpty(stageToDelete)) return;
        string path = Path.Combine(Application.persistentDataPath, stageToDelete + ".json");
        if (File.Exists(path)) File.Delete(path);
        stageToDelete = null;
        deleteConfirmDialog.SetActive(false);
        PopulateReadyStagesMenu();
    }

    public void CancelDeleteStage() {
        stageToDelete = null;
        deleteConfirmDialog.SetActive(false);
    }

    public void LoadObjects(string fileName) {
        RemoveAllObjects();

        string json;
        string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
        if (File.Exists(path)) {
            json = File.ReadAllText(path);
        } else {
            TextAsset preset = Resources.Load<TextAsset>("PresetStages/" + fileName);
            if (preset == null) {
                Debug.LogWarning($"Файл '{fileName}' не найден");
                return;
            }
            json = preset.text;
        }

        PlaceStageRootInFrontOfPlayer();

        ObjectDataList wrapper = JsonUtility.FromJson<ObjectDataList>(json);
        if (wrapper.objectDataList != null) objectDataList = wrapper.objectDataList;

        foreach (ObjectData data in objectDataList) {
            string baseName = data.prefabName.EndsWith("(Clone)")
                ? data.prefabName[..^7]
                : data.prefabName;
            if (prefabMap.TryGetValue(baseName, out GameObject prefab)) {
                GameObject obj = Instantiate(prefab, stageRoot);
                obj.transform.localPosition = data.position;
                obj.transform.localRotation = data.rotation;
                установленныеМишени.Add(obj);
            } else {
                Debug.LogWarning("Prefab not found in map: " + data.prefabName);
            }
        }

        if (wrapper.shootingZones != null)
            foreach (ShootingZoneData zone in wrapper.shootingZones)
                SpawnShootingZone(zone);

        competitionModeService.stateEnum = StateEnum.MoveStage;
    }

    private void placeATarget(GameObject preview, GameObject prefab) {
        triggerPressed = true;
        установленныеМишени.Add(Object.Instantiate(prefab, preview.transform.position, preview.transform.rotation, stageRoot));
    }

    private void rotateLeft() {
        currentPreview.transform.Rotate(0f, -5f, 0f, Space.Self);
    }

    private void rotateRight() {
        currentPreview.transform.Rotate(0f, 5f, 0f, Space.Self);
    }

    // авто-повтор удержания стика, как у клавиатуры: первый шаг сразу, потом пауза и частые повторы
    private bool ThumbstickStep(OVRInput.Button button) {
        if (!OVRInput.Get(button, OVRInput.Controller.RTouch)) {
            if (repeatButton == button) repeatButton = OVRInput.Button.None;
            return false;
        }
        if (repeatButton != button) {                 // только начали держать это направление
            repeatButton = button;
            nextRepeatTime = Time.time + RepeatDelay;
            return true;
        }
        if (Time.time >= nextRepeatTime) {
            nextRepeatTime = Time.time + RepeatInterval;
            return true;
        }
        return false;
    }

    private void placeToSurface(GameObject go, RaycastHit hit, float pad = 0.002f) {
        var n = hit.normal.normalized;

        float oldY = go.transform.eulerAngles.y;
        go.transform.rotation = Quaternion.FromToRotation(Vector3.up, n);
        go.transform.rotation = Quaternion.Euler(0f, oldY, 0f);

        Bounds b;
        var cols = go.GetComponentsInChildren<Collider>(true);
        if (cols.Length > 0) {
            Physics.SyncTransforms();
            b = cols[0].bounds; for (int i = 1; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);
        } else {
            var rs = go.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 0) { go.transform.position = hit.point + n * pad; return; }
            b = rs[0].bounds; for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        }

        Vector3 ext = b.extents;
        float rN = Vector3.Dot(ext, new Vector3(Mathf.Abs(n.x), Mathf.Abs(n.y), Mathf.Abs(n.z)));
        float pivotToCenterN = Vector3.Dot(b.center - go.transform.position, n);
        go.transform.position = hit.point + n * (rN - pivotToCenterN + pad);
    }

    private void setHighlight(GameObject obj, bool on) {
        var mpb = new MaterialPropertyBlock();
        if (on) mpb.SetColor("_BaseColor", new Color(1f, 0.3f, 0.3f, 1f));
        foreach (var r in obj.GetComponentsInChildren<Renderer>())
            r.SetPropertyBlock(mpb);
    }
    
    private void PlaceStageRootInFrontOfPlayer() {
        Transform cam = Camera.main.transform;
        Vector3 forward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 xzTarget = cam.position + forward * 2f;

        float floorY = 0f;
        if (Physics.Raycast(new Vector3(xzTarget.x, cam.position.y, xzTarget.z), Vector3.down, out RaycastHit hit, 5f))
            floorY = hit.point.y;

        stageRoot.position = new Vector3(xzTarget.x, floorY, xzTarget.z);
        stageRoot.rotation = Quaternion.LookRotation(-forward);
    }
    
    private void moveStage() {
        if (rayInteractor.State != InteractorState.Normal) return;

        Ray ray = rayInteractor.Ray;
        if (!tryRaycastIgnoringStage(ray, out RaycastHit hit)) return;

        // двигаем так, чтобы геометрический центр стейджа оказался в hit.point
        Vector3 center = GetStageCenter();
        Vector3 xzOffset = new Vector3(center.x - stageRoot.position.x, 0f, center.z - stageRoot.position.z);
        stageRoot.position = new Vector3(hit.point.x - xzOffset.x, hit.point.y, hit.point.z - xzOffset.z);

        // вращение вокруг геометрического центра (он теперь в hit.point)
        if (ThumbstickStep(OVRInput.Button.PrimaryThumbstickLeft))
            stageRoot.RotateAround(hit.point, Vector3.up, -5f);
        if (ThumbstickStep(OVRInput.Button.PrimaryThumbstickRight))
            stageRoot.RotateAround(hit.point, Vector3.up, 5f);

        // подтвердить и выйти из режима
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)) {
            competitionModeService.stateEnum = StateEnum.Idle;
        }
    }

    private bool tryRaycastIgnoringStage(Ray ray, out RaycastHit nearest) {
        nearest = default;
        RaycastHit[] hits = Physics.RaycastAll(ray);
        float bestDist = float.PositiveInfinity;
        bool found = false;
        foreach (var h in hits) {
            string n = h.collider.gameObject.name;
            if (n.Equals("emptyObjectForCollider") || n.Equals("Glock17")) continue;
            if (h.collider.transform.IsChildOf(stageRoot)) continue;
            if (h.distance < bestDist) {
                bestDist = h.distance;
                nearest = h;
                found = true;
            }
        }
        return found;
    }

    private Vector3 GetStageCenter() {
        if (установленныеМишени.Count == 0) return stageRoot.position;

        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;
        foreach (var obj in установленныеМишени) {
            Vector3 p = obj.transform.position;
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }
        return (min + max) / 2f;
    }
    
    public void setMoveStageMode() {
        competitionModeService.stateEnum = StateEnum.MoveStage;
    }

    public void setDrawShootingZoneMode() {
        competitionModeService.stateEnum = StateEnum.DrawShootingZone;
    }

    private void drawShootingZone() {
        if (!inZoneMode) enterZoneMode();
        competitionModeService.hideCommandsText();

        if (rayInteractor.State != InteractorState.Normal) {
            zonePreviewLine.enabled = false;
            return;
        }

        Ray ray = rayInteractor.Ray;
        if (!Physics.Raycast(ray, out RaycastHit hit) || !hit.collider.gameObject.name.Equals("MegaFloor")) {
            zonePreviewLine.enabled = false;
            return;
        }

        Vector3 raw = new Vector3(hit.point.x, hit.point.y + FloorOffset, hit.point.z);
        Vector3 candidate = snapToExisting(raw);   // магнит к ближайшему уже поставленному углу
        bool snapped = candidate != raw;

        if (zonePoints.Count >= 1) {
            zonePreviewLine.enabled = true;
            zonePreviewLine.positionCount = 2;
            zonePreviewLine.SetPosition(0, zonePoints[zonePoints.Count - 1]);
            zonePreviewLine.SetPosition(1, candidate);
        } else {
            zonePreviewLine.enabled = false;
        }

        // кнопка B — закончить текущую линию и начать новую
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) {
            finishLine();
            return;
        }

        if (OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) == 0) zoneTriggerPressed = false;

        if (!zoneTriggerPressed &&
            (Keyboard.current.spaceKey.wasPressedThisFrame || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))) {
            zoneTriggerPressed = true;
            addZonePoint(candidate);

            // клик по примагниченной точке → контур замкнут: фиксируем и выходим в Idle
            if (snapped && zonePoints.Count >= 3) {
                finalizeCurrentZone();
                competitionModeService.stateEnum = StateEnum.Idle;
            }
        }
    }

    private void addZonePoint(Vector3 candidate) {
        zonePoints.Add(candidate);
        zoneLine.positionCount = zonePoints.Count;
        zoneLine.SetPosition(zonePoints.Count - 1, candidate);
    }

    // если курсор рядом с уже поставленной точкой — возвращаем её координаты (магнит)
    private Vector3 snapToExisting(Vector3 candidate) {
        foreach (Vector3 p in zonePoints)
            if (Vector3.Distance(p, candidate) <= SnapRadius) return p;
        return candidate;
    }

    private void enterZoneMode() {
        inZoneMode = true;
        zoneTriggerPressed = false;
        zonePoints.Clear();

        currentZone = new GameObject("ShootingZone");
        currentZone.transform.SetParent(stageRoot, false);
        currentZone.AddComponent<ShootingZoneMarker>();

        zoneLine = currentZone.AddComponent<LineRenderer>();
        configureZoneLine(zoneLine);
        zoneLine.useWorldSpace = true;   // world пока рисуем, в локальные конвертируем при замыкании
        zoneLine.loop = false;
        zoneLine.positionCount = 0;

        GameObject preview = new GameObject("ShootingZonePreview");
        preview.transform.SetParent(currentZone.transform, false);
        zonePreviewLine = preview.AddComponent<LineRenderer>();
        configureZoneLine(zonePreviewLine);
        zonePreviewLine.useWorldSpace = true;
        zonePreviewLine.positionCount = 0;
        zonePreviewLine.enabled = false;

        showZoneHint();
    }

    private void showZoneHint() {
        if (zoneHintMessage == null) return;
        var label = zoneHintMessage.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.text = "Click \"B\" to cancel";
        zoneHintMessage.SetActive(true);
    }

    // закончить текущую линию (B) и сразу начать новую — остаёмся в режиме рисования
    private void finishLine() {
        if (zonePoints.Count >= 2) finalizeCurrentZone();
        else discardCurrentZone();
        enterZoneMode();
    }

    private void finalizeCurrentZone() {
        // переводим world-точки в локальные относительно stageRoot, чтобы линия ехала со стейджем
        zoneLine.useWorldSpace = false;
        zoneLine.positionCount = zonePoints.Count;
        for (int i = 0; i < zonePoints.Count; i++)
            zoneLine.SetPosition(i, stageRoot.InverseTransformPoint(zonePoints[i]));
        zoneLine.loop = false;

        if (zonePreviewLine != null) Destroy(zonePreviewLine.gameObject);
        зоныСтрельбы.Add(currentZone);
        clearCurrentZoneRefs();
    }

    private void discardCurrentZone() {
        if (currentZone != null) Destroy(currentZone);
        clearCurrentZoneRefs();
    }

    private void clearCurrentZoneRefs() {
        currentZone = null;
        zoneLine = null;
        zonePreviewLine = null;
        zonePoints.Clear();
    }

    private void exitZoneMode() {
        inZoneMode = false;
        if (zoneHintMessage != null) zoneHintMessage.SetActive(false);
        if (currentZone == null) return;
        if (zonePoints.Count >= 2) finalizeCurrentZone();  // сохраняем незамкнутую линию
        else discardCurrentZone();                          // одиночная точка — выбрасываем
    }

    private void configureZoneLine(LineRenderer lr) {
        lr.material = shootingZoneMaterial;
        lr.startWidth = LineWidth;
        lr.endWidth = LineWidth;
    }

    private List<ShootingZoneData> CollectShootingZones() {
        var result = new List<ShootingZoneData>();
        foreach (GameObject zone in зоныСтрельбы) {
            var lr = zone.GetComponent<LineRenderer>();
            var pts = new List<Vector3>(lr.positionCount);
            for (int i = 0; i < lr.positionCount; i++) {
                // точки линии локальны относительно зоны; переводим в локальные относительно stageRoot
                Vector3 world = zone.transform.TransformPoint(lr.GetPosition(i));
                pts.Add(stageRoot.InverseTransformPoint(world));
            }
            result.Add(new ShootingZoneData { points = pts, closed = lr.loop });
        }
        return result;
    }

    private void SpawnShootingZone(ShootingZoneData data) {
        spawnZoneFromLocalPoints(data.points);
    }
}