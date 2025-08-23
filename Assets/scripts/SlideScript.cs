using System;
using UnityEngine;
using UnityEngine.Serialization;

public class SlideScript : MonoBehaviour {
    private Vector3 localPosition0;
    private Vector3 localRotation0;
    private PistolScript pistolScript;
    
    public AudioSource sliderBackSound;
    public AudioSource sliderReleaseSound;
    float maxOffset = 0.009f;
    
    private Animator sliderAnimator;
    private Boolean sliderAnimationRunning;
    private bool slideLockedFlag = false;
    public bool shouldSliderLockTempFlagDebbug;
    private bool pendingLock;
    //private JointDrive savedDrive;
    
    private ConfigurableJoint configurableJoint;
    private Vector3 connectedAnchor;
    
    private void Awake() {
        sliderAnimator = GetComponent<Animator>();
        sliderAnimator.enabled = false;
        localPosition0 = transform.localPosition;
        localRotation0 = transform.localEulerAngles;
        
        pistolScript = GetComponentInParent<PistolScript>();
        
        configurableJoint = GetComponent<ConfigurableJoint>();
        //savedDrive = configurableJoint.xDrive;
        
        configurableJoint.autoConfigureConnectedAnchor = false;
        connectedAnchor = configurableJoint.connectedAnchor;
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
    
    public void onRunSliderMovedBack() {
        sliderMovedBackActions(false);
    }
	
    void Update () {
        Vector3 currentLocal = transform.localPosition;
        float dz = currentLocal.z - localPosition0.z;
        dz = Mathf.Clamp(dz, 0f, maxOffset);
        
        transform.localPosition = new Vector3(localPosition0.x, localPosition0.y, localPosition0.z + dz);
        transform.localEulerAngles = localRotation0;
        
        DetectSlidePull(); // проверяем, было ли полное передёргивание
        
        //todo yp temp log
        var d = configurableJoint.xDrive;
        Debug.Log($"spring={d.positionSpring}, damper={d.positionDamper}, maxF={d.maximumForce}, proj={configurableJoint.projectionMode}");
    }

    private bool hasTriggeredPullEvent = false;  // флаг, что затвор дёрнули назад
    private float slidePullThreshold = 0.0085f;  // насколько нужно оттянуть, чтобы считать, что затвор дёрнули

    private void LateUpdate() {
        //DetectSlidePull(); // проверяем, было ли полное передёргивание
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
        } else if (slideLockedFlag) {
            // снятие с задержки при дотяжке назад
            if (slideLockedFlag && /*!shouldSliderLockTempFlagDebbug*/ !pistolScript.shouldSliderLock()  /* dz >= lockDz + releaseOffset*/) {
                slideLockedFlag = false;
                //if (configurableJoint) configurableJoint.xDrive = savedDrive; // вернуть пружину
                slideLockRangeMove(0);
            }
        }
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

    public void DetectSlidePull() {
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
        slideLockedFlag = true;
        sliderAnimator.enabled = false;
        sliderAnimationRunning = false;

        slideLockRangeMove(0.008f);
        //ApplyFrontStop();

        /*JointDrive drive = configurableJoint.xDrive; // получить текущие настройки  //todo надо взять общий. что в переменных
        drive.positionSpring = 0f; // сила пружины
        configurableJoint.xDrive = drive; // записать обратно*/

        /*var pos = transform.localPosition;
        pos.z = -0.01290196f;
        transform.localPosition = pos;*/
    }
    
    private void slideLockRangeMove(float newLimit) {
        var connectedAnchorTemp = connectedAnchor;
        connectedAnchorTemp.z = localPosition0.z + newLimit;                  // или ca.x — по твоей оси
        configurableJoint.connectedAnchor = connectedAnchorTemp;

        var linearLimit = configurableJoint.linearLimit; 
        linearLimit.limit = newLimit; 
        configurableJoint.linearLimit = linearLimit; // ±half => окно [rear, rear+travel]
        //configurableJoint.targetPosition = Vector3.zero;// цель = центр окна
    }
    
    private void tryMoveRoundToChamber() {
        pistolScript.moveRoundFromMagazineToChamber();//пытаемся досылать патрон, если он есть
    }
    
    private void ejectRound(bool manual) {
        pistolScript.removeRoundFromChamber(manual);
    }
}
