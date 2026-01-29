using UnityEngine;

public class MagazinesBagScript : MonoBehaviour {
    public GameObject magazineSpawn; 
    public GameObject magazinePrefub;
    private GameObject codeObject;
    private Main mainScript;
    
    private GameObject magazine;
    
    private bool isColor1Active = true;

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
