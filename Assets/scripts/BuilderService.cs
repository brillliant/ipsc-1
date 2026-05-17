using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

public class BuilderService : MonoBehaviour {
    [Header("Prefabs")]
    public GameObject ipscTargetPreview;
    public GameObject ipscTargetPrefab;
    public GameObject ipscTargetNoShotPreview;
    public GameObject ipscTargetNoShotPrefab;
    public GameObject barrelPreview;
    public GameObject barrelPrefab;
    public GameObject wallPreview;
    public GameObject wallPrefab;

    [SerializeField] private Transform readyStagesContainer;
    [SerializeField] private Transform stageRoot;
    public GameObject installStageButtonPrefub;

    [SerializeField] private RayInteractor rayInteractor;

    private CompetitionModeService competitionModeService;
    private readonly List<GameObject> установленныеМишени = new List<GameObject>();
    public readonly List<GameObject> пробоины = new List<GameObject>();

    public GameObject currentPreview;
    private List<ObjectData> objectDataList = new List<ObjectData>();
    private bool triggerPressed;
    private GameObject hoveredObject;
    private Dictionary<string, GameObject> prefabMap;
    public GameObject saveNameDialog;
    
    [SerializeField] private TMP_InputField nameField;

    void Start() {
        competitionModeService = GetComponent<CompetitionModeService>();
        prefabMap = new Dictionary<string, GameObject> {
            { ipscTargetPrefab.name,      ipscTargetPrefab      },
            { ipscTargetNoShotPrefab.name, ipscTargetNoShotPrefab },
            { barrelPrefab.name,           barrelPrefab           },
            { wallPrefab.name,             wallPrefab             },
        };
    }

    void Update() {
        if (competitionModeService.isShootMode()) return;

        if (competitionModeService.stateEnum == StateEnum.IPSC_target) {
            buildWith(ipscTargetPreview, ipscTargetPrefab);
        } else if (competitionModeService.stateEnum == StateEnum.IPSC_noshot) {
            buildWith(ipscTargetNoShotPreview, ipscTargetNoShotPrefab);
        } else if (competitionModeService.stateEnum == StateEnum.Barrel) {
            buildWith(barrelPreview, barrelPrefab);
        } else if (competitionModeService.stateEnum == StateEnum.Wall) {
            buildWith(wallPreview, wallPrefab);
        } else if (competitionModeService.stateEnum == StateEnum.Remove) {
            updateRemoveHighlight();
            tryRemoveHovered();
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
                if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.RTouch)) rotateLeft();
                if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.RTouch)) rotateRight();
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
        GameObject newHovered = null;

        if (Physics.Raycast(ray, out RaycastHit hit, 50f)) {
            Transform t = hit.collider.transform;
            while (t != null && newHovered == null) {
                foreach (var obj in установленныеМишени) {
                    if (t.gameObject == obj) { newHovered = obj; break; }
                }
                t = t.parent;
            }
        }

        if (newHovered == hoveredObject) return;
        if (hoveredObject != null) setHighlight(hoveredObject, false);
        hoveredObject = newHovered;
        if (hoveredObject != null) setHighlight(hoveredObject, true);
    }

    private void tryRemoveHovered() {
        if (hoveredObject != null && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)) {
            установленныеМишени.Remove(hoveredObject);
            Destroy(hoveredObject);
            hoveredObject = null;
        }
    }

    private void clearHoverHighlight() {
        if (hoveredObject == null) return;
        setHighlight(hoveredObject, false);
        hoveredObject = null;
    }

    public void showSaveDialog() {
        saveNameDialog.SetActive(true);
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

        saveNameDialog.SetActive(false);
        PlaceStageRootInFrontOfPlayer();

        objectDataList.Clear();
        foreach (GameObject obj in установленныеМишени) {
            // позиция и поворот ОТНОСИТЕЛЬНО stageRoot
            Vector3 localPos = stageRoot.InverseTransformPoint(obj.transform.position);
            Quaternion localRot = Quaternion.Inverse(stageRoot.rotation) * obj.transform.rotation;
            objectDataList.Add(new ObjectData(obj.name, localPos, localRot));
        }
        ObjectDataList wrapper = new ObjectDataList { objectDataList = objectDataList };
        File.WriteAllText(path, JsonUtility.ToJson(wrapper));
    }

    private void RemoveAllObjects() {
        foreach (GameObject obj in установленныеМишени)
            Destroy(obj);
        установленныеМишени.Clear();
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

        string[] files = Directory.GetFiles(Application.persistentDataPath, "*.json");
        foreach (string file in files) {
            string stageName = Path.GetFileNameWithoutExtension(file);
            GameObject btn = Instantiate(installStageButtonPrefub, readyStagesContainer);

            // подменяем label
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = stageName;

            // вешаем onClick
            var toggle = btn.GetComponentInChildren<UnityEngine.UI.Toggle>();
            if (toggle != null) {
                toggle.onValueChanged.AddListener(isOn => {
                    if (isOn) LoadObjects(stageName);
                });
            }
        }
    }

    public void LoadObjects(string fileName) {
        RemoveAllObjects();

        string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
        if (!File.Exists(path)) {
            Debug.LogWarning($"Файл '{fileName}' не найден");
            return;
        }

        PlaceStageRootInFrontOfPlayer();

        string json = File.ReadAllText(path);
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
    }

    private void placeATarget(GameObject preview, GameObject prefab) {
        triggerPressed = true;
        установленныеМишени.Add(Object.Instantiate(prefab, preview.transform.position, preview.transform.rotation));
    }

    private void rotateLeft() {
        currentPreview.transform.Rotate(0f, -5f, 0f, Space.Self);
    }

    private void rotateRight() {
        currentPreview.transform.Rotate(0f, 5f, 0f, Space.Self);
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
}