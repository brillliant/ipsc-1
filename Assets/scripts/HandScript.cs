using UnityEngine;

public class HandScript : MonoBehaviour {
    private GameObject pistol;
    private PistolScript pistolScript;
    
    public GameObject magazineSpawn; 
    public GameObject magazinePrefub;
    private GameObject codeObject;
    private Main mainScript;
    
    private GameObject magazine;
    
    private bool isColor1Active = true;

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

    void Update() {
        if (Input.GetKeyUp(KeyCode.X) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) {
            getReturnMag();
        }
    }

    void Start() {
        codeObject = GameObject.Find("codeObject");
        mainScript = codeObject.GetComponent<Main>();
    }

    public void getReturnMag() {
        if (!mainScript.isHandKeepingMagazine) {
            instantiateMagazineInHand();
        }
        else {
            Destroy(magazine);
            mainScript.isHandKeepingMagazine = false;
        }
    }

    private void instantiateMagazineInHand() {
        magazine = Instantiate(
            magazinePrefub,
            new Vector3(
                magazineSpawn.transform.position.x,
                magazineSpawn.transform.position.y,
                magazineSpawn.transform.position.z
            ),
            Quaternion.Euler(
                magazineSpawn.transform.rotation.eulerAngles.x,
                magazineSpawn.transform.rotation.eulerAngles.y,
                magazineSpawn.transform.rotation.eulerAngles.z
            ),
            magazineSpawn.transform
        );
        mainScript.isHandKeepingMagazine = true;
        Debug.Log("схватил магазин");
    }
}