using UnityEngine;

public class BulletScript : MonoBehaviour {
    private GameObject codeObject;
    public GameObject bulletHolePrefab; // Префаб пробоины
    private Main mainScript;
    private double min = 0.0001;
    private double max = 0.0009;

    // Awake, а не Start: столкновение может случиться в первом же шаге физики, ещё до Start
    void Awake() {
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

        GameObject пробоина = Instantiate(bulletHolePrefab, adjustedHitPoint, hitRotation);
        пробоина.transform.SetParent(collision.collider.transform, true);   // дырка едет вместе с мишенью (упавший поппер, сдвиг стейджа)
        mainScript.builderService.пробоины.Add(пробоина);
        Destroy(gameObject);
    }
}