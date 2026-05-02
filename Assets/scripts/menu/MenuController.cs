using System;
using System.Collections.Generic;
using System.Reflection;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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

    [HideInInspector] public int currentIndex = 0;
    [HideInInspector] public bool isTargetSetUpMenuActivated = true;
    [HideInInspector] public bool isNoShotSetUpMenuActivated = false;
    [HideInInspector] public bool removeMode = false;

    private Action onClearPreview;
    private List<TextMeshProUGUI> menuList;
    private GameObject pistol;
    private PistolScript pistolScript;
    private GameObject rayLeft;
    private GameObject rayRight;

    private bool  _hoveringUI;
    
    void Start() {
        pistol = GameObject.Find("Glock17");
        pistolScript = pistol.GetComponent<PistolScript>();

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

    public void changeMenu() {
        removeMode = false;
        onClearPreview();

        int index = getNextIndex();
        highlightNecessaryMenuItem(index);

        if (currentIndex == 0) {
            isTargetSetUpMenuActivated = true;
            isNoShotSetUpMenuActivated = false;
        } else if (currentIndex == 2) {
            isTargetSetUpMenuActivated = false;
            isNoShotSetUpMenuActivated = true;
        } else {
            isTargetSetUpMenuActivated = false;
            isNoShotSetUpMenuActivated = false;
        }

        if (currentIndex == 1) pistolScript.setMagRoundCount(15);
        else if (currentIndex == 3) pistolScript.setMagRoundCount(int.MaxValue);
    }

    public void chooseIPSClowTarget() {
        onClearPreview();
        currentIndex = 0;
        isTargetSetUpMenuActivated = true;
        isNoShotSetUpMenuActivated = false;
        removeMode = false;
        highlightNecessaryMenuItem(currentIndex);
    }

    public void chooseIPSCNowshotLowTarget() {
        onClearPreview();
        currentIndex = 2;
        isTargetSetUpMenuActivated = false;
        isNoShotSetUpMenuActivated = true;
        removeMode = false;
        highlightNecessaryMenuItem(currentIndex);
    }

    public void chooseBarrel() {
        onClearPreview();
        currentIndex = 4;
        isTargetSetUpMenuActivated = false;
        isNoShotSetUpMenuActivated = false;
        removeMode = false;
        highlightNecessaryMenuItem(currentIndex);
    }

    public void chooseWall() {
        onClearPreview();
        currentIndex = 5;
        isTargetSetUpMenuActivated = false;
        isNoShotSetUpMenuActivated = false;
        removeMode = false;
        highlightNecessaryMenuItem(currentIndex);
    }

    public void removeModeOn() {
        onClearPreview();
        removeMode = true;
        currentIndex = -1;
        isTargetSetUpMenuActivated = false;
        isNoShotSetUpMenuActivated = false;
        highlightNecessaryMenuItem(currentIndex);
    }

    public int getCurrentIndex() => currentIndex;
    public bool isShootMode() => currentIndex is 1 or 3;

    private int getNextIndex() {
        if (currentIndex + 1 <= menuList.Count - 1) currentIndex++;
        else currentIndex = 0;
        return currentIndex;
    }

    private void highlightNecessaryMenuItem(int index) {
        for (int i = 0; i < menuList.Count; i++) {
            if (i == index) {
                menuList[i].fontSize = 8;
                menuList[i].fontStyle = FontStyles.Bold;
                menuList[i].color = Color.red;
            } else {
                menuList[i].fontSize = 5;
                menuList[i].fontStyle = FontStyles.Normal;
                menuList[i].color = Color.gray;
            }
        }
    }

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