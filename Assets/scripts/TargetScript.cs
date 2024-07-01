using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScript : MonoBehaviour {
    public GameObject bulletHolePrefab; // Префаб пробоины
    
    // private void OnCollisionEnter(Collision collision) {
    //     if (collision.gameObject.name == "bullet2(Clone)") {
    //         ContactPoint contact = collision.contacts[0];
    //         Vector3 hitPoint = contact.point;
    //         Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
    //
    //         Instantiate(bulletHolePrefab, hitPoint, hitRotation);
    //         
    //         Destroy(collision.gameObject);
    //     }
    // }
}
