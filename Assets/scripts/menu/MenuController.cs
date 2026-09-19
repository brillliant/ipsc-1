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
        
        // Allow Switch Off у группы тайлов включён (иначе Unity сама нажимает первый тайл при старте),
        // поэтому отжатие активного тайла кликом должно выключать режим — обработчики choose* об этом не знают
        foreach (var toggle in objectTiles()) {
            var group = toggle.group;
            if (group == null) continue;
            toggle.onValueChanged.AddListener(on => {
                if (on || group.AnyTogglesOn() || competitionModeService.stateEnum == StateEnum.StageRun) return;
                onClearPreview();
                competitionModeService.stateEnum = StateEnum.Idle;
            });
        }

        menu.SetActive(false);
        setRayStatus(false);
    }

    // тайлы секции Objects — дети одного Content (родителя тайла Inspect)
    private List<UnityEngine.UI.Toggle> objectTiles() {
        var result = new List<UnityEngine.UI.Toggle>();
        Transform content = null;
        foreach (var t in menu.GetComponentsInChildren<UnityEngine.UI.Toggle>(true))
            if (t.gameObject.name == "Inspect") { content = t.transform.parent; break; }
        if (content == null) return result;
        foreach (Transform child in content) {
            var toggle = child.GetComponent<UnityEngine.UI.Toggle>();
            if (toggle != null) result.Add(toggle);
        }
        return result;
    }

    // Группа тайлов и подсветка нажатого тайла настроены в сцене (overrides: Toggle.m_Group и
    // AnimatorOverrideLayerWeigth._toggle у каждого тайла-режима, Allow Switch Off у группы выключен).
    // Код нужен только чтобы отжать тайл, когда режим выключается не кликом по тайлу.
    private static void setPressedVisual(Oculus.Interaction.Samples.AnimatorOverrideLayerWeigth layer, bool on) {
        if (layer != null && layer.isActiveAndEnabled) layer.SetOverrideLayerActive(on);
    }

    // отжать все тайлы из кода (закрытие меню): режим при этом уже сброшен снаружи
    private void releaseTiles() {
        foreach (var toggle in objectTiles()) {
            if (!toggle.isOn) continue;
            toggle.SetIsOnWithoutNotify(false);
            setPressedVisual(toggle.GetComponent<Oculus.Interaction.Samples.AnimatorOverrideLayerWeigth>(), false);
        }
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
        releaseTiles();   // пока меню ещё активно — аниматоры тайлов живы
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

    public void chooseWall4() {
        onClearPreview();
        competitionModeService.stateEnum = StateEnum.Wall4;
    }

    // «лупа»: тайл-переключатель, onValueChanged дёргает и при включении, и при выключении — поэтому toggle по состоянию
    public void chooseInspect() {
        onClearPreview();
        competitionModeService.stateEnum = competitionModeService.stateEnum == StateEnum.Inspect
            ? StateEnum.Idle : StateEnum.Inspect;
    }

    // выход из режима кнопкой контроллера: гасим его тайл в меню, чтобы картинка совпадала с состоянием
    public void setTileToggle(string tileName, bool on) {
        foreach (var toggle in menu.GetComponentsInChildren<UnityEngine.UI.Toggle>(true)) {
            if (toggle.gameObject.name != tileName) continue;
            toggle.SetIsOnWithoutNotify(on);
            setPressedVisual(toggle.GetComponent<Oculus.Interaction.Samples.AnimatorOverrideLayerWeigth>(), on);
        }
    }

    public void removeModeOn() {
        onClearPreview();
        competitionModeService.stateEnum = StateEnum.Remove;
    }
}