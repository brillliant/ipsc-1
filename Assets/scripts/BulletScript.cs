using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {
    public GameObject bulletHolePrefab; // Префаб пробоины

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.name == "TargetMain") {
            ContactPoint contact = collision.contacts[0];
            Vector3 hitPoint = contact.point;
            Quaternion hitRotation = Quaternion.FromToRotation(Vector3.back, contact.normal);

            Instantiate(bulletHolePrefab, hitPoint, hitRotation);
        }
        Destroy(gameObject);
    }
}
