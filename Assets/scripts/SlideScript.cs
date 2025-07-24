using UnityEngine;
using UnityEngine.Serialization;

public class SlideScript : MonoBehaviour {
    private Vector3 localPosition0;
    private Vector3 localRotation0;
    private PistolScript pistolScript;
    
    public AudioSource sliderBackSound;
    public AudioSource sliderReleaseSound;
    float maxOffset = 0.009f;

    private void Awake() {
        localPosition0 = transform.localPosition;
        localRotation0 = transform.localEulerAngles;
        
        pistolScript = GetComponentInParent<PistolScript>();
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

    private void DetectSlidePull() {
        float dz = transform.localPosition.z - localPosition0.z;

        // если затвор оттянули назад — запоминаем это
        if (!hasTriggeredPullEvent && dz >= slidePullThreshold) {
            sliderBackSound.PlayOneShot(sliderBackSound.clip);
            hasTriggeredPullEvent = true;
            
            if (pistolScript.isRoundInChamber()) {
                ejectRound(); // выброс патрона
                pistolScript.setRoundInChamber(false);
            }
        }

        // если затвор вернулся почти полностью вперёд — считаем передёргивание завершённым
        if (hasTriggeredPullEvent && dz < 0.001f) {
            hasTriggeredPullEvent = false;
            sliderReleaseSound.PlayOneShot(sliderReleaseSound.clip);
            tryChamberRound(); // пробуем дослать патрон
        }
    }

    private void tryChamberRound() {
        pistolScript.moveRoundFromMagazineToChamber();//пытаемся досылать патрон, если он есть
    }
    
    private void ejectRound() {
        pistolScript.setRoundInChamber(false);
        //todo запустить анимацию выброса патрона (это может быть пустой, а может гильзя. пока похер)
    }
}
