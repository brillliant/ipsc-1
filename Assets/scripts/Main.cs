using System;
using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using Meta.XR.MRUtilityKit;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Main : MonoBehaviour {
    public GameObject camera;
    public GameObject menu;
    private GameObject controllerRayInteractorLeft;
    private GameObject controllerRayInteractorRight;

    public GameObject ipscTargetPreview;
    public GameObject ipscTargetPrefab;

    public GameObject ipscTargetNoShotPreview;
    public GameObject ipscTargetNoShotPrefab;

    public GameObject barrelPreview;
    public GameObject barrelPrefub;

    public GameObject wallPreview;
    public GameObject wallPrefub;

    public Boolean isHandKeepingMagazine = false;

    public TextMeshProUGUI menuItem1_target;
    public TextMeshProUGUI menuItem2_shoot;
    public TextMeshProUGUI menuItem3_noShot;
    public TextMeshProUGUI menuItem4_dryFire;
    public TextMeshProUGUI menuItem5_barrel;
    public TextMeshProUGUI menuItem6_wall;

    public List<ObjectData> objectDataList;
    public List<GameObject> установленныеМишени;
    public List<GameObject> пробоины;

    public Boolean triggerPressed = false;
    public GameObject effectMeshObject;

    private string targetLayer = "Character";

    private Transform bulletPoint;

    private TextMeshProUGUI readyText;
    private TextMeshProUGUI hintText;
    public AudioSource loadAndMakeReadySound;
    public AudioSource areYouReadySound;
    public AudioSource standByySound;
    public AudioSource beepSound;
    public AudioSource ifYouAreFinishedUnloadAndShowClear;
    public AudioSource ifClearHammerDownAndHolster;
    public AudioSource rangeIsClear;

    private GameObject pistol;
    private PistolScript pistolScript;

    private MeshRenderer pushHandPointOnPistolMesh;

    private GameObject leftHand;
    private MeshRenderer pushMagazinePointOnHandMesh;
    private FloorScript floorScript;
    private EffectMesh effectMeshScript;//todo added for demo

    public MenuController menuController;
    public GameModeService gameModeService;

    private GameObject hoveredObject;

    void Start() {
        Transform root = camera.transform;

        controllerRayInteractorLeft = root.Find(
            "[BuildingBlock] Interaction/[BuildingBlock] Controller Interactions/LeftController/ControllerInteractors/ControllerRayInteractor"
        )?.gameObject;

        controllerRayInteractorRight = root.Find(
            "[BuildingBlock] Interaction/[BuildingBlock] Controller Interactions/RightController/ControllerInteractors/ControllerRayInteractor"
        )?.gameObject;

        InvokeRepeating(nameof(setHandColliderLayer), 1f, 1f);
        pistol = GameObject.Find("Glock17");
        pistolScript = pistol.GetComponent<PistolScript>();
        floorScript = GetComponent<FloorScript>();

        effectMeshScript = effectMeshObject.GetComponent<EffectMesh>();//todo added for demo
        bulletPoint = pistolScript.bulletPoint;

        readyText = GameObject.Find("ready").GetComponent<TextMeshProUGUI>();
        hintText = GameObject.Find("hint").GetComponent<TextMeshProUGUI>();

        objectDataList = new List<ObjectData>();
        установленныеМишени = new List<GameObject>();

        pushHandPointOnPistolMesh = pistol.transform.Find("pushHandPoint/Sphere").gameObject.GetComponent<MeshRenderer>();
        pushMagazinePointOnHandMesh = GameObject.Find("pushMagazinePointOnHand").gameObject.GetComponent<MeshRenderer>();
        leftHand = GameObject.Find("OpenXRLeftHand").transform.Find("LeftHand").gameObject;

        menuController = new MenuController(menu, pistol, controllerRayInteractorLeft, controllerRayInteractorRight,
            bulletPoint, pistolScript,
            menuItem1_target, menuItem2_shoot, menuItem3_noShot,
            menuItem4_dryFire, menuItem5_barrel, menuItem6_wall);

        gameModeService = new GameModeService(this, readyText, hintText, pistolScript,
            loadAndMakeReadySound, areYouReadySound, standByySound,
            beepSound, ifYouAreFinishedUnloadAndShowClear, ifClearHammerDownAndHolster, rangeIsClear);
    }

    void Update() {
        if (Keyboard.current.pKey.wasPressedThisFrame || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))
            menuController.showHideMenu();

        if (menuController.isTargetSetUpMenuActivated) {
            menuController.paintRay();
            setUpObject(ipscTargetPreview, ipscTargetPrefab);
            readyText.gameObject.SetActive(false);
            hintText.gameObject.SetActive(false);
        } else if (menuController.isNoShotSetUpMenuActivated) {
            menuController.paintRay();
            setUpObject(ipscTargetNoShotPreview, ipscTargetNoShotPrefab);
            readyText.gameObject.SetActive(false);
            hintText.gameObject.SetActive(false);
        } else if (menuController.currentIndex == 4) {
            menuController.paintRay();
            setUpObject(barrelPreview, barrelPrefub);
            readyText.gameObject.SetActive(false);
            hintText.gameObject.SetActive(false);
        } else if (menuController.currentIndex == 5) {
            menuController.paintRay();
            setUpObject(wallPreview, wallPrefub);
            readyText.gameObject.SetActive(false);
            hintText.gameObject.SetActive(false);
        } else if (menuController.removeMode) {
            menuController.paintRay();
            updateRemoveHighlight();
            if (hoveredObject != null && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)) {
                установленныеМишени.Remove(hoveredObject);
                Destroy(hoveredObject);
                hoveredObject = null;
            }
        } else {
            clearHoverHighlight();
            menuController.hideRay();
        }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.LTouch)) menuController.changeMenu();
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.LTouch) ||
            Keyboard.current.mKey.wasPressedThisFrame) showHideDebugMesh();

        if (!(menuController.isTargetSetUpMenuActivated && menuController.isNoShotSetUpMenuActivated)
            && !gameModeService.stageStarted
            && !gameModeService.inprocessCommand
            && (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch)
                || Keyboard.current.zKey.wasPressedThisFrame)) {
            gameModeService.startStage();
        }

        if (!(menuController.isTargetSetUpMenuActivated && menuController.isNoShotSetUpMenuActivated)
            && gameModeService.stageStarted
            && !gameModeService.inprocessCommand
            && (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch)
                || Keyboard.current.zKey.wasPressedThisFrame)) {
            gameModeService.stopStage();
        }

        if (!(menuController.isTargetSetUpMenuActivated && menuController.isNoShotSetUpMenuActivated)
            && (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch)
                || Keyboard.current.leftShiftKey.wasPressedThisFrame)) {
            gameModeService.interruptAttempt();
        }
    }

    void setHandColliderLayer() {
        GameObject capsules = GameObject.Find("Capsules");
        if (capsules == null) return;

        int layer = LayerMask.NameToLayer(targetLayer);
        SetLayerRecursive(capsules, layer);
        CancelInvoke(nameof(setHandColliderLayer));
    }

    void SetLayerRecursive(GameObject obj, int layer) {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    public void sayHolsterCommand() => gameModeService.sayHolsterCommand();

    public void chooseIPSClowTarget() => menuController.chooseIPSClowTarget();
    public void chooseIPSCNowshotLowTarget() => menuController.chooseIPSCNowshotLowTarget();
    public void chooseBarrel() => menuController.chooseBarrel();
    public void chooseWall() => menuController.chooseWall();
    public void removeModeOn() => menuController.removeModeOn();
    public int getCurrentIndex() => menuController.getCurrentIndex();
    public bool isShootMode() => menuController.isShootMode();

    private void showHideDebugMesh() {
        pushHandPointOnPistolMesh.enabled = !pushHandPointOnPistolMesh.enabled;
        leftHand.SetActive(!leftHand.activeSelf);
        pushMagazinePointOnHandMesh.enabled = !pushMagazinePointOnHandMesh.enabled;
        floorScript.highlight = !floorScript.highlight;

        if (effectMeshScript.HideMesh) effectMeshScript.HideMesh = false;
        else effectMeshScript.HideMesh = true;
    }

    private void rotateLeft() {
        menuController.currentPreview.transform.Rotate(0f, -5f, 0f, Space.Self);
    }

    private void rotateRight() {
        menuController.currentPreview.transform.Rotate(0f, 5f, 0f, Space.Self);
    }

    protected void setUpObject(GameObject preview, GameObject prefab) {
        if (!menuController.currentPreview) menuController.currentPreview = Instantiate(preview);
        if (menuController.currentPreview && !menuController.currentPreview.activeSelf)
            menuController.currentPreview.SetActive(true);

        Ray ray = new Ray(bulletPoint.position, bulletPoint.forward);

        if (Physics.Raycast(ray, out RaycastHit hit) && !hit.collider.gameObject.name.Equals("emptyObjectForCollider")
                                                     && !hit.collider.gameObject.name.Equals("Glock17")) {
            placeToSurface(menuController.currentPreview, hit);

            if (menuController.currentIndex == 5) {
                if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.RTouch)) rotateLeft();
                if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.RTouch)) rotateRight();
            } else {
                Vector3 cameraPosition = Camera.main.transform.position;
                menuController.currentPreview.transform.LookAt(new Vector3(cameraPosition.x, menuController.currentPreview.transform.position.y, cameraPosition.z));
            }

            if (OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) == 0 || Keyboard.current.spaceKey.wasPressedThisFrame)
                triggerPressed = false;

            if (!triggerPressed &&
                (Keyboard.current.spaceKey.wasPressedThisFrame || OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) > 0.5))
                placeATarget(menuController.currentPreview, prefab);
        }

        if (OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger) > 0.5) SaveObjects();
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) RemoveAllObjects();
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch)) LoadObjects();
    }

    private void updateRemoveHighlight() {
        Ray ray = new Ray(bulletPoint.position, bulletPoint.forward);
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

    private void clearHoverHighlight() {
        if (hoveredObject == null) return;
        setHighlight(hoveredObject, false);
        hoveredObject = null;
    }

    private void setHighlight(GameObject obj, bool on) {
        var mpb = new MaterialPropertyBlock();
        if (on) mpb.SetColor("_BaseColor", new Color(1f, 0.3f, 0.3f, 1f));
        foreach (var r in obj.GetComponentsInChildren<Renderer>())
            r.SetPropertyBlock(mpb);
    }

    private void placeATarget(GameObject preview, GameObject prefab) {
        triggerPressed = true;
        установленныеМишени.Add(Instantiate(prefab, preview.transform.position, preview.transform.rotation));
    }

    public void SaveObjects() {
        objectDataList.Clear();
        foreach (GameObject obj in установленныеМишени) {
            ObjectData data = new ObjectData(obj.name, obj.transform.position, obj.transform.rotation);
            objectDataList.Add(data);
        }
        ObjectDataList wrapper = new ObjectDataList { objectDataList = objectDataList };
        string json = JsonUtility.ToJson(wrapper);
        File.WriteAllText(Application.persistentDataPath + "/saveData.json", json);
    }

    public void RemoveAllObjects() {
        foreach (GameObject obj in установленныеМишени) {
            if (obj != gameObject) Destroy(obj);
        }
        установленныеМишени.Clear();
        clearHoles();
    }

    public void clearHoles() {
        foreach (GameObject пробоина in пробоины) {
            if (пробоина != gameObject) Destroy(пробоина);
        }
        пробоины.Clear();
    }

    public void LoadObjects() {
        RemoveAllObjects();

        string path = Application.persistentDataPath + "/saveData.json";
        if (File.Exists(path)) {
            string json = File.ReadAllText(path);
            ObjectDataList wrapper = JsonUtility.FromJson<ObjectDataList>(json);
            if (wrapper.objectDataList != null) objectDataList = wrapper.objectDataList;

            foreach (ObjectData data in objectDataList) {
                GameObject prefab = Resources.Load<GameObject>(data.prefabName.Substring(0, data.prefabName.Length - 7));
                if (prefab != null) {
                    установленныеМишени.Add(Instantiate(prefab, data.position, data.rotation));
                } else {
                    Debug.LogWarning("Prefab not found: " + data.prefabName);
                }
            }
        }
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
}
