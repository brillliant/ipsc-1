using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class FloorScript : MonoBehaviour {
    [SerializeField] private float halfSizeMeters = 5f; // расстояние в каждую сторону
    [SerializeField] private bool addCollider = true;
    [SerializeField] private bool highlight = true;     // включать ли подсветку
    [SerializeField] private Color highlightColor = Color.magenta; // цвет подсветки

    private GameObject megaFloor;
    private MeshRenderer renderer;
    private Material mat;

    void Start() {
        MRUKRoom room = MRUK.Instance?.GetCurrentRoom();
        if (room == null) { 
            CreateAt(Vector3.zero, Vector3.up); 
            return; 
        }

        MRUKAnchor floor = room.GetFloorAnchor(); 
        if (floor == null) { 
            CreateAt(room.transform.position, room.transform.up); 
            return; 
        }

        Transform pose = floor.transform; 
        CreateAt(pose.position, pose.up, pose.rotation);
    }

    void CreateAt(Vector3 center, Vector3 normal, Quaternion? exactRot = null) {
        megaFloor = GameObject.CreatePrimitive(PrimitiveType.Plane); 
        megaFloor.name = "MegaFloor";
        megaFloor.transform.position = center;
        megaFloor.transform.rotation = exactRot ?? Quaternion.FromToRotation(Vector3.up, normal);

        float scale = (halfSizeMeters * 2f) / 10f;
        megaFloor.transform.localScale = new Vector3(scale, 1f, scale);

        renderer = megaFloor.GetComponent<MeshRenderer>();
        if (renderer != null) {
            mat = new Material(Shader.Find("Standard"));
            renderer.material = mat;
        }

        Collider col = megaFloor.GetComponent<Collider>();
        if (addCollider) {
            if (col == null) megaFloor.AddComponent<MeshCollider>();
        } else {
            if (col != null) Destroy(col);
        }
    }

    void Update() {
        if (renderer == null || mat == null) return;

        if (highlight) {
            mat.color = highlightColor; // включена подсветка
            renderer.enabled = true;
        } else {
            renderer.enabled = false;   // подсветка выключена
        }
    }
}