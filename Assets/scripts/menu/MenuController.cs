using UnityEngine;

public class MenuController {
    private readonly GameObject menu;
    private readonly GameObject pistol;
    private readonly GameObject rayLeft;
    private readonly GameObject rayRight;
    
    private readonly float distance = 0.45f;
    private readonly float verticalOffset = -0.2f;
    private readonly float rightOffset = 0.6f;
    private readonly float rotationOffset = -52f;
    
    public MenuController(GameObject menu, GameObject pistol, GameObject rayLeft, GameObject rayRight) {
        this.menu = menu;
        this.pistol = pistol;
        this.rayLeft = rayLeft;
        this.rayRight = rayRight;
    }
    
    public void showHideMenu() {
        bool willBeActive = !menu.activeSelf;
        menu.SetActive(willBeActive);

        rayLeft.SetActive(willBeActive);
        rayRight.SetActive(willBeActive);
        pistol.SetActive(!willBeActive);

        if (willBeActive) {
            positionMenuInFrontOfPlayer();
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
    
    public bool IsActive() => menu.activeSelf;

}
