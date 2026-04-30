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

    [HideInInspector] public int currentIndex = 0;
    [HideInInspector] public bool isTargetSetUpMenuActivated = true;
    [HideInInspector] public bool isNoShotSetUpMenuActivated = false;
    [HideInInspector] public bool removeMode = false;

    private Action onClearPreview;
    private List<TextMeshProUGUI> menuList;
    private LineRenderer line;
    private GameObject pistol;
    private PistolScript pistolScript;
    private Transform bulletPoint;
    private GameObject rayLeft;
    private GameObject rayRight;

    void Start() {
        pistol = GameObject.Find("Glock17");
        pistolScript = pistol.GetComponent<PistolScript>();
        bulletPoint = pistolScript.bulletPoint;

        Transform root = camera.transform;
        rayLeft = root.Find(
            "[BuildingBlock] Interaction/[BuildingBlock] Controller Interactions/LeftController/ControllerInteractors/ControllerRayInteractor"
        )?.gameObject;
        rayRight = root.Find(
            "[BuildingBlock] Interaction/[BuildingBlock] Controller Interactions/RightController/ControllerInteractors/ControllerRayInteractor"
        )?.gameObject;

        menuList = new List<TextMeshProUGUI> {
            menuItem1_target, menuItem2_shoot, menuItem3_noShot,
            menuItem4_dryFire, menuItem5_barrel, menuItem6_wall
        };
    }

    public void init(Action onClearPreview) {
        this.onClearPreview = onClearPreview;
    }

    public void showHideMenu() {
        bool willBeActive = !menu.activeSelf;
        menu.SetActive(willBeActive);
        rayLeft.SetActive(willBeActive);
        rayRight.SetActive(willBeActive);
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
        showHideMenu();
    }

    public void chooseIPSCNowshotLowTarget() {
        onClearPreview();
        currentIndex = 2;
        isTargetSetUpMenuActivated = false;
        isNoShotSetUpMenuActivated = true;
        removeMode = false;
        highlightNecessaryMenuItem(currentIndex);
        showHideMenu();
    }

    public void chooseBarrel() {
        onClearPreview();
        currentIndex = 4;
        isTargetSetUpMenuActivated = false;
        isNoShotSetUpMenuActivated = false;
        removeMode = false;
        highlightNecessaryMenuItem(currentIndex);
        showHideMenu();
    }

    public void chooseWall() {
        onClearPreview();
        currentIndex = 5;
        isTargetSetUpMenuActivated = false;
        isNoShotSetUpMenuActivated = false;
        removeMode = false;
        highlightNecessaryMenuItem(currentIndex);
        showHideMenu();
    }

    public void removeModeOn() {
        onClearPreview();
        removeMode = true;
        paintRay();
        line.material.color = Color.red;
        line.startWidth = 0.015f;
        line.endWidth = 0.003f;
        currentIndex = -1;
        isTargetSetUpMenuActivated = false;
        isNoShotSetUpMenuActivated = false;
        highlightNecessaryMenuItem(currentIndex);
        showHideMenu();
    }

    public void paintRay() {
        if (!line) {
            var rayGameObject = new GameObject("Ray");
            rayGameObject.transform.SetParent(bulletPoint, false);
            line = rayGameObject.AddComponent<LineRenderer>();
            line.startWidth = 0.005f;
            line.endWidth = 0.001f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.material.color = Color.cyan;
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.alignment = LineAlignment.View;
        }

        if (!line.enabled) {
            line.enabled = true;
            line.material.color = Color.cyan;
            line.startWidth = 0.005f;
            line.endWidth = 0.001f;
        }

        line.SetPosition(0, bulletPoint.position);
        line.SetPosition(1, bulletPoint.position + bulletPoint.forward * 10f);
    }

    public void hideRay() {
        if (!line) return;
        line.enabled = false;
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