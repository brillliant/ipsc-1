using System;
using UnityEngine;

public class HandScript : MonoBehaviour {
    private GameObject pistol;
    private PistolScript pistolScript;
    private MagazinesBagScript magazinesBagScript;

    void Awake() {
        if (pistol == null) {
            pistol = GameObject.Find("Glock17");
        }
        pistolScript = pistol.GetComponent<PistolScript>();
        magazinesBagScript = GameObject.Find("Cube2").GetComponent<MagazinesBagScript>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.name == "pushHandPoint" 
            && pistolScript.hasMagazineChild() && !pistolScript.isMagazineLockedInPistol()) {
            pistolScript.getMagazineScript().magazineLock();            
        }
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.X) || OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch)) {
            magazinesBagScript.getReturnMag();
        }
    }
}
