using System;
using System.Collections.Generic;
using Oculus.Interaction;
using TMPro;
using UnityEngine;

public class MenuController : MonoBehaviour {
    [Header("UI")]
    public GameObject menu;
    public GameObject camera;

    /*[Header("Menu Items")]
    public TextMeshProUGUI menuItem1_target;
    public TextMeshProUGUI menuItem2_shoot;
    public TextMeshProUGUI menuItem3_noShot;
    public TextMeshProUGUI menuItem4_dryFire;
    public TextMeshProUGUI menuItem5_barrel;
    public TextMeshProUGUI menuItem6_wall;*/

    private FloorScript floorScript;

    private CompetitionModeService competitionModeService;
    private BodyLockedFollow bodyLockedFollow;
    
    private Action onClearPreview;
    //private List<TextMeshProUGUI> menuList;
    private GameObject pistol;
    private GameObject rayLeft;
    private GameObject rayRight;

    private bool _hoveringUI;
    
    void Start() {
        pistol = GameObject.Find("Glock17");

        /*menuList = new List<TextMeshProUGUI> {
            menuItem1_target, menuItem2_shoot, menuItem3_noShot,
            menuItem4_dryFire, menuItem5_barrel, menuItem6_wall
        };*/

        Transform rayRightTransform = camera.transform.Find(
            "[BuildingBlock] Interaction/[BuildingBlock] Controller Interactions/" +
            "RightController/ControllerInteractors/ControllerRayInteractor"
        );

        rayRight = rayRightTransform.gameObject;
        competitionModeService = GetComponent<CompetitionModeService>();
        bodyLockedFollow = menu.GetComponent<BodyLockedFollow>();
        floorScript = GetComponent<FloorScript>();
        
        menu.SetActive(false);
        setRayStatus(false);
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

    // правый луч контроллера — тот же, которым тыкаем в кнопки меню
    public RayInteractor GetRightRayInteractor() {
        return rayRight != null ? rayRight.GetComponent<RayInteractor>() : null;
    }

    public void hideMenu() {
        onClearPreview();
        // сброс в Idle — только для билдер-режимов; идущий стейдж закрытием меню не прерываем
        if (competitionModeService.stateEnum != StateEnum.StageRun)
            competitionModeService.stateEnum = StateEnum.Idle;
        showHideMenu(false);
    }
    
    public void showMenu() {
        showHideMenu(true);
    }
    
    public void showHideMenu(bool willBeActive) {
        menu.SetActive(willBeActive);
        setRayStatus(willBeActive);
        pistol.SetActive(!willBeActive);

        if (willBeActive) {
            bodyLockedFollow.SnapToSpawnPosition();   // поставить перед игроком один раз при открытии
            foreach (var hs in FindObjectsOfType<HandednessService>())
                hs.Recalibrate();                     // перекалибровать эталон трекинга при открытии меню
        }
        
        if (!willBeActive) floorScript.highlight = false;
    }

    public void chooseIPSClowTarget() {
        onClearPreview();
        competitionModeService.stateEnum = StateEnum.IPSC_target;
    }

    public void chooseIPSCNowshotLowTarget() {
        onClearPreview();
        competitionModeService.stateEnum = StateEnum.IPSC_noshot;
    }

    public void chooseUspsaTarget() {
        onClearPreview();
        competitionModeService.stateEnum = StateEnum.USPSA_target;
    }

    public void chooseUspsaNoShotTarget() {
        onClearPreview();
        competitionModeService.stateEnum = StateEnum.USPSA_noshot;
    }

    public void chooseUspsaRightDarkTarget() {
        onClearPreview();
        competitionModeService.stateEnum = StateEnum.USPSA_rightDark;
    }

    public void chooseUspsaLeftDarkTarget() {
        onClearPreview();
        competitionModeService.stateEnum = StateEnum.USPSA_leftDark;
    }

    public void chooseUspsaLeftRightDarkTarget() {
        onClearPreview();
        competitionModeService.stateEnum = StateEnum.USPSA_leftRightDark;
    }

    public void chooseBarrel() {
        onClearPreview();
        competitionModeService.stateEnum = StateEnum.Barrel;
    }

    public void chooseUspsaMiniPopper() {
        onClearPreview();
        competitionModeService.stateEnum = StateEnum.USPSAMiniPopper;
    }

    public void chooseUspsaPopper() {
        onClearPreview();
        competitionModeService.stateEnum = StateEnum.USPSAPopper;
    }

    public void chooseWall() {
        onClearPreview();
        competitionModeService.stateEnum = StateEnum.Wall;
    }

    public void removeModeOn() {
        onClearPreview();
        competitionModeService.stateEnum = StateEnum.Remove;
    }
}