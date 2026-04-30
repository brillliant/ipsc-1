using System;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.InputSystem;

public class Main : MonoBehaviour {
    [HideInInspector] public Boolean isHandKeepingMagazine;
    public GameObject effectMeshObject;
    private string targetLayer = "Character";

    private GameObject pistol;
    private PistolScript pistolScript;

    private MeshRenderer pushHandPointOnPistolMesh;
    private GameObject leftHand;
    private MeshRenderer pushMagazinePointOnHandMesh;
    private FloorScript floorScript;
    private EffectMesh effectMeshScript;//todo added for demo

    [HideInInspector] public MenuController menuController;
    [HideInInspector] public GameModeService gameModeService;
    public BuilderService builderService;

    void Start() {
        InvokeRepeating(nameof(setHandColliderLayer), 1f, 1f);
        pistol = GameObject.Find("Glock17");
        pistolScript = pistol.GetComponent<PistolScript>();
        floorScript = GetComponent<FloorScript>();

        effectMeshScript = effectMeshObject.GetComponent<EffectMesh>();//todo added for demo

        pushHandPointOnPistolMesh = pistol.transform.Find("pushHandPoint/Sphere").gameObject.GetComponent<MeshRenderer>();
        pushMagazinePointOnHandMesh = GameObject.Find("pushMagazinePointOnHand").gameObject.GetComponent<MeshRenderer>();
        leftHand = GameObject.Find("OpenXRLeftHand").transform.Find("LeftHand").gameObject;

        menuController = GetComponent<MenuController>();
        menuController.init(() => builderService.clearPreview());

        gameModeService = GetComponent<GameModeService>();
        gameModeService.init(pistolScript);

        builderService = GetComponent<BuilderService>();
    }

    void Update() {
        if (Keyboard.current.pKey.wasPressedThisFrame || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))
            menuController.showHideMenu();

        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.LTouch)) menuController.changeMenu();
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.LTouch) ||
            Keyboard.current.mKey.wasPressedThisFrame) showHideDebugMesh();

        handleStageInput();
    }

    private void handleStageInput() {
        if (menuController.isTargetSetUpMenuActivated && menuController.isNoShotSetUpMenuActivated) return;

        bool triggerDown = OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch)
                           || Keyboard.current.zKey.wasPressedThisFrame;

        if (!gameModeService.stageStarted && !gameModeService.inprocessCommand && triggerDown) gameModeService.startStage();
        if (gameModeService.stageStarted && !gameModeService.inprocessCommand && triggerDown) gameModeService.stopStage();
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch)
            || Keyboard.current.leftShiftKey.wasPressedThisFrame)
            gameModeService.interruptAttempt();
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

    private void showHideDebugMesh() {
        pushHandPointOnPistolMesh.enabled = !pushHandPointOnPistolMesh.enabled;
        leftHand.SetActive(!leftHand.activeSelf);
        pushMagazinePointOnHandMesh.enabled = !pushMagazinePointOnHandMesh.enabled;
        floorScript.highlight = !floorScript.highlight;

        if (effectMeshScript.HideMesh) effectMeshScript.HideMesh = false;
        else effectMeshScript.HideMesh = true;
    }
}