using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class MenuController {
    private readonly GameObject menu;
    private readonly GameObject pistol;
    private readonly GameObject rayLeft;
    private readonly GameObject rayRight;
    private readonly Transform bulletPoint;
    private readonly PistolScript pistolScript;

    private readonly float distance = 0.45f;
    private readonly float verticalOffset = -0.2f;
    private readonly float rightOffset = 0.6f;
    private readonly float rotationOffset = -52f;

    public int currentIndex = 0;
    public bool isTargetSetUpMenuActivated = true;
    public bool isNoShotSetUpMenuActivated = false;
    public bool removeMode = false;
    public GameObject currentPreview;

    private List<TextMeshProUGUI> menuList;
    private LineRenderer line;

    public MenuController(GameObject menu, GameObject pistol, GameObject rayLeft, GameObject rayRight,
        Transform bulletPoint, PistolScript pistolScript,
        TextMeshProUGUI menuItem1, TextMeshProUGUI menuItem2, TextMeshProUGUI menuItem3,
        TextMeshProUGUI menuItem4, TextMeshProUGUI menuItem5, TextMeshProUGUI menuItem6) {
        this.menu = menu;
        this.pistol = pistol;
        this.rayLeft = rayLeft;
        this.rayRight = rayRight;
        this.bulletPoint = bulletPoint;
        this.pistolScript = pistolScript;

        menuList = new List<TextMeshProUGUI> {
            menuItem1, menuItem2, menuItem3, menuItem4, menuItem5, menuItem6
        };
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
        clearPreview();

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
        clearPreview();
        currentIndex = 0;
        isTargetSetUpMenuActivated = true;
        isNoShotSetUpMenuActivated = false;
        removeMode = false;
        highlightNecessaryMenuItem(currentIndex);
        showHideMenu();
    }

    public void chooseIPSCNowshotLowTarget() {
        clearPreview();
        currentIndex = 2;
        isTargetSetUpMenuActivated = false;
        isNoShotSetUpMenuActivated = true;
        removeMode = false;
        highlightNecessaryMenuItem(currentIndex);
        showHideMenu();
    }

    public void chooseBarrel() {
        clearPreview();
        currentIndex = 4;
        isTargetSetUpMenuActivated = false;
        isNoShotSetUpMenuActivated = false;
        removeMode = false;
        highlightNecessaryMenuItem(currentIndex);
        showHideMenu();
    }

    public void chooseWall() {
        clearPreview();
        currentIndex = 5;
        isTargetSetUpMenuActivated = false;
        isNoShotSetUpMenuActivated = false;
        removeMode = false;
        highlightNecessaryMenuItem(currentIndex);
        showHideMenu();
    }

    public void removeModeOn() {
        clearPreview();
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

    public bool IsActive() => menu.activeSelf;

    private void clearPreview() {
        if (!currentPreview) return;
        currentPreview.SetActive(false);
        Object.Destroy(currentPreview);
        currentPreview = null;
    }

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
