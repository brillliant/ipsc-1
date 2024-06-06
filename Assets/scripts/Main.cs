using System.Collections;
using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using UnityEngine;

public class Main : MonoBehaviour {
    public GameObject prefab;
    public GameObject previewPrefab;
    private GameObject currentPreview;

    public List<ObjectData> objectDataList = new List<ObjectData>();
    public List<GameObject> objectsToSave = new List<GameObject>();
    
    void Start() {
        currentPreview = Instantiate(previewPrefab);
    }

    void Update() {
        Ray ray = new Ray(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch),
            OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward);

        if (Physics.Raycast(ray, out RaycastHit hit)) {
            currentPreview.transform.position = hit.point;
            currentPreview.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // Поворот модели лицом к камере
            Vector3 cameraPosition = Camera.main.transform.position;
            currentPreview.transform.LookAt(new Vector3(cameraPosition.x, currentPreview.transform.position.y,
                cameraPosition.z));

            if (OVRInput.GetDown(OVRInput.Button.One)) {
                Instantiate(prefab, hit.point, currentPreview.transform.rotation);
                objectsToSave.Add(
                Instantiate(prefab, hit.point, currentPreview.transform.rotation)
                );
            }
        }
    }

    public void SaveObjects() {
        objectDataList.Clear();

        // Добавьте ваши объекты в список
        foreach (GameObject obj in objectsToSave) {
            ObjectData data = new ObjectData(obj.name, obj.transform.position, obj.transform.rotation);
            objectDataList.Add(data);
        }

        // Оберните список в объект-обертку
        ObjectDataList wrapper = new ObjectDataList { objectDataList = objectDataList };

        // Сериализуйте объект-обертку
        string json = JsonUtility.ToJson(wrapper);
        File.WriteAllText(Application.persistentDataPath + "/saveData.json", json);
    }

    public void LoadObjects() {
        string path = Application.persistentDataPath + "/saveData.json";
        if (File.Exists(path)) {
            string json = File.ReadAllText(path);

            // Десериализуйте JSON в объект-обертку
            ObjectDataList wrapper = JsonUtility.FromJson<ObjectDataList>(json);

            // Извлеките список из объекта-обертки
            objectDataList = wrapper.objectDataList;

            // Восстановите объекты
            foreach (ObjectData data in objectDataList) {
                GameObject prefab = Resources.Load<GameObject>(data.prefabName);
                if (prefab != null) {
                    Instantiate(prefab, data.position, data.rotation);
                } else {
                    Debug.LogWarning("Prefab not found: " + data.prefabName);
                }
            }
        }
    }
}