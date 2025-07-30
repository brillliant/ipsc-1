using System;
using UnityEngine;

public class PistolScript : MonoBehaviour {
    [SerializeField] 
    private float bulletSpeed;
    public  float roundThrowSpeed;
    public Boolean triggerPressed = false;
    public GameObject bulletPrefub;
    public GameObject roundPrefub;
    public GameObject magazine;
    
    public AudioSource shotSound;
    public AudioSource emptyShotSound;
    public AudioSource magazineOutSound;
    public AudioSource magazineInSound;
    
    private SlideScript slideScript;

    [SerializeField] private Transform bulletPoint;
    private Transform throwRoundPoint;
    public GameObject codeObject;
    private Main mainScript;
    private MagazineScript magazineScript;
    
    private Boolean roundInChamber = false;
    private Boolean firedRound = false;
    private Boolean magazineLocked = true;
    private Boolean inShooting = false;
    
    void Start() {
        magazineScript = magazine.GetComponent<MagazineScript>();
        _vibration.Duration = 0.15f;
        _vibration.Samples = new[] { 1f };
        _vibration.SamplesCount = 1;
        shotSound.volume = 1.0f;

        mainScript = codeObject.GetComponent<Main>();
        //Time.timeScale = 0.1f;
    }

    private void Awake() {
        slideScript = transform.Find("Slide").GetComponent<SlideScript>();
        throwRoundPoint = transform.Find("throwRoundPoint").transform;
        
#if UNITY_EDITOR
        roundInChamber = true;
#endif
    }

    private OVRInput.HapticsAmplitudeEnvelopeVibration _vibration = new OVRInput.HapticsAmplitudeEnvelopeVibration();

    void Update() {
        if (!mainScript.isTargetSetUpMenuActivated && !mainScript.isNoShotSetUpMenuActivated) {
            shootActionIfNeeded();
            if (OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) == 0 || Input.GetKeyUp(KeyCode.Space)) {
                triggerPressed = false;
            }
            
            if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) mainScript.clearHoles();
            if (Input.GetKeyUp(KeyCode.U) || OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch)) releaseMagazine();
        }
    }

    private Transform findMagazine(Transform parent) {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(false)) {
            if (child == parent) continue; 
            if (child.name.Contains("Magazine")) {
                return child;
            }
        }
        return null;
    }

    private void releaseMagazine() {
        if (magazineLocked) {
            //todo сделать чтобы эти вещи сетались когда магазин установлен.
            magazine = findMagazine(transform.Find("MagazineRoot"))?.gameObject;
            magazineScript = magazine.GetComponent<MagazineScript>();
            
            magazineOutSound.PlayOneShot(magazineOutSound.clip);
            
            magazineLocked = false;
            magazine.GetComponent<Rigidbody>().isKinematic = false;
            magazine.GetComponent<Rigidbody>().useGravity = true;

            magazineScript.setIsMagazineMovingInGun(true);
        } else {
            //todo yp сделать другой звук
        }
    }

    void shootActionIfNeeded() {
        if (!triggerPressed && 
            (OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) > 0 
             || Input.GetKeyDown(KeyCode.Space))) {
            
            triggerPressed = true;

            if (roundInChamber && !inShooting) {
                inShooting = true;
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

        shotSound.PlayOneShot(shotSound.clip);
        bulletRigidbody.velocity = bulletPoint.forward * bulletSpeed;

        firedRound = true;
        
        Destroy(bullet, 3);
        slideScript.runSliderAnimation();
    }

    public void moveRoundFromMagazineToChamber() {
        if (magazineLocked && magazineScript.getRoundCount() > 0) {
            roundInChamber = true;
            firedRound = false;
            magazineScript.decrementRoundCount();
            inShooting = false;
        }
    }
    
    public Boolean isRoundInChamber() {
        return roundInChamber;
    }
    
    public void removeRoundFromChamber() {
        roundInChamber = false;
        
        var round = Instantiate(roundPrefub);
        var roundRigidbody = round.GetComponent<Rigidbody>();
        round.transform.position = throwRoundPoint.position;
        round.transform.rotation = transform.rotation;
        
        roundRigidbody.ResetCenterOfMass();
        roundRigidbody.centerOfMass = Vector3.zero;
        
        roundRigidbody.angularVelocity = UnityEngine.Random.insideUnitSphere * 7f;

        //shotSound.PlayOneShot(shotSound.clip);
        
        // Генерация направления с отклонением до 40°
        Vector3 direction = throwRoundPoint.forward;
        Quaternion tiltX = Quaternion.AngleAxis(UnityEngine.Random.Range(-20f, 20f), throwRoundPoint.right);
        Quaternion tiltY = Quaternion.AngleAxis(UnityEngine.Random.Range(-20f, 20f), throwRoundPoint.up);
        direction = tiltY * tiltX * direction;
        
        // Случайный множитель силы: 70%–130%
        var forceMultiplier = UnityEngine.Random.Range(0.7f, 1.3f);
        
        roundRigidbody.velocity = direction.normalized * (roundThrowSpeed * forceMultiplier);
        
        Destroy(round, 60);
    }

    public void setMagazineLocked(Boolean magazineLocked) {
        if (magazineLocked) {
            magazine = findMagazine(transform.Find("MagazineRoot"))?.gameObject;
            magazineScript = magazine.GetComponent<MagazineScript>();
            magazineInSound.PlayOneShot(magazineInSound.clip);
        } 
        this.magazineLocked = magazineLocked;
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
