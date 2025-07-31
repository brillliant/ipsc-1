using System;
using UnityEngine;

public class SlideScript : MonoBehaviour {
    private Vector3 localPosition0;
    private Vector3 localRotation0;
    private PistolScript pistolScript;
    
    public AudioSource sliderBackSound;
    public AudioSource sliderReleaseSound;
    float maxOffset = 0.009f;
    
    private Animator sliderAnimator;
    private Boolean sliderAnimationRunning;
    
    private void Awake() {
        sliderAnimator = GetComponent<Animator>();
        sliderAnimator.enabled = false;
        localPosition0 = transform.localPosition;
        localRotation0 = transform.localEulerAngles;
        
        pistolScript = GetComponentInParent<PistolScript>();
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
    }

    private bool hasTriggeredPullEvent = false;  // флаг, что затвор дёрнули назад
    private float slidePullThreshold = 0.0085f;  // насколько нужно оттянуть, чтобы считать, что затвор дёрнули

    private void LateUpdate() {
        DetectSlidePull(); // проверяем, было ли полное передёргивание
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
        }
    }

    private void sliderReturnedActions() {
        // если затвор вернулся почти полностью вперёд — считаем передёргивание завершённым
        if (hasTriggeredPullEvent) {
            hasTriggeredPullEvent = false;
            
            if (!sliderReleaseSound.isPlaying) {
                sliderReleaseSound.PlayOneShot(sliderReleaseSound.clip);
            }
            
            tryChamberRound(); // пробуем дослать патрон
        }
    }

    public void DetectSlidePull() {
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

    private void tryChamberRound() {
        pistolScript.moveRoundFromMagazineToChamber();//пытаемся досылать патрон, если он есть
    }
    
    private void ejectRound(bool manual) {
        pistolScript.removeRoundFromChamber(manual);
        //todo запустить анимацию выброса патрона (это может быть пустой, а может гильзя. пока похер)
    }
}
