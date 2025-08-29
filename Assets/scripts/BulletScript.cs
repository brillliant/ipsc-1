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
        if (collision.gameObject.name == "TargetMain") {
            ContactPoint contact = collision.contacts[0];
            Vector3 hitPoint = contact.point;
            Quaternion hitRotation = Quaternion.FromToRotation(Vector3.back, contact.normal);

            float randomValue = Random.Range((float)min, (float)max);
            
            // Смещение по направлению нормали для предотвращения Z-fighting
            Vector3 offset = contact.normal * randomValue;
            Vector3 adjustedHitPoint = hitPoint + offset;
            
            mainScript.пробоины.Add(Instantiate(bulletHolePrefab, adjustedHitPoint, hitRotation));
        }
        Destroy(gameObject);
    }
}
