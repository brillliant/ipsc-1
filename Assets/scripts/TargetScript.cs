using UnityEngine;

/*
 * Реакция мишени на попадание. Вешается на объект с коллайдером мишени.
 * Пуля при столкновении сама ставит пробоину и уничтожается (BulletScript),
 * а мишень здесь отыгрывает свою реакцию: пока это звук, дальше — падение поппера, очки и т.д.
 */
public class TargetScript : MonoBehaviour {
    public AudioSource hitSound;

    private Animator animator;   // есть у поппера; у картонных мишеней его нет — все обращения под null-проверкой
    private bool fallen;

    private void Start() {
        animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.GetComponent<BulletScript>() == null) return; // реагируем только на пулю

        if (hitSound != null && hitSound.clip != null)
            hitSound.PlayOneShot(hitSound.clip);

        if (animator != null && !fallen) { // повторное попадание не взводит триггер ещё раз
            fallen = true;
            animator.SetTrigger("fall");
        }
    }

    // возврат мишени в исходное положение перед новым прогоном
    public void resetTarget() {
        if (animator == null) return;
        animator.ResetTrigger("fall");            // не даём висящему триггеру уронить поппер сразу после подъёма
        if (fallen) animator.SetTrigger("reset"); // reset по стоящему попперу не взводим — иначе он сорвёт следующее падение
        fallen = false;
    }
}
