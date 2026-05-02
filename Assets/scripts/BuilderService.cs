using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using Oculus.Interaction;
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

    [SerializeField] private RayInteractor rayInteractor;

    private MenuController menuController;
    private GameModeService gameModeService;
    private readonly List<GameObject> установленныеМишени = new List<GameObject>();
    public readonly List<GameObject> пробоины = new List<GameObject>();

    public GameObject currentPreview;
    private List<ObjectData> objectDataList = new List<ObjectData>();
    private bool triggerPressed;
    private GameObject hoveredObject;

    void Start() {
        menuController = GetComponent<MenuController>();
        gameModeService = GetComponent<GameModeService>();
    }

    void Update() {
        if (menuController.isTargetSetUpMenuActivated) {
            buildWith(ipscTargetPreview, ipscTargetPrefab);
        } else if (menuController.isNoShotSetUpMenuActivated) {
            buildWith(ipscTargetNoShotPreview, ipscTargetNoShotPrefab);
        } else if (menuController.currentIndex == 4) {
            buildWith(barrelPreview, barrelPrefab);
        } else if (menuController.currentIndex == 5) {
            buildWith(wallPreview, wallPrefab);
        } else if (menuController.removeMode) {
            updateRemoveHighlight();
            tryRemoveHovered();
        } else {
            clearHoverHighlight();
        }
    }

    private void buildWith(GameObject preview, GameObject prefab) {
        setUpObject(preview, prefab);
        gameModeService.hideUI();
    }

    public void clearPreview() {
        if (!currentPreview) return;
        currentPreview.SetActive(false);
        Destroy(currentPreview);
        currentPreview = null;
    }

    private void setUpObject(GameObject preview, GameObject prefab) {
        if (!currentPreview) currentPreview = Instantiate(preview);
        if (currentPreview && !currentPreview.activeSelf)
            currentPreview.SetActive(true);

        Ray ray = rayInteractor.Ray;

        if (Physics.Raycast(ray, out RaycastHit hit) && !hit.collider.gameObject.name.Equals("emptyObjectForCollider")
                                                     && !hit.collider.gameObject.name.Equals("Glock17")) {
            placeToSurface(currentPreview, hit);

            if (menuController.currentIndex == 5) {
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
                (Keyboard.current.spaceKey.wasPressedThisFrame || OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) > 0.5))
                placeATarget(currentPreview, prefab);
        }

        if (OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger) > 0.5) SaveObjects();
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) RemoveAllObjects();
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch)) LoadObjects();
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
            Object.Destroy(hoveredObject);
            hoveredObject = null;
        }
    }

    private void clearHoverHighlight() {
        if (hoveredObject == null) return;
        setHighlight(hoveredObject, false);
        hoveredObject = null;
    }

    private void SaveObjects() {
        objectDataList.Clear();
        foreach (GameObject obj in установленныеМишени) {
            objectDataList.Add(new ObjectData(obj.name, obj.transform.position, obj.transform.rotation));
        }
        ObjectDataList wrapper = new ObjectDataList { objectDataList = objectDataList };
        File.WriteAllText(Application.persistentDataPath + "/saveData.json", JsonUtility.ToJson(wrapper));
    }

    private void RemoveAllObjects() {
        foreach (GameObject obj in установленныеМишени)
            Object.Destroy(obj);
        установленныеМишени.Clear();
        clearHoles();
    }

    public void clearHoles() {
        foreach (GameObject пробоина in пробоины)
            Object.Destroy(пробоина);
        пробоины.Clear();
    }

    public void LoadObjects() {
        RemoveAllObjects();

        string path = Application.persistentDataPath + "/saveData.json";
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        ObjectDataList wrapper = JsonUtility.FromJson<ObjectDataList>(json);
        if (wrapper.objectDataList != null) objectDataList = wrapper.objectDataList;

        foreach (ObjectData data in objectDataList) {
            GameObject prefab = Resources.Load<GameObject>(data.prefabName.Substring(0, data.prefabName.Length - 7));
            if (prefab != null) {
                установленныеМишени.Add(Object.Instantiate(prefab, data.position, data.rotation));
            } else {
                Debug.LogWarning("Prefab not found: " + data.prefabName);
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
}