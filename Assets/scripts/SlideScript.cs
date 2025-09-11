using System;
using Oculus.Interaction;
using UnityEngine;

public class SlideScript : MonoBehaviour {
    [SerializeField] private OneGrabTranslateTransformer oneGrabTranslateTransformer;
    
    private Vector3 localPosition0;
    private Vector3 localRotation0;
    private PistolScript pistolScript;
    
    public AudioSource sliderBackSound;
    public AudioSource sliderReleaseSound;
    float maxOffset = 0.009f;
    
    private Animator sliderAnimator;
    private Boolean sliderAnimationRunning;
    private bool slideLockedFlag = false;
    private bool pendingLock;
    
    private ConfigurableJoint configurableJoint;
    private Vector3 connectedAnchor;
    private const float погрешностьМеханизма = 0.001f;
    private Main mainScript;
    private BoxCollider boxCollider;
    
    private float holdSlideTimer = 0f;
    private const float delayToShowEmptyChamber = 2f;

    private void Awake() {
        sliderAnimator = GetComponent<Animator>();
        sliderAnimator.enabled = false;
        localPosition0 = transform.localPosition;
        localRotation0 = transform.localEulerAngles;
        
        startZ = localPosition0.z;
        backZ = startZ + maxOffset + погрешностьМеханизма;
        
        pistolScript = GetComponentInParent<PistolScript>();
        
        configurableJoint = GetComponent<ConfigurableJoint>();
        
        configurableJoint.autoConfigureConnectedAnchor = false;
        connectedAnchor = configurableJoint.connectedAnchor;
        
        mainScript = GameObject.Find("codeObject").GetComponent<Main>();;
        boxCollider = GetComponent<BoxCollider>();
    }

    public void runSliderAnimation() {
        sliderAnimationRunning = true;
        sliderAnimator.enabled = true;
        sliderAnimator.SetTrigger("SliderShotMove"); // запускает sliderShotMove
    }

    public void onRunSliderReturned() {
        sliderAnimator.enabled = false;
        sliderReturnedActions();
        sliderAnimationRunning = false;
    }
    
    /**
     * оттянул назад
     */
    public void onRunSliderMovedBack() {
        sliderMovedBackActions(false);
    }
	
    void Update () {
        moveSlideBySticker();

        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) setSlideUnLocked();
    }

    /**
     * двигать слайдер через стик
     */
    private void moveSlideBySticker() {
        if (sliderAnimationRunning) return;
        
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
        float down = Mathf.Max(0f, -stick.y);
        
        if (down > 0.05f) {
            boxCollider.enabled = false;
            if (oneGrabTranslateTransformer) oneGrabTranslateTransformer.enabled = false;
            if (slideLockedFlag) {
                if (down > 0.8f) {//порог срабатывания выше, если стоим на задержке. диапазон 0-1
                    slideBackMove();
                }
            } else {
                // стик рулит — ручной кламп не трогаем
                slideBackMove();
            }
        } else {
            boxCollider.enabled = true;
            // ручное перетягивание — только кламп
            if (oneGrabTranslateTransformer) oneGrabTranslateTransformer.enabled = true;

            Vector3 currentLocal = transform.localPosition;
            float dz = currentLocal.z - localPosition0.z;
            dz = Mathf.Clamp(currentLocal.z - localPosition0.z, 0f, maxOffset);

            transform.localPosition = new Vector3(localPosition0.x, localPosition0.y, localPosition0.z + dz);
        }
        transform.localEulerAngles = localRotation0;
    }

    private float startZ;        // положение вперёд
    private float backZ;

    private void slideBackMove() {
        // получаем ось стика
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
        float down = Mathf.Max(0f, -stick.y); // вниз = минус → делаем положительное значение

        float z = Mathf.Lerp(startZ, backZ, down);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
    }

    private bool hasTriggeredPullEvent = false;  // флаг, что затвор дёрнули назад
    private float slidePullThreshold = 0.0088f;  // насколько нужно оттянуть, чтобы считать, что затвор дёрнули
    private float slidePullShowСhamberThreshold = 0.006f;  // насколько нужно оттянуть, чтобы считать, что затвор дёрнули

    private void LateUpdate() {
        checkEmptyBackHold(); // проверяем показал ли патронник судье? (если надо)
        detectSlidePull(); // проверяем, было ли полное передёргивание
    }
    
    /**
     * если была команда судьей, вынял ли магазин и показал ли пустой патронник?
     */
    private void checkEmptyBackHold() {
        if (mainScript.unloadAndShowClearCommandGiven && !pistolScript.isMagazineInPistol()) {
            if (!sliderAnimationRunning) {
                float dz = transform.localPosition.z - localPosition0.z;

                if (dz >= slidePullShowСhamberThreshold && !pistolScript.isRoundInChamber()) {
                    holdSlideTimer += Time.deltaTime;
                    
                    if (holdSlideTimer >= delayToShowEmptyChamber) {
                        mainScript.sayHolsterCommand();
                        holdSlideTimer = 0f;
                    }
                } else {
                    holdSlideTimer = 0f;
                }
            }
        }
    }

    private void sliderMovedBackActions(bool manual) {
        // если затвор оттянули назад — запоминаем это
        if (!hasTriggeredPullEvent) {
            hasTriggeredPullEvent = true;
            
            if (!sliderBackSound.isPlaying) {
                sliderBackSound.PlayOneShot(sliderBackSound.clip);
            }
            
            if (pistolScript.isRoundInChamber()) {
                ejectRound(manual); // выброс патрона
            }

            pistolScript.hammerDown = false;
        } else if (slideLockedFlag) {
            // снятие с задержки при дотяжке назад
            if (slideLockedFlag && !pistolScript.shouldSliderLock()) {
                setSlideUnLocked();
            }
        }
    }

    private void setSlideLockedDelay() {
        slideLockedFlag = true;
        slideLockRangeMove(0.008f);
        oneGrabTranslateTransformer.Constraints.MinZ.Value = -0.0143f;
    }
    
    private void setSlideUnLocked() {
        slideLockedFlag = false;
        slideLockRangeMove(0);
        oneGrabTranslateTransformer.Constraints.MinZ.Value = -0.02058057f;
    }

    private void sliderReturnedActions() {
        // если затвор вернулся почти полностью вперёд — считаем передёргивание завершённым
        if (hasTriggeredPullEvent) {
            hasTriggeredPullEvent = false;
            
            if (!sliderReleaseSound.isPlaying) {
                sliderReleaseSound.PlayOneShot(sliderReleaseSound.clip);
            }
            
            tryMoveRoundToChamber(); // пробуем дослать патрон
        }
    }

    public void detectSlidePull() {
        checkSlideLock();
        
        if (!sliderAnimationRunning) {
            float dz = transform.localPosition.z - localPosition0.z;

            if (dz >= slidePullThreshold) {
                sliderMovedBackActions(true);
            }

            if (dz < 0.001f) {
                sliderReturnedActions();
            }
        }
    }
    
    /**
     * при ручном движении слайдера
     */
    private void checkSlideLock() {
        if (!sliderAnimationRunning && !slideLockedFlag && pistolScript.shouldSliderLock()) { // нахуа? если это же я чекаю в onSliderMovedBackAndLittleReturnEvent
            float dz = transform.localPosition.z - localPosition0.z;
            if (dz >= slidePullThreshold) {
                pendingLock = true;
            }
        }
    }

    /**
     * ивент для анимации, когда слайдер ушел назад, и вернулся немного
     */
    public void onSliderMovedBackAndLittleReturnEvent() {
        if (!slideLockedFlag && pistolScript.shouldSliderLock()) {
            pendingLock = true;
        }
    }
    
    void FixedUpdate() {
        if (!pendingLock) return;
        pendingLock = false;
        slideLock();
    }

    public void slideLock() {
        sliderAnimator.enabled = false;
        sliderAnimationRunning = false;
        hasTriggeredPullEvent = true;
        setSlideLockedDelay();
    }
    
    private void slideLockRangeMove(float newLimit) {
        var connectedAnchorTemp = connectedAnchor;
        connectedAnchorTemp.z = localPosition0.z + newLimit;                  // или ca.x — по твоей оси
        configurableJoint.connectedAnchor = connectedAnchorTemp;

        var linearLimit = configurableJoint.linearLimit; 
        linearLimit.limit = newLimit; 
        configurableJoint.linearLimit = linearLimit; // ±half => окно [rear, rear+travel]
    }
    
    private void tryMoveRoundToChamber() {
        pistolScript.moveRoundFromMagazineToChamber();//пытаемся досылать патрон, если он есть
    }
    
    private void ejectRound(bool manual) {
        pistolScript.removeRoundFromChamber(manual);
    }
}
