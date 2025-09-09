using UnityEngine;

public class InsertPositionValidationScript : MonoBehaviour {
    private MagazineScript magazineScript;
    private PistolScript pistolScript;
    private GameObject reloadPoint2;
    private const float orientationToleranceDegrees = 35f;
    private Transform magazineRootTransform;

    private void Start() {
        magazineScript = GetComponentInParent<MagazineScript>();
        reloadPoint2 = magazineScript.reloadPoint2;
        pistolScript = magazineScript.pistolScript;
        
        magazineRootTransform = magazineScript.magazineRoot.transform;
    }
    
    void OnTriggerEnter(Collider other) {
        if (other.gameObject == reloadPoint2 && !magazineScript.isMagazineMovingInGun &&
            !pistolScript.hasMagazineChild()) {
            
            if (isInsertingMagazineAnglesValid()) {
                magazineScript.startInsertion();
            }
        }
    }
    
    private bool isInsertingMagazineAnglesValid() {
        float cosTol = Mathf.Cos(orientationToleranceDegrees * Mathf.Deg2Rad);

        if (Vector3.Dot(transform.forward, magazineRootTransform.forward) < cosTol) return false;
        if (Vector3.Dot(transform.up,      magazineRootTransform.up)      < cosTol) return false;

        return true;
    }
}
