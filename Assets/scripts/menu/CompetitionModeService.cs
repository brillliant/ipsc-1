using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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

    //[HideInInspector] public bool isStageStarted = false;
    [HideInInspector] public bool inprocessCommand = false;
    [HideInInspector] public bool unloadAndShowClearCommandGiven = false;
    [HideInInspector] public bool hummerDownCommandGiven = false;

    private PistolScript pistolScript;
    private Coroutine pending;
    private float startTime;
    private bool running;
    private float lastShotTime;

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

        //isStageStarted = false;
        unloadAndShowClearCommandGiven = false;
        hummerDownCommandGiven = false;
        inprocessCommand = false;
        pistolScript.hammerDown = false;
    }

    public void stopStage() {
        //isStageStarted = false;
        inprocessCommand = true;
        stopTimer();

        readyText.text = "If you are finished, unload and show clear";
        ifYouAreFinishedUnloadAndShowClear.PlayOneShot(ifYouAreFinishedUnloadAndShowClear.clip);
        readyText.gameObject.SetActive(true);

        unloadAndShowClearCommandGiven = true;
    }

    public void sayHolsterCommand() {
        unloadAndShowClearCommandGiven = false;

        readyText.gameObject.SetActive(true);
        readyText.text = "If clear, hammer down and holster";
        ifClearHammerDownAndHolster.PlayOneShot(ifClearHammerDownAndHolster.clip);

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

        rangeIsClear.PlayOneShot(rangeIsClear.clip);
        readyText.text = "Your time: " + showTotalTime();
    }

    public void startStage() {
        inprocessCommand = true;
        //isStageStarted = true;
        showLoadAndMakeReadyCommand();
    }

    private void standBy() {
        readyText.text = "Stand by!";
        standByySound.PlayOneShot(standByySound.clip);
        pending = StartCoroutine(After(UnityEngine.Random.Range(2f, 4f), beepAndStartTimer));
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
        pending = StartCoroutine(After(2f, standBy));
    }

    private void showLoadAndMakeReadyCommand() {
        readyText.gameObject.SetActive(true);
        hintText.gameObject.SetActive(true);
        readyText.text = "Load and make ready";
        loadAndMakeReadySound.PlayOneShot(loadAndMakeReadySound.clip);
        pending = StartCoroutine(After(4f, showAreYouReadyCommand));
    }
}