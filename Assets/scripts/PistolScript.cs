using System;
using System.Collections;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class PistolScript : MonoBehaviour {
    [SerializeField] 
    private float bulletSpeed;
    public Boolean triggerPressed = false;
    public GameObject bulletPrefub;
    public GameObject magazine;
    
    public AudioSource shotSound;
    public AudioSource emptyShotSound;
    public AudioSource magazineOutSound;

    [SerializeField] private Transform bulletPoint;
    //public Transform leftHandTransform;
    public GameObject codeObject;
    private Main mainScript;
    
    private int roundsCount;// = 10000;
    private Boolean isСartridgeInChamber = true;
    private Boolean isMagazineIn = true;
    
    void Start() {
        roundsCount = magazine.GetComponent<MagazineScript>().getRoundCount();
        _vibration.Duration = 0.15f;
        _vibration.Samples = new[] { 1f };
        _vibration.SamplesCount = 1;
        shotSound.volume = 1.0f;

        mainScript = codeObject.GetComponent<Main>();
        //Time.timeScale = 0.1f;
    }

    private OVRInput.HapticsAmplitudeEnvelopeVibration _vibration = new OVRInput.HapticsAmplitudeEnvelopeVibration();

    void Update() {
        if (!mainScript.isTargetSetUpMenuActivated && !mainScript.isNoShotSetUpMenuActivated) {
            shootActionIfNeeded();
            if (OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) == 0 || Input.GetKeyUp(KeyCode.Space)) {
                triggerPressed = false;
            }
            
            if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) mainScript.clearHoles();
            if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch)) magazineAppears();
            if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch)) releaseMagazine();
        }
    }

    private void magazineAppears() {
        /*if (leftHandTransform != null) {
            transform.position = leftHandTransform.position;
            transform.rotation = leftHandTransform.rotation;
            transform.parent = leftHandTransform;
        }*/
    }

    private void releaseMagazine() {
        isMagazineIn = false;

        magazine.GetComponent<MeshCollider>().convex = true;
        magazine.GetComponent<Rigidbody>().isKinematic = false;
        magazine.GetComponent<Rigidbody>().useGravity = true;

        magazine.transform.parent = null;

        magazineOutSound.PlayOneShot(magazineOutSound.clip);
        magazine.transform.Find("ISDK_HandGrabInteraction").gameObject.SetActive(true);
    }
    void shootActionIfNeeded() {
        if (!triggerPressed && 
            (OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) > 0 || Input.GetKeyDown(KeyCode.Space))
            ) {
            
            triggerPressed = true;

            if (isСartridgeInChamber) {
                shoot();
            } else {
                emptyShoot();
            }
        }
    }

    private void emptyShoot() {
        emptyShotSound.PlayOneShot(emptyShotSound.clip);
    }
    
    private void shoot() {
        OVRInput.SetControllerHapticsAmplitudeEnvelope(_vibration, OVRInput.Controller.RTouch);
        var bullet = Instantiate(bulletPrefub);
        var bulletRigidbody = bullet.GetComponent<Rigidbody>();
        var transform1 = bullet.transform;
        transform1.position = bulletPoint.position;
        transform1.rotation = bulletPoint.rotation;

        bulletRigidbody.velocity = bulletPoint.forward * bulletSpeed;
        Destroy(bullet, 3);
        shotSound.PlayOneShot(shotSound.clip);
        isСartridgeInChamber = false;
        moveСartridgeFromMagazineToChamber();
    }

    private void moveСartridgeFromMagazineToChamber() {
        if (isMagazineIn && roundsCount > 0) {
            isСartridgeInChamber = true;
            roundsCount--;
            magazine.GetComponent<MagazineScript>().decrementRoundCount();
        }
    }

    void Template() {
        if (OVRInput.Get(OVRInput.Button.SecondaryShoulder)) {
            //int a = 2;
            Debug.Log("A button pressed");
        }
        
        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstick)) { //��������� ����������, ������ �����
            //int a = 2;
            Debug.Log("A button pressed");
        }
        
        if (OVRInput.Get(OVRInput.Button.SecondaryTouchpad)) {
            //int a = 2;
            Debug.Log("A button pressed");
        }
        
        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger)) { //��������� ����������, ������� �����
            //int a = 2;
            Debug.Log("A button pressed");
        }
        
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger)) { //��������� ����������, �����
            //int a = 2;
            Debug.Log("A button pressed");
        }
        
        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown)) { //��������� ����������, ���� ����
            //int a = 2;
            Debug.Log("A button pressed");
        }
        
        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight)) { //��������� ����������, ���� ������
            //int a = 2;
            Debug.Log("A button pressed");
        }

        if (OVRInput.Controller.RTouch.Equals(true)) {
            //int i = 3;
            Debug.Log("A button pressed");
        }
        
        if (OVRInput.Get(OVRInput.Button.Any)) {
            //int a = 2;
            Debug.Log("A button pressed");
        }

        if (OVRInput.Get(OVRInput.Button.One)) {  //������ � �����, ������� ��� �������
            //int a = 2;
            Debug.Log("A button pressed");
        }

        if (OVRInput.Get(OVRInput.Button.Two)) {  //������ B
            //int a = 2;
            Debug.Log("A button pressed");
        }

        if (OVRInput.Get(OVRInput.Button.Three)) {  //������ X
            //int a = 2;
            Debug.Log("A button pressed");
        }

        if (OVRInput.Get(OVRInput.Button.Four)) {  //������ Y
            //int a = 2;
            Debug.Log("A button pressed");
        }
         
        if (OVRInput.Get(OVRInput.Button.Start)) {  //������ left menu
            //int a = 2;
            Debug.Log("A button pressed");
        }

        if (OVRInput.Get(OVRInput.Button.Back)) {  // 
            //int a = 2;
            Debug.Log("A button pressed");
        }

        if (OVRInput.Get(OVRInput.Button.Down)) {  //��������� ����������, ���� ����
            //int a = 2;
            Debug.Log("A button pressed");
        }

        if (OVRInput.Get(OVRInput.Button.Left)) {  //��������� ����������, ���� ����� 
            //int a = 2;
            Debug.Log("A button pressed");
        }

        if (OVRInput.Get(OVRInput.Button.Up)) {  //��������� ����������, ���� �����
            //int a = 2;
            Debug.Log("A button pressed");
        }

        if (OVRInput.Get(OVRInput.Button.DpadDown)) {  // 
            //int a = 2;
            Debug.Log("A button pressed");
        }

        if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick)) {  //��������� ����������, ������ ����� 
            //int a = 2;
            Debug.Log("A button pressed");
        }

        /*        if (OVRInput.Get(OVRInput.Controller.RHand)) {  //������ 
                    //int a = 2;
                    Debug.Log("A button pressed");
                }*/

        //float IndexTrigger_Value = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);

        if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0) {  //��������� ����������, �����. ������� �������
            Debug.Log("A button pressed");
        }

        if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0) {  // 
            Debug.Log("A button pressed");
        }

        if (OVRInput.GetDown(OVRInput.Button.One)) {  //������ � �������
            Debug.Log("A button pressed");
        }
        
        //RawButton - конкретная кнопка конкретного контроллера. без праймари-секондари
        
        if (OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) > 0) {  //������ � ������� //TODO ПРОТЕСТИТЬ. ДОЛЖЕН БЫТЬ ПРАВЫЙ КУРОК!!!
            Debug.Log("A button pressed");
        }
        
        // if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger, OVRInput.Controller.Touch)) {
        //     
        // }
    }
}
