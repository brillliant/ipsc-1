using UnityEngine;

public class BodyLockedFollow : MonoBehaviour {
    [Header("Refs")]
    [SerializeField] private Transform head;

    [Header("Spawn position")]
    [SerializeField] private float distance       = 0.45f;
    [SerializeField] private float rightOffset    = 0.6f;
    [SerializeField] private float verticalOffset = -0.6f;
    [SerializeField] private float rotationOffset = -52f;

    [Header("Dead zones")]
    [SerializeField] private float xzDeadZone      = 0.9f;
    [SerializeField] private float verticalDeadZone = 0.3f;
    [SerializeField] private float settleDelay      = 0.6f;

    private Vector3 _lastDesired;
    private float   _outsideZoneTime = -1f;
    private bool    _ready;

    void Awake() {
        if (head == null && Camera.main != null) head = Camera.main.transform;
    }

    void LateUpdate() {
        if (!_ready || head == null) return;

        Vector3 bodyForward = new Vector3(head.forward.x, 0, head.forward.z).normalized;
        Vector3 desired     = head.position + bodyForward * distance + head.right * rightOffset;
        desired.y           = head.position.y + verticalOffset;

        Vector3 menuPos   = transform.position;
        bool xzOutside    = Vector2.Distance(new Vector2(menuPos.x, menuPos.z), new Vector2(desired.x, desired.z)) > xzDeadZone;
        bool yOutside     = Mathf.Abs(desired.y - menuPos.y) > verticalDeadZone;
        bool desiredStable = Vector3.Distance(desired, _lastDesired) < 0.05f;

        if ((xzOutside || yOutside) && desiredStable) {
            if (_outsideZoneTime < 0f) _outsideZoneTime = Time.time;
            else if (Time.time - _outsideZoneTime >= settleDelay) {
                SnapToSpawnPosition();
                _outsideZoneTime = -1f;
            }
        } else {
            _outsideZoneTime = -1f;
        }

        _lastDesired = desired;
    }

    public void SnapToSpawnPosition() {
        if (head == null && Camera.main != null) head = Camera.main.transform;
        if (head == null) return;

        Vector3 bodyForward = new Vector3(head.forward.x, 0, head.forward.z).normalized;
        Vector3 spawnPos    = head.position + bodyForward * distance + head.right * rightOffset;
        spawnPos.y          = head.position.y + verticalOffset;

        Vector3 lookDir = head.position - spawnPos;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude < 0.0001f) return;

        _ready = true;

        transform.SetPositionAndRotation(
            spawnPos,
            Quaternion.LookRotation(-lookDir.normalized) * Quaternion.Euler(0, rotationOffset, 0)
        );
    }
}
