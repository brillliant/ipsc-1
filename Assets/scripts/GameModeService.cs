using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameModeService {
    private MonoBehaviour host;
    private Coroutine pending;

    private TextMeshProUGUI readyText;
    private TextMeshProUGUI hintText;
    private PistolScript pistolScript;

    private AudioSource loadAndMakeReadySound;
    private AudioSource areYouReadySound;
    private AudioSource standByySound;
    private AudioSource beepSound;
    private AudioSource ifYouAreFinishedUnloadAndShowClear;
    private AudioSource ifClearHammerDownAndHolster;
    private AudioSource rangeIsClear;

    public bool stageStarted = false;
    public bool inprocessCommand = false;
    public bool unloadAndShowClearCommandGiven = false;
    public bool hummerDownCommandGiven = false;

    private float startTime;
    private bool running;
    private float lastShotTime;

    public GameModeService(MonoBehaviour host, TextMeshProUGUI readyText, TextMeshProUGUI hintText,
        PistolScript pistolScript,
        AudioSource loadAndMakeReadySound, AudioSource areYouReadySound, AudioSource standByySound,
        AudioSource beepSound, AudioSource ifYouAreFinishedUnloadAndShowClear,
        AudioSource ifClearHammerDownAndHolster, AudioSource rangeIsClear) {
        this.host = host;
        this.readyText = readyText;
        this.hintText = hintText;
        this.pistolScript = pistolScript;
        this.loadAndMakeReadySound = loadAndMakeReadySound;
        this.areYouReadySound = areYouReadySound;
        this.standByySound = standByySound;
        this.beepSound = beepSound;
        this.ifYouAreFinishedUnloadAndShowClear = ifYouAreFinishedUnloadAndShowClear;
        this.ifClearHammerDownAndHolster = ifClearHammerDownAndHolster;
        this.rangeIsClear = rangeIsClear;
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

    public void registerShot() {
        if (!running) return;
        lastShotTime = Time.realtimeSinceStartup - startTime;
    }

    private void stopTimer() {
        running = false;
    }

    public string showTotalTime() {
        return formatTime(lastShotTime);
    }

    public string formatTime(float t) {
        int minutes = (int)(t / 60f);
        float seconds = t % 60f;
        return $"{minutes:00}:{seconds:00.00}";
    }

    public void interruptAttempt() {
        if (pending != null) host.StopCoroutine(pending);
        stopTimer();

        AudioSource[] allAudioSources = Object.FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in allAudioSources) {
            source.Stop();
        }
        readyText.gameObject.SetActive(false);
        hintText.gameObject.SetActive(false);

        stageStarted = false;
        unloadAndShowClearCommandGiven = false;
        hummerDownCommandGiven = false;
        inprocessCommand = false;
        pistolScript.hammerDown = false;
    }

    public void stopStage() {
        stageStarted = false;
        inprocessCommand = true;
        stopTimer();

        readyText.text = "If you are finished, unload and show clear";
        ifYouAreFinishedUnloadAndShowClear.PlayOneShot(ifYouAreFinishedUnloadAndShowClear.clip);
        readyText.gameObject.SetActive(true);

        unloadAndShowClearCommandGiven = true;
    }

    private void showTimeOnTheScreen() {
        readyText.text = "Your time: " + showTotalTime();
    }

    public void sayHolsterCommand() {
        unloadAndShowClearCommandGiven = false;

        readyText.gameObject.SetActive(true);
        readyText.text = "If clear, hammer down and holster";
        ifClearHammerDownAndHolster.PlayOneShot(ifClearHammerDownAndHolster.clip);

        hummerDownCommandGiven = true;
        pistolScript.hammerDown = false;
    }

    public void clearHintShotTime() {
        inprocessCommand = false;
        hummerDownCommandGiven = false;

        rangeIsClear.PlayOneShot(rangeIsClear.clip);
        showTimeOnTheScreen();
    }

    private void standBy() {
        readyText.text = "Stand by!";
        standByySound.PlayOneShot(standByySound.clip);
        pending = host.StartCoroutine(After(UnityEngine.Random.Range(2f, 4f), beepAndStartTimer));
    }

    private void beepAndStartTimer() {
        readyText.gameObject.SetActive(false);
        beepSound.PlayOneShot(beepSound.clip);

        startTimer();
        inprocessCommand = false;
    }

    private void showAreYouReadyCommand() {
        readyText.text = "Are you ready?";
        areYouReadySound.PlayOneShot(areYouReadySound.clip);
        hintText.gameObject.SetActive(false);

        pending = host.StartCoroutine(After(2f, standBy));
    }

    private void showLoadAndMakeReadyCommand() {
        readyText.gameObject.SetActive(true);
        hintText.gameObject.SetActive(true);

        readyText.text = "Load and make ready";
        loadAndMakeReadySound.PlayOneShot(loadAndMakeReadySound.clip);

        pending = host.StartCoroutine(After(4f, showAreYouReadyCommand));
    }

    public void startStage() {
        inprocessCommand = true;
        stageStarted = true;

        showLoadAndMakeReadyCommand();
    }
}
