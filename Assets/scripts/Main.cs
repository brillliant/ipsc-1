using System;
using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using Meta.XR.MRUtilityKit;
using TMPro;
using UnityEngine;

public class Main : MonoBehaviour {
    public GameObject targetPrefab;
    public GameObject previewPrefab;
    public GameObject previewPrefabNoShot;
    public GameObject prefabNoShot;
    public GameObject cube2Temp;
    public Boolean isHandKeepingMagazine = false;
    
    private GameObject currentPreview;
    //private GameObject currentNoShotPreview;
    private EffectMesh effectMeshScript;
    
    public TextMeshProUGUI menuItem1_stage;
    public TextMeshProUGUI menuItem2_shoot;
    public TextMeshProUGUI menuItem3_noShot;

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
    
    void Start() {
        InvokeRepeating(nameof(SetHandColliderLayer), 1f, 1f); // кажду секунду пробуем задать слой для левой руки
        
        objectDataList = new List<ObjectData>();
        установленныеМишени = new List<GameObject>();
        //currentPreview = Instantiate(previewPrefab);
        effectMeshScript = effectMeshObject.GetComponent<EffectMesh>();

        menuList = new List<TextMeshProUGUI>();
        menuList.Add(menuItem1_stage);
        menuList.Add(menuItem2_shoot);
        menuList.Add(menuItem3_noShot);
    }
    
    void SetHandColliderLayer() {
        GameObject capsules = GameObject.Find("Capsules");
        if (capsules == null) return;

        int layer = LayerMask.NameToLayer(targetLayer);
        SetLayerRecursive(capsules, layer);
        CancelInvoke(nameof(SetHandColliderLayer)); // один раз хватит
    }

    void SetLayerRecursive(GameObject obj, int layer) {
        obj.layer = layer;
        foreach (Transform child in obj.transform) {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    void Update() {
        if (isTargetSetUpMenuActivated) {
            setUpTargets(previewPrefab, targetPrefab);
        } else if (isNoShotSetUpMenuActivated) {
            setUpTargets(previewPrefabNoShot, prefabNoShot);
        } else {
            currentPreview.SetActive(false);
        }
        //menu change
        if (Input.GetKeyUp(KeyCode.Y) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown)) changeMenu();
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft)) showHideWalls();
    }

    private void showHideWalls() {
        if (effectMeshScript.HideMesh) {
            effectMeshScript.HideMesh = false;
            tempColorChange();
        } else {
            effectMeshScript.HideMesh = true;
            tempColorChange();
        }
    }

    protected void setUpTargets(GameObject previewPrefab, GameObject prefab) {
        if (!currentPreview) currentPreview = Instantiate(previewPrefab);
        if (currentPreview && !currentPreview.activeSelf) currentPreview.SetActive(true);

        Ray ray = new Ray(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch),
            OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward);

        if (Physics.Raycast(ray, out RaycastHit hit)
            && !hit.collider.gameObject.name.Equals("emptyObjectForCollider")
            && !hit.collider.gameObject.name.Equals("Glock17")) {
            currentPreview.transform.position = hit.point;
            currentPreview.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // Поворот модели лицом к камере
            Vector3 cameraPosition = Camera.main.transform.position;
            currentPreview.transform.LookAt(new Vector3(cameraPosition.x, currentPreview.transform.position.y,
                cameraPosition.z));

            if (OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) == 0 || Input.GetKeyUp(KeyCode.Space)) {
                triggerPressed = false;
            }

            if (!triggerPressed && OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) > 0.5) placeATarget(hit, currentPreview, prefab);
        }

        if (OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger) > 0.5) SaveObjects();
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) RemoveAllObjects();
        if (OVRInput.GetDown(OVRInput.Button.One)) LoadObjects();
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
        int index = getNextIndex();

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
    }

    private void placeATarget(RaycastHit hit, GameObject previewPrefab, GameObject prefab) {
        triggerPressed = true;
        установленныеМишени.Add(
            Instantiate(prefab, hit.point, previewPrefab.transform.rotation)
        );
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

    //todo удалить после отладки
    private Color color1 = Color.red;
    private Color color2 = Color.blue;
    
    private bool isColor1Active = true;
    
    //todo удалить позже
    private void tempColorChange() {
        Renderer renderer = cube2Temp.GetComponent<Renderer>();
        if (renderer != null) {
            renderer.material.color = isColor1Active ? color1 : color2;
            isColor1Active = !isColor1Active;
        } else {
            Debug.LogWarning("У объекта нет компонента Renderer!");
        }
    }
}