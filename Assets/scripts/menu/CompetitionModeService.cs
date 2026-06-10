using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class CompetitionModeService : MonoBehaviour {
    [Header("UI")]
    public TextMeshProUGUI readyText;
    public TextMeshProUGUI hintText;

    [Header("Sounds")]
    public AudioSource loadAndMakeReadySound;
    public AudioSource areYouReadySound;
    public AudioSource standByySound;
    public AudioSource beepSound;
    public AudioSource ifYouAreFinishedUnloadAndShowClear;
    public AudioSource ifClearHammerDownAndHolster;
    public AudioSource rangeIsClear;

    [HideInInspector] public bool inprocessCommand = false;
    [HideInInspector] public bool unloadAndShowClearCommandGiven = false;
    [HideInInspector] public bool hummerDownCommandGiven = false;

    private PistolScript pistolScript;
    private Coroutine pending;
    private float startTime;
    private bool running;
    private float lastShotTime;

    private MenuController menuController;
    private FloorScript floorScript;
    
    private StateEnum _stateEnum;
    public StateEnum stateEnum {
        get => _stateEnum;
        set {
            _stateEnum = value;
            if (floorScript != null) floorScript.highlight = isPlacementState(value);
        }
    }
    [HideInInspector] public RoundsCount roundsCount;

    // режимы, в которых идёт расстановка → показываем синий пол
    private static bool isPlacementState(StateEnum s) =>
        s == StateEnum.IPSC_target || s == StateEnum.IPSC_noshot || s == StateEnum.USPSA_target ||
        s == StateEnum.Barrel || s == StateEnum.Wall || s == StateEnum.DrawShootingZone ||
        s == StateEnum.MoveStage;
    
    [SerializeField] Image buttonBackground;
    [SerializeField] TMP_Text buttonLabel;

    private Color greenColor = new Color(0.30f, 0.52f, 0.31f);
    private Color redColor  = new Color(0.83f, 0.18f, 0.18f);

    private void setButtonColor(Color color) {
        buttonBackground.color = color;
    }
    
    public bool isShootMode() => stateEnum == StateEnum.StageRun;

    private void Start() {
        menuController = GetComponent<MenuController>();;
        pistolScript = GetComponent<PistolScript>();
        floorScript = GetComponent<FloorScript>();
    }

    public void init(PistolScript pistolScript) {
        this.pistolScript = pistolScript;
    }

    private IEnumerator After(float delay, Action action) {
        yield return new WaitForSeconds(delay);
        action();
    }

    private void startTimer() {
        startTime = Time.realtimeSinceStartup;
        lastShotTime = 0f;
        running = true;
    }

    public void registerShotTime() {
        if (!running) return;
        lastShotTime = Time.realtimeSinceStartup - startTime;
    }

    private void stopTimer() {
        running = false;
    }

    public string showTotalTime() => formatTime(lastShotTime);

    public string formatTime(float t) {
        int minutes = (int)(t / 60f);
        float seconds = t % 60f;
        return $"{minutes:00}:{seconds:00.00}";
    }

    public void interruptAttempt() {
        if (pending != null) StopCoroutine(pending);
        stopTimer();

        foreach (AudioSource source in FindObjectsOfType<AudioSource>())
            source.Stop();

        readyText.gameObject.SetActive(false);
        hintText.gameObject.SetActive(false);

        unloadAndShowClearCommandGiven = false;
        hummerDownCommandGiven = false;
        inprocessCommand = false;
        pistolScript.hammerDown = false;
        
        setButtonColor(greenColor);
        buttonLabel.text = "Start";
        stateEnum = StateEnum.Idle;
    }

    public void stopStage() {
        if (pending != null) StopCoroutine(pending);
        inprocessCommand = true;
        stopTimer();

        readyText.text = "If you are finished, unload and show clear";
        ifYouAreFinishedUnloadAndShowClear.Play();
        readyText.gameObject.SetActive(true);

        unloadAndShowClearCommandGiven = true;
    }

    public void sayHolsterCommand() {
        unloadAndShowClearCommandGiven = false;

        readyText.gameObject.SetActive(true);
        readyText.text = "If clear, hammer down and holster";
        ifClearHammerDownAndHolster.Play();

        hummerDownCommandGiven = true;
        pistolScript.hammerDown = false;
    }

    public void hideCommandsText() {
        readyText.gameObject.SetActive(false);
        hintText.gameObject.SetActive(false);
    }

    public void clearHintShotTime() {
        inprocessCommand = false;
        hummerDownCommandGiven = false;

        rangeIsClear.Play();
        readyText.text = "Your time: " + showTotalTime();
    }

    public void startStage() {
        inprocessCommand = true;
        showLoadAndMakeReadyCommand();
    }

    private void standBy() {
        readyText.text = "Stand by!";
        standByySound.Play();
        pending = StartCoroutine(After(UnityEngine.Random.Range(2f, 4f), beepAndStartTimer));
    }

    private void beepAndStartTimer() {
        readyText.gameObject.SetActive(false);
        beepSound.Play();
        startTimer();
        inprocessCommand = false;
    }

    private void showAreYouReadyCommand() {
        readyText.text = "Are you ready?";
        areYouReadySound.Play();
        hintText.gameObject.SetActive(false);
        pending = StartCoroutine(After(2f, standBy));
    }

    private void showLoadAndMakeReadyCommand() {
        readyText.gameObject.SetActive(true);
        hintText.gameObject.SetActive(true);
        readyText.text = "Load and make ready";
        loadAndMakeReadySound.Play();
        pending = StartCoroutine(After(4f, showAreYouReadyCommand));
    }

    public void startStopRangeAndMenu() {
        if (menuController.menu.activeSelf) {
            menuController.hideMenu();
        } else {
            menuController.showMenu();
        }
        startStopRange();
    }

    public void setRoundsCount(RoundsCount roundsCount) {
        this.roundsCount = roundsCount;
    }
    
    public void startStopRange() {
        if (stateEnum != StateEnum.StageRun) {
            stateEnum = StateEnum.StageRun;
            
            setButtonColor(redColor);
            buttonLabel.text = "Stop";
            startStage();
        } else {
            stateEnum = StateEnum.Idle;
            stopStage();
            setButtonColor(greenColor);
            buttonLabel.text = "Start";
        }
    }
}