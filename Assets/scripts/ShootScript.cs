using System;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class ShootScript : MonoBehaviour {
    [SerializeField] 
    private float bulletSpeed = 0.03f;
    public Boolean triggerPressed = false;
    public GameObject bulletPrefub;
    public GameObject magazine;
    
    public AudioSource shotSound;
    public AudioSource emptyShotSound;
    public AudioSource magazineOutSound;
    
    [SerializeField] private Transform bulletPoint;
    public GameObject codeObject;
    private Main mainScript;
    
    private int magazineBulletsCount = 5;
    private Boolean isBulletInChamber = true;
    private Boolean isMagazineIn = true;
    
    void Start() {
        _vibration.Duration = 0.15f;
        _vibration.Samples = new[] { 1f };
        _vibration.SamplesCount = 1;
        shotSound.volume = 0.4f;

        mainScript = codeObject.GetComponent<Main>();

        //Time.timeScale = 0.1f;
    }

    private OVRInput.HapticsAmplitudeEnvelopeVibration _vibration = new OVRInput.HapticsAmplitudeEnvelopeVibration();

    void Update() {
        if (!mainScript.isStageMenuActivated) {
            shootActionIfNeeded();
            if (OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) == 0 || Input.GetKeyUp(KeyCode.Space)) {
                triggerPressed = false;
            }
            
            if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) mainScript.clearHoles();
            if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch)) releaseMagazine();
        }
    }

    private void releaseMagazine() {
        isMagazineIn = false;
        
        magazine.GetComponent<MeshCollider>().convex = true;
        magazine.GetComponent<Rigidbody>().useGravity = true;
        
        magazineOutSound.PlayOneShot(magazineOutSound.clip);
    }

    void shootActionIfNeeded() {
        if (!triggerPressed && 
            (OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) > 0 || Input.GetKeyDown(KeyCode.Space))
            ) {
            triggerPressed = true;

            if (isBulletInChamber) {
                shoot();
            } else {
                emptyShoot();
            }
        }
    }

    private void emptyShoot() {
        emptyShotSound.Play();
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
        shotSound.Play();  //todo при новом проигрывании старое прекращается
        isBulletInChamber = false;
        //shotSound.Stop();
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
