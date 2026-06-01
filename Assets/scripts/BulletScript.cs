using UnityEngine;

public class BulletScript : MonoBehaviour {
    private GameObject codeObject;
    public GameObject bulletHolePrefab; // Префаб пробоины
    private Main mainScript;
    private double min = 0.0001;
    private double max = 0.0009;
        
    void Start() {
        codeObject = GameObject.Find("codeObject");
        mainScript = codeObject.GetComponent<Main>();
    }
    
    private void OnCollisionEnter(Collision collision) {
        ContactPoint contact = collision.contacts[0];
        Vector3 hitPoint = contact.point;
        Quaternion hitRotation = Quaternion.FromToRotation(Vector3.back, contact.normal);

        float randomValue = Random.Range((float)min, (float)max);

        Vector3 offset = contact.normal * randomValue;
        Vector3 adjustedHitPoint = hitPoint + offset;

        mainScript.builderService.пробоины.Add(Instantiate(bulletHolePrefab, adjustedHitPoint, hitRotation));
        Destroy(gameObject);
    }
}