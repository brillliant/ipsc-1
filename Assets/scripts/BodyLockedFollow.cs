using UnityEngine;

public class BodyLockedFollow : MonoBehaviour {
    [Header("Refs")]
    [SerializeField] private Transform head;           // CenterEyeAnchor

    [Header("Spawn position (relative to head)")]
    [SerializeField] private float distance       = 0.45f;
    [SerializeField] private float rightOffset    = 0.6f;
    [SerializeField] private float verticalOffset = -0.2f;   // ниже шлема, метры
    [SerializeField] private float rotationOffset = -52f;

    [Header("Follow")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float deadZone    = 0.25f;

    private Vector3 _worldOffset;
    private Vector3 _targetPos;
    private bool    _ready;

    void OnEnable() {
        if (head == null && Camera.main != null) head = Camera.main.transform;
        SnapToSpawnPosition();
    }

    void LateUpdate() {
        if (!_ready || head == null) return;

        Vector3 desired = head.position + _worldOffset;
        // Y тоже из _worldOffset — а в нём зашита разница (head.y - 0.2),
        // поэтому при наклоне/приседании меню следует за головой по высоте автоматически.

        if (Vector3.Distance(_targetPos, desired) > deadZone) {
            _targetPos = desired;
        }
        transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime * followSpeed);
    }

    public void SnapToSpawnPosition() {
        if (head == null && Camera.main != null) head = Camera.main.transform;
        if (head == null) return;

        Vector3 spawnPos = head.position + head.forward * distance;
        spawnPos += head.right * rightOffset;
        spawnPos.y = head.position.y + verticalOffset;     // ← высота от головы

        Vector3 lookDir = head.position - spawnPos;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude < 0.0001f) return;

        Quaternion spawnRot = Quaternion.LookRotation(-lookDir.normalized);
        spawnRot *= Quaternion.Euler(0, rotationOffset, 0);

        _worldOffset = spawnPos - head.position;
        _targetPos   = spawnPos;
        _ready       = true;

        transform.SetPositionAndRotation(spawnPos, spawnRot);
    }
}