using UnityEngine;

public class HandScript : MonoBehaviour {
    private GameObject pistol;
    private PistolScript pistolScript;

    void Awake() {
        if (pistol == null) {
            pistol = GameObject.Find("Glock17");
        }
        pistolScript = pistol.GetComponent<PistolScript>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.name == "pushHandPoint" 
            && pistolScript.hasMagazineChild() && !pistolScript.isMagazineLockedInPistol()) {
            pistolScript.getMagazineScript().magazineLock();            
        }
    }
}
