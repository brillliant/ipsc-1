using TMPro;
using UnityEngine;

// Пистолет всегда на РОДНОЙ позе контроллера (не прыгает на руку).
//
// Детектор дрейфа: положение руки относительно контроллера в СИСТЕМЕ КООРДИНАТ КОНТРОЛЛЕРА.
// При жёстком хвате это 3D-вектор ПОСТОЯННЫЙ — не зависит ни от перемещения, ни от поворота
// тела/прицеливания (поворот сокращается во всех трёх осях сразу). dev = векторное расстояние
// текущего смещения от эталонного. Растёт ТОЛЬКО когда контроллер реально уходит относительно
// руки (дрейф при перекрытии диодов). Мировые координаты тут нельзя — там вектор поворачивается.
//
// На Quest Pro не нужен — держи выключенным.
public class HandednessService : MonoBehaviour {
    [SerializeField] private OVRCameraRig cameraRig;
    [SerializeField] private bool isLeftHandedPlayer;  // true: пистолет в левом контроллере
    [SerializeField] private bool showDebug = true;
    [SerializeField] private TMP_Text debugLabel;

    [Header("Tuning")]
    [SerializeField] private float maxDeviation = 0.01f;    // порог (м): выше — TRIG
    [SerializeField] private float calibSeconds = 3.0f;     // сколько секунд калибруем эталон
    [SerializeField] private float smoothingSeconds = 0.3f; // лёгкое сглаживание против джиттера руки

    private Vector3 restingOffsetLocal;   // эталонное положение руки в системе координат контроллера
    private Vector3 filteredOffsetLocal;
    private bool calibrated;
    private float startTime = -1f;

    private float dbgDev;

    void Start() {
        cameraRig.UpdatedAnchors += stabilizePistolPose;
    }

    private void OnDestroy() {
        if (cameraRig != null) cameraRig.UpdatedAnchors -= stabilizePistolPose;
    }

    // перезапустить калибровку эталона (вызывается при открытии меню)
    public void Recalibrate() {
        calibrated = false;
        startTime = -1f;
    }

    private void stabilizePistolPose(OVRCameraRig rig) {
        OVRInput.Controller controller = isLeftHandedPlayer ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        OVRInput.Controller hand = isLeftHandedPlayer ? OVRInput.Controller.LHand : OVRInput.Controller.RHand;
        Transform anchor = isLeftHandedPlayer ? rig.leftHandAnchor : rig.rightHandAnchor;

        Vector3 rawPos = OVRInput.GetLocalControllerPosition(controller);
        Quaternion rawRot = OVRInput.GetLocalControllerRotation(controller);

        bool handTracked = OVRInput.GetControllerPositionTracked(hand);
        Vector3 handPos = OVRInput.GetLocalControllerPosition(hand);

        if (startTime < 0f) startTime = Time.time;

        if (handTracked) {
            // рука в системе координат контроллера — инвариантно к повороту (жёсткий хват → константа)
            Vector3 offsetLocal = Quaternion.Inverse(rawRot) * (handPos - rawPos);

            if (!calibrated) {
                restingOffsetLocal = offsetLocal;
                filteredOffsetLocal = offsetLocal;
                dbgDev = 0f;
                if (Time.time - startTime >= calibSeconds) calibrated = true;
            } else {
                float k = smoothingSeconds > 0f ? 1f - Mathf.Exp(-Time.deltaTime / smoothingSeconds) : 1f;
                filteredOffsetLocal = Vector3.Lerp(filteredOffsetLocal, offsetLocal, k);
                dbgDev = (filteredOffsetLocal - restingOffsetLocal).magnitude;   // 3D векторное расстояние
            }
        } else {
            dbgDev = 0f;
        }

        // --- пистолет ВСЕГДА на родной позе контроллера ---
        anchor.localPosition = rawPos;
        anchor.localRotation = rawRot;

        if (showDebug && debugLabel != null) {
            bool trig = calibrated && dbgDev > maxDeviation;
            debugLabel.text = (trig ? ">>> TRIG <<<" : (calibrated ? "OK" : "CALIB"))
                + "\ndev: " + (dbgDev * 100f).ToString("0.0") + "cm"
                + "\nhand: " + (handTracked ? "yes" : "NO");
        }
    }
}
