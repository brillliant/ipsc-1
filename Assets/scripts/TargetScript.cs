using System.Collections;
using UnityEngine;

/*
 * Реакция мишени на попадание. Вешается на объект с коллайдером мишени.
 * Пуля при столкновении сама ставит пробоину и уничтожается (BulletScript),
 * а мишень здесь отыгрывает свою реакцию: пока это звук, дальше — падение поппера, очки и т.д.
 */
public class TargetScript : MonoBehaviour {
    // Звуки — объекты сцены sounds/MetallHit и sounds/PopperFall; их проставляет BuilderService при установке мишени
    public AudioSource hitSound;
    public AudioSource fallSound;

    private const float SpeedOfSound = 343f;   // м/с — лязг доходит до игрока с задержкой на дистанцию

    private Animator animator;   // есть у поппера; у картонных мишеней его нет — все обращения под null-проверкой
    private bool fallen;

    private void Start() {
        animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.GetComponent<BulletScript>() == null) return; // реагируем только на пулю

        if (hitSound != null && hitSound.clip != null)
            StartCoroutine(playHitSoundDelayed());

        if (animator != null && !fallen) { // повторное попадание не взводит триггер ещё раз
            fallen = true;
            animator.SetTrigger("fall");
        }
    }

    // полёт пули физика уже отыграла честно, а обратный путь звука от мишени до уха досчитываем сами
    private IEnumerator playHitSoundDelayed() {
        float delay = Camera.main != null
            ? Vector3.Distance(Camera.main.transform.position, transform.position) / SpeedOfSound
            : 0f;
        yield return new WaitForSeconds(delay);
        hitSound.PlayOneShot(hitSound.clip);
    }

    // Animation Event в клипе PopperFall (за 0.3 с до конца): поппер ложится в крайнее заднее положение
    public void onFallen() {
        if (fallSound != null && fallSound.clip != null)
            fallSound.PlayOneShot(fallSound.clip);
    }

    // возврат мишени в исходное положение перед новым прогоном
    public void resetTarget() {
        if (animator == null) return;
        animator.ResetTrigger("fall");            // не даём висящему триггеру уронить поппер сразу после подъёма
        if (fallen) animator.SetTrigger("reset"); // reset по стоящему попперу не взводим — иначе он сорвёт следующее падение
        fallen = false;
    }
}
