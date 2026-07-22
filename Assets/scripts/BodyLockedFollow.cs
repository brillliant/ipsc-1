using UnityEngine;

// Одноразовое позиционирование меню перед игроком при открытии.
// Постоянного слежения за телом больше нет — меню стоит там, где его поставили
// или куда перетащили, и не реагирует на повороты и перемещения игрока.
public class BodyLockedFollow : MonoBehaviour {
    [Header("Refs")]
    [SerializeField] private Transform head;

    [Header("Spawn position")]
    [SerializeField] private float distance       = 0.45f;
    [SerializeField] private float rightOffset    = 0.6f;
    [SerializeField] private float verticalOffset = -0.6f;
    [SerializeField] private float rotationOffset = -52f;

    void Awake() {
        if (head == null && Camera.main != null) head = Camera.main.transform;
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

        transform.SetPositionAndRotation(
            spawnPos,
            Quaternion.LookRotation(-lookDir.normalized) * Quaternion.Euler(0, rotationOffset, 0)
        );
    }
}
