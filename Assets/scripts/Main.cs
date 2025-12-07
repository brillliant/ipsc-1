using System;
using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Main : MonoBehaviour {
    public GameObject ipscTargetPreview;
    public GameObject ipscTargetPrefab;

    public GameObject ipscTargetNoShotPreview;
    public GameObject ipscTargetNoShotPrefab;
    
    public GameObject barrelPreview;
    public GameObject barrelPrefub;
    
    public GameObject wallPreview;
    public GameObject wallPrefub;

    public Boolean isHandKeepingMagazine = false;
    
    private GameObject currentPreview;
    
    public TextMeshProUGUI menuItem1_target;
    public TextMeshProUGUI menuItem2_shoot;
    public TextMeshProUGUI menuItem3_noShot;
    public TextMeshProUGUI menuItem4_dryFire;
    public TextMeshProUGUI menuItem5_barrel;
    public TextMeshProUGUI menuItem6_wall;

    public List<ObjectData> objectDataList;
    public List<GameObject> установленныеМишени;
    public List<GameObject> пробоины;

    private List<TextMeshProUGUI> menuList;
    
    private int currentIndex = 0;
    public Boolean isTargetSetUpMenuActivated = true;
    public Boolean isNoShotSetUpMenuActivated = false;
    public Boolean triggerPressed = false;
    public GameObject effectMeshObject;
    
    private string targetLayer = "Character";
    
    private LineRenderer line;
    private Transform bulletPoint;

    private TextMeshProUGUI readyText;
    private TextMeshProUGUI hintText;
    private bool stageStarted = false;  //это выполнение упражнения. стейдж.
                                        //а Attempt - это все, пока пистолет не опустится назад в кобуру и судья не скажет "рендж клиар"
    public AudioSource loadAndMakeReadySound;
    public AudioSource areYouReadySound;
    public AudioSource standByySound;
    public AudioSource beepSound;
    public AudioSource ifYouAreFinishedUnloadAndShowClear;
    public AudioSource ifClearHammerDownAndHolster;
    public AudioSource rangeIsClear;
    
    private bool running;
    private float startTime;
    private float lastShotTime;
    private bool inprocessCommand;
    public bool unloadAndShowClearCommandGiven = false;
    public bool hummerDownCommandGiven = false;

    private GameObject pistol;
    private PistolScript pistolScript;

    private MeshRenderer pushHandPointOnPistolMesh;

    private GameObject leftHand;
    private MeshRenderer pushMagazinePointOnHandMesh;
    private FloorScript floorScript;
    
    void Start() {
        InvokeRepeating(nameof(setHandColliderLayer), 1f, 1f); // кажду секунду пробуем задать слой для левой руки
        pistol = GameObject.Find("Glock17");
        pistolScript = pistol.GetComponent<PistolScript>();
        floorScript = GetComponent<FloorScript>();
        bulletPoint = pistolScript.bulletPoint;

        readyText = GameObject.Find("ready").GetComponent<TextMeshProUGUI>();
        hintText = GameObject.Find("hint").GetComponent<TextMeshProUGUI>();
        
        objectDataList = new List<ObjectData>();
        установленныеМишени = new List<GameObject>();

        menuList = new List<TextMeshProUGUI>();
        menuList.Add(menuItem1_target);
        menuList.Add(menuItem2_shoot);
        menuList.Add(menuItem3_noShot);
        menuList.Add(menuItem4_dryFire);
        menuList.Add(menuItem5_barrel);
        menuList.Add(menuItem6_wall);
        
        //todo demo temp
        changeMenu(); changeMenu(); changeMenu();
        menuItem1_target.enabled = false;
        menuItem2_shoot.enabled = false;
        menuItem3_noShot.enabled = false;
        menuItem4_dryFire.enabled = false;
        menuItem5_barrel.enabled = false;
        menuItem6_wall.enabled = false;
        
#if UNITY_EDITOR
        //changeMenu();
#endif
        
        pushHandPointOnPistolMesh = pistol.transform.Find("pushHandPoint/Sphere").gameObject.GetComponent<MeshRenderer>();
        pushMagazinePointOnHandMesh = GameObject.Find("pushMagazinePointOnHand").gameObject.GetComponent<MeshRenderer>();
        leftHand = GameObject.Find("OpenXRLeftHand").transform.Find("LeftHand").gameObject;
    }
    
    void Update() {
        if (isTargetSetUpMenuActivated) {
            paintRay();
            setUpObject(ipscTargetPreview, ipscTargetPrefab);
            
            readyText.gameObject.SetActive(false);
            hintText.gameObject.SetActive(false);
        } else if (isNoShotSetUpMenuActivated) {
            paintRay();
            setUpObject(ipscTargetNoShotPreview, ipscTargetNoShotPrefab);
            
            readyText.gameObject.SetActive(false);
            hintText.gameObject.SetActive(false);
        } else if (currentIndex == 4) {//barrel
            paintRay();
            setUpObject(barrelPreview, barrelPrefub);
            
            readyText.gameObject.SetActive(false);
            hintText.gameObject.SetActive(false);
        } else if (currentIndex == 5) {//wall
            paintRay();
            setUpObject(wallPreview, wallPrefub);
            
            readyText.gameObject.SetActive(false);
            hintText.gameObject.SetActive(false);
        } else {
            hideRay();
        }
        
        if (Input.GetKeyUp(KeyCode.J) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight)) changeMenu();
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft)) showHideDebugMesh();
        
        if (!(isTargetSetUpMenuActivated && isNoShotSetUpMenuActivated) 
            && !stageStarted
            && !inprocessCommand
            && (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch)  
                || Input.GetKeyDown(KeyCode.Z))) {
            
            startStage();
        }
        
        if (!(isTargetSetUpMenuActivated && isNoShotSetUpMenuActivated) 
            && stageStarted 
            && !inprocessCommand
            && (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch)  
                || Input.GetKeyDown(KeyCode.Z))) {
            
            stopStage();
        }
        
        if (!(isTargetSetUpMenuActivated && isNoShotSetUpMenuActivated) 
            //&& stageStarted  выключить можно всегда.  если что. добавить флаг. AttemptStarted
            && (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch)  
                || Input.GetKeyDown(KeyCode.LeftShift))) {
            
            interruptAttempt();
        }
    }
    
    private void startTimer() {
        startTime = Time.realtimeSinceStartup;
        lastShotTime = 0f;
        running = true;
    }
    public void registerShot() {
        if (!running) return;
        lastShotTime = Time.realtimeSinceStartup - startTime;
    }
    private void stopTimer() {
        running = false;
    }
    
    // текущее время с момента старта (для дисплея)
    private float elapsed() {
        return running ? Time.realtimeSinceStartup - startTime : lastShotTime;
    }
    
    // итоговое время (равно последнему зафиксированному выстрелу)
    public string showTotalTime() {
        return formatTime(lastShotTime);
    }
    
    public string formatTime(float t) {
        int minutes = (int)(t / 60f);
        float seconds = t % 60f;
        return $"{minutes:00}:{seconds:00.00}";
    }
    
    void setHandColliderLayer() {
        GameObject capsules = GameObject.Find("Capsules");
        if (capsules == null) return;

        int layer = LayerMask.NameToLayer(targetLayer);
        SetLayerRecursive(capsules, layer);
        CancelInvoke(nameof(setHandColliderLayer)); // один раз хватит
    }

    void SetLayerRecursive(GameObject obj, int layer) {
        obj.layer = layer;
        foreach (Transform child in obj.transform) {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    private void interruptAttempt() {
        CancelInvoke();
        stopTimer();
        
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in allAudioSources) {
            source.Stop();
        }
        readyText.gameObject.SetActive(false);
        hintText.gameObject.SetActive(false);
        
        stageStarted = false;
        unloadAndShowClearCommandGiven = false;
        hummerDownCommandGiven = false;
        inprocessCommand = false;
        pistolScript.hammerDown = false;
    }
    
    private void stopStage() {
        stageStarted = false;
        inprocessCommand = true;
        stopTimer();
        
        readyText.text = "If you are finished, unload and show clear";
        ifYouAreFinishedUnloadAndShowClear.PlayOneShot(ifYouAreFinishedUnloadAndShowClear.clip);
        readyText.gameObject.SetActive(true);

        unloadAndShowClearCommandGiven = true;
    }

    public void sayHolsterCommand() {
        unloadAndShowClearCommandGiven = false;
        
        readyText.gameObject.SetActive(true);
        readyText.text = "If clear, hammer down and holster";
        ifClearHammerDownAndHolster.PlayOneShot(ifClearHammerDownAndHolster.clip);

        hummerDownCommandGiven = true;
        pistolScript.hammerDown = false;
    }
    
    public void clearHintShotTime() {
        inprocessCommand = false;
        hummerDownCommandGiven = false;
        
        rangeIsClear.PlayOneShot(rangeIsClear.clip);

        showTimeOnTheScreen();
    }

    private void showTimeOnTheScreen() {
        readyText.text = "Your time: " + showTotalTime();
    }

    private void startStage() {
        inprocessCommand = true;
        stageStarted = true;

        showLoadAndMakeReadyCommand();
    }

    private void showLoadAndMakeReadyCommand() {
        readyText.gameObject.SetActive(true);
        hintText.gameObject.SetActive(true);
        
        readyText.text = "Load and make ready";
        loadAndMakeReadySound.PlayOneShot(loadAndMakeReadySound.clip);
        //hintText.text = "(press left trigger when ready)";
        
        Invoke(nameof(showAreYouReadyCommand), 4f);
    }
    
    private void showAreYouReadyCommand() {
        readyText.text = "Are you ready?";
        areYouReadySound.PlayOneShot(areYouReadySound.clip);
        hintText.gameObject.SetActive(false);
        
        Invoke(nameof(standBy), 2f);
    }
    
    private void standBy() {
        readyText.text = "Stand by!";
        standByySound.PlayOneShot(standByySound.clip);
        Invoke(nameof(beepAndStartTimer), Random.Range(2f, 4f));
    }
    
    private void beepAndStartTimer() {
        readyText.gameObject.SetActive(false);
        beepSound.PlayOneShot(beepSound.clip);
        
        startTimer();
        inprocessCommand = false;
    }

    private void showHideDebugMesh() {
        pushHandPointOnPistolMesh.enabled = !pushHandPointOnPistolMesh.enabled;
        leftHand.SetActive(!leftHand.activeSelf);
        pushMagazinePointOnHandMesh.enabled = !pushMagazinePointOnHandMesh.enabled;
        floorScript.highlight = !floorScript.highlight;
    }

    protected void setUpObject(GameObject preview, GameObject prefab) {
        if (!currentPreview) currentPreview = Instantiate(preview);
        if (currentPreview && !currentPreview.activeSelf) currentPreview.SetActive(true);

        Ray ray = new Ray(bulletPoint.position, bulletPoint.forward); 
        
        if (Physics.Raycast(ray, out RaycastHit hit) && !hit.collider.gameObject.name.Equals("emptyObjectForCollider")
                                                     && !hit.collider.gameObject.name.Equals("Glock17")) {
            
            placeToSurface(currentPreview, hit);
            
            // currentPreview.transform.position = hit.point;
            // currentPreview.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // Поворот модели лицом к камере
            Vector3 cameraPosition = Camera.main.transform.position;
            currentPreview.transform.LookAt(new Vector3(cameraPosition.x, currentPreview.transform.position.y, cameraPosition.z));

            if (OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) == 0 || Input.GetKeyUp(KeyCode.Space)) {
                triggerPressed = false;
            }

            if (!triggerPressed &&
                (Input.GetKeyDown(KeyCode.Space) || OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) > 0.5)
                ) {
                placeATarget(currentPreview, prefab);
            }
        }

        if (OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger) > 0.5) 
            SaveObjects();
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) 
            RemoveAllObjects();
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch)) 
            LoadObjects();
    }

    private void paintRay() {
        if (!line) {
            var rayGameObject = new GameObject("Ray");
            rayGameObject.transform.SetParent(bulletPoint, false);
            line = rayGameObject.AddComponent<LineRenderer>();
            line.startWidth = 0.005f;
            line.endWidth   = 0.001f;
            line.material = new Material(Shader.Find("Sprites/Default")); // стерео-совместимый
            line.material.color = Color.red;
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.alignment = LineAlignment.View;
        }
        
        if (!line.enabled) line.enabled = true;

        line.SetPosition(0, bulletPoint.position);               // визуализация того же луча
        line.SetPosition(1, bulletPoint.position + bulletPoint.forward * 10f);
    }
    
    private void hideRay() {
        if (!line) return;
        line.enabled = false;
    }

    private int getNextIndex() {
        if (currentIndex + 1 <= menuList.Count - 1) {
            currentIndex++;
        } else {
            currentIndex = 0;
        }
        return currentIndex;
    }

    private void changeMenu() {
        if (currentPreview) {
            currentPreview.SetActive(false);
            Destroy(currentPreview); 
        }
        
        int index = getNextIndex();

        highlightNecessaryMenuItem(index);

        if (currentIndex == 0) {
            isTargetSetUpMenuActivated = true;
            isNoShotSetUpMenuActivated = false;
        } else if (currentIndex == 2) {
            isTargetSetUpMenuActivated = false;
            isNoShotSetUpMenuActivated = true;
        } else {
            isTargetSetUpMenuActivated = false;
            isNoShotSetUpMenuActivated = false;
        }

        if (currentIndex == 1) {
            pistolScript.setMagRoundCount(15);
        } else if (currentIndex == 3) {
            pistolScript.setMagRoundCount(int.MaxValue);
        }
    }

    private void highlightNecessaryMenuItem(int index) {
        for (int i = 0; i < menuList.Count; i++) {
            if (i == index) {
                menuList[i].fontSize = 8;
                menuList[i].fontStyle = FontStyles.Bold;
                menuList[i].color = Color.red;
            } else {
                menuList[i].fontSize = 5;
                menuList[i].fontStyle = FontStyles.Normal;
                menuList[i].color = Color.gray;
            }
        }
    }

    private void placeATarget(GameObject preview, GameObject prefab) {
        triggerPressed = true;
        установленныеМишени.Add(Instantiate(prefab, preview.transform.position, preview.transform.rotation));
    }

    public void SaveObjects() {
        objectDataList.Clear();

        // Добавьте ваши объекты в список
        foreach (GameObject obj in установленныеМишени) {
            ObjectData data = new ObjectData(obj.name, obj.transform.position, obj.transform.rotation);
            objectDataList.Add(data);
        }

        // Оберните список в объект-обертку
        ObjectDataList wrapper = new ObjectDataList { objectDataList = objectDataList };

        // Сериализуйте объект-обертку
        string json = JsonUtility.ToJson(wrapper);
        File.WriteAllText(Application.persistentDataPath + "/saveData.json", json);
    }

    public void RemoveAllObjects() {
        foreach (GameObject obj in установленныеМишени) {
            // Проверяем, что это не объект самого скрипта, чтобы избежать его удаления
            if (obj != gameObject) {
                Destroy(obj);
            }
        }
        установленныеМишени.Clear();
        clearHoles();
    }

    public void clearHoles() {
        foreach (GameObject пробоина in пробоины) {
            if (пробоина != gameObject) {
                Destroy(пробоина);
            }
        }
        пробоины.Clear();
    }

    public int getCurrentIndex() {
        return currentIndex;
    }
    
    public bool isShootMode() {
        return currentIndex is 1 or 3;
    }

    public void LoadObjects() {
        RemoveAllObjects();
 
        string path = Application.persistentDataPath + "/saveData.json";
        if (File.Exists(path)) {
            string json = File.ReadAllText(path);

            // Десериализуйте JSON в объект-обертку
            ObjectDataList wrapper = JsonUtility.FromJson<ObjectDataList>(json);

            // Извлеките список из объекта-обертки
            if (wrapper.objectDataList != null) {
                objectDataList = wrapper.objectDataList;
            }
 
            // Восстановите объекты
            foreach (ObjectData data in objectDataList) {
                GameObject prefab = Resources.Load<GameObject>(/*"ipcs_target/" + */data.prefabName.Substring(0, data.prefabName.Length - 7));

                if (prefab != null) {
                    установленныеМишени.Add(
                    Instantiate(prefab, data.position, data.rotation)
                    );
                } else {
                  Debug.LogWarning("Prefab not found: " + data.prefabName);
                }
            }
        }
    }
    
    private void placeToSurface(GameObject go, RaycastHit hit, float pad = 0.002f) {
        var n = hit.normal.normalized;

        // выровнять "вверх" по нормали
        go.transform.rotation = Quaternion.FromToRotation(Vector3.up, n);

        // взять границы: сначала из коллайдеров, иначе из рендереров
        Bounds b;
        var cols = go.GetComponentsInChildren<Collider>(true);
        if (cols.Length > 0) {
            Physics.SyncTransforms();
            b = cols[0].bounds; for (int i = 1; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);
        } else {
            var rs = go.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 0) { go.transform.position = hit.point + n * pad; return; }
            b = rs[0].bounds; for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        }

        Vector3 ext = b.extents;
        float rN = Vector3.Dot(ext, new Vector3(Mathf.Abs(n.x), Mathf.Abs(n.y), Mathf.Abs(n.z)));
        float pivotToCenterN = Vector3.Dot(b.center - go.transform.position, n);

        go.transform.position = hit.point + n * (rN - pivotToCenterN + pad);
    }
}