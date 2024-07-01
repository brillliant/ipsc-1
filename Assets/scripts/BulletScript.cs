using UnityEngine;

public class BulletScript : MonoBehaviour {
    private GameObject codeObject;
    public GameObject bulletHolePrefab; // Префаб пробоины
    private Main mainScript;
        
    void Start() {
        codeObject = GameObject.Find("codeObject");
        mainScript = codeObject.GetComponent<Main>();
    }
    
    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.name == "TargetMain") {
            ContactPoint contact = collision.contacts[0];
            Vector3 hitPoint = contact.point;
            Quaternion hitRotation = Quaternion.FromToRotation(Vector3.back, contact.normal);

            mainScript.пробоины.Add(Instantiate(bulletHolePrefab, hitPoint, hitRotation));
        }
        Destroy(gameObject);
    }
}
