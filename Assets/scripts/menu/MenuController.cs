using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuController : MonoBehaviour {
    [Header("UI")]
    public GameObject menu;
    public GameObject camera;

    [Header("Menu Items")]
    public TextMeshProUGUI menuItem1_target;
    public TextMeshProUGUI menuItem2_shoot;
    public TextMeshProUGUI menuItem3_noShot;
    public TextMeshProUGUI menuItem4_dryFire;
    public TextMeshProUGUI menuItem5_barrel;
    public TextMeshProUGUI menuItem6_wall;

    private readonly float distance = 0.45f;
    private readonly float verticalOffset = -0.2f;
    private readonly float rightOffset = 0.6f;
    private readonly float rotationOffset = -52f;

    //[HideInInspector] public bool removeMode = false; //todo заменить на stateEnum
    [HideInInspector] public StateEnum stateEnum;
    
    private Action onClearPreview;
    private List<TextMeshProUGUI> menuList;
    private GameObject pistol;
    private GameObject rayLeft;
    private GameObject rayRight;

    private bool _hoveringUI;
    
    void Start() {
        pistol = GameObject.Find("Glock17");

        menuList = new List<TextMeshProUGUI> {
            menuItem1_target, menuItem2_shoot, menuItem3_noShot,
            menuItem4_dryFire, menuItem5_barrel, menuItem6_wall
        };

        Transform rayRightTransform = camera.transform.Find(
            "[BuildingBlock] Interaction/[BuildingBlock] Controller Interactions/" +
            "RightController/ControllerInteractors/ControllerRayInteractor"
        );

        rayRight = rayRightTransform.gameObject;
    }
    
    private void setRayStatus(bool status) {
        if (status)
            StartCoroutine(enableRay());
        else
            rayRight.SetActive(false);
    }

    private System.Collections.IEnumerator enableRay() {
        rayRight.SetActive(true);
        yield return null;
        foreach (Transform child in rayRight.transform)
            child.gameObject.SetActive(true);
    }

    public void init(Action onClearPreview) {
        this.onClearPreview = onClearPreview;
    }

    public void showHideMenu() {
        bool willBeActive = !menu.activeSelf;
        menu.SetActive(willBeActive);
        setRayStatus(willBeActive);
        pistol.SetActive(!willBeActive);

        if (willBeActive) positionMenuInFrontOfPlayer();
    }

    public void chooseIPSClowTarget() {
        onClearPreview();
        stateEnum = StateEnum.IPSC_target;
    }

    public void chooseIPSCNowshotLowTarget() {
        onClearPreview();
        stateEnum = StateEnum.IPSC_noshot;
    }

    public void chooseBarrel() {
        onClearPreview();
        stateEnum = StateEnum.Barrel;
    }

    public void chooseWall() {
        onClearPreview();
        stateEnum = StateEnum.Wall;
    }

    public void removeModeOn() {
        onClearPreview();
        stateEnum = StateEnum.Remove;
    }

    public bool isShootMode() => stateEnum == StateEnum.Competition || stateEnum == StateEnum.DryRun;

    private void positionMenuInFrontOfPlayer() {
        Transform head = Camera.main.transform;

        Vector3 spawnPos = head.position + head.forward * distance;
        spawnPos.y = head.position.y + verticalOffset;
        spawnPos += head.right * rightOffset;

        Vector3 lookDir = head.position - spawnPos;
        lookDir.y = 0;
        Quaternion spawnRot = Quaternion.LookRotation(-lookDir.normalized);
        spawnRot *= Quaternion.Euler(0, rotationOffset, 0);

        menu.transform.position = spawnPos;
        menu.transform.rotation = spawnRot;
    }
}