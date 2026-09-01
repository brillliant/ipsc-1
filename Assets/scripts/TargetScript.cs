using UnityEngine;

/*
 * Реакция мишени на попадание. Вешается на объект с коллайдером мишени.
 * Пуля при столкновении сама ставит пробоину и уничтожается (BulletScript),
 * а мишень здесь отыгрывает свою реакцию: пока это звук, дальше — падение поппера, очки и т.д.
 */
public class TargetScript : MonoBehaviour {
    public AudioSource hitSound;

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.GetComponent<BulletScript>() == null) return; // реагируем только на пулю

        if (hitSound != null && hitSound.clip != null)
            hitSound.PlayOneShot(hitSound.clip);
    }
}
