using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class FloorScript : MonoBehaviour {
    [SerializeField] private float halfSizeMeters = 5f;
    [SerializeField] private bool addCollider = true;

    [Header("Highlight like EffectMesh")]
    [SerializeField] public bool highlight = true;                // вкл/выкл в рантайме
    [SerializeField] private Material effectMaterial;              // присвой сюда RoomBoxEffects из MRUK

    private GameObject megaFloor;
    private MeshRenderer meshRenderer;
    private Material runtimeMat;

    void Start() {
        MRUKRoom room = MRUK.Instance?.GetCurrentRoom();
        if (room == null) { CreateAt(Vector3.zero, Vector3.up); return; }

        MRUKAnchor floor = room.GetFloorAnchor();
        if (floor == null) { CreateAt(room.transform.position, room.transform.up); return; }

        Transform pose = floor.transform;
        CreateAt(pose.position, pose.up, pose.rotation);
    }

    void CreateAt(Vector3 center, Vector3 normal, Quaternion? exactRot = null) {
        megaFloor = GameObject.CreatePrimitive(PrimitiveType.Plane);     // 10×10
        megaFloor.name = "MegaFloor";
        megaFloor.transform.position = center + normal.normalized * 0.203f;;
        megaFloor.transform.rotation = exactRot ?? Quaternion.FromToRotation(Vector3.up, normal);

        float scale = (halfSizeMeters * 2f) / 10f;                        // перевод в масштаб
        megaFloor.transform.localScale = new Vector3(scale, 1f, scale);

        meshRenderer = megaFloor.GetComponent<MeshRenderer>();
        if (meshRenderer != null) {
            // инстанс, чтобы не править оригинальный материал в проекте
            runtimeMat = effectMaterial != null ? new Material(effectMaterial) : null;
            if (runtimeMat != null) meshRenderer.material = runtimeMat;
        }

        Collider col = megaFloor.GetComponent<Collider>();
        if (addCollider) {
            if (col == null) megaFloor.AddComponent<MeshCollider>();
        } else {
            if (col != null) Destroy(col);
        }

        ApplyHighlight();
    }

    void Update() => ApplyHighlight();

    private void ApplyHighlight() {
        if (meshRenderer == null) return;
        meshRenderer.enabled = highlight && runtimeMat != null;
    }
}