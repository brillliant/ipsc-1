using System;
using Meta.XR.MRUtilityKit;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Main : MonoBehaviour {
    [HideInInspector] public Boolean isHandKeepingMagazine;
    public GameObject effectMeshObject;
    private string targetLayer = "Character";

    private GameObject pistol;
    
    private PistolScript pistolScript;
    private FloorScript floorScript;

    private MeshRenderer pushHandPointOnPistolMesh;
    private GameObject leftHand;
    private MeshRenderer pushMagazinePointOnHandMesh;

    private EffectMesh effectMeshScript;//todo added for demo

    [HideInInspector] public MenuController menuController;
    [HideInInspector] public CompetitionModeService competitionModeService;
    [HideInInspector] public BuilderService builderService;
    
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

        competitionModeService = GetComponent<CompetitionModeService>();
        competitionModeService.init(pistolScript);

        builderService = GetComponent<BuilderService>();
        
        if (OVRManager.display != null && OVRManager.display.displayFrequenciesAvailable != null) {
            OVRManager.display.displayFrequency = 90.0f;
        }
    }

    void Update() {
        if (Keyboard.current.pKey.wasPressedThisFrame || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))
            menuController.showHideMenu(pistol.activeSelf);

        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.LTouch) ||
            Keyboard.current.mKey.wasPressedThisFrame) showHideDebugMesh();

        if (Keyboard.current.oKey.wasPressedThisFrame || OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch)) competitionModeService.startStopRange();
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.LTouch)) competitionModeService.interruptAttempt();

        if (floorScript.IsCalibrating && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            floorScript.ConfirmCalibration();
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