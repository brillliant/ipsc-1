using TMPro;
using UnityEngine;

// Якорь стороны пистолета всегда следует за КОНТРОЛЛЕРОМ — запрещает системе
// перекидывать пистолет на руку, когда датчики перекрыты держателем/ладонями.
// Каскад источников позиции:
//   1) оптика контроллера жива — берём её (и непрерывно запоминаем офсет до ладони);
//   2) оптика упала — первые ImuTrustSeconds доверяем системной позе (IMU dead reckoning);
//   3) дальше едем за ладонью: позиция ладони + заранее запомненный офсет (multimodal);
//   4) источников нет — позиция не обновляется. Вращение всегда по гироскопу контроллера.
public class HandednessService : MonoBehaviour {
    [SerializeField] private OVRCameraRig cameraRig;
    [SerializeField] private bool isLeftHandedPlayer; // true: пистолет в левом контроллере
    [SerializeField] private TMP_Text debugLabel;     // отдельный текст, который игра не прячет

    private const float ImuTrustSeconds = 1.5f;    // сколько секунд после потери оптики доверяем системной IMU-позе
    private const float RecoverLerpSeconds = 0f;   // сглаживание возврата к оптике; 0 — мгновенный переход

    private Vector3 lastPosition;
    private Quaternion lastRotation = Quaternion.identity;

    private Vector3 gripOffset;          // позиция пистолета относительно ладони, обновляется пока есть оптика/IMU
    private bool gripOffsetValid;
    private float opticalLostAt = -1f;   // момент потери оптики; -1 — оптика жива
    private float recoveredAt = -1f;     // момент возврата оптики (для лерпа)
    private Vector3 recoverFrom;

    void Start() {
        cameraRig.UpdatedAnchors += forceControllerPoseOnPistolSide;
    }

    private void forceControllerPoseOnPistolSide(OVRCameraRig rig) {
        OVRInput.Controller controller = isLeftHandedPlayer ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        OVRInput.Controller hand = isLeftHandedPlayer ? OVRInput.Controller.LHand : OVRInput.Controller.RHand;
        Transform anchor = isLeftHandedPlayer ? rig.leftHandAnchor : rig.rightHandAnchor;

        bool optical = OVRInput.GetControllerPositionTracked(controller);
        bool handTracked = OVRInput.GetControllerPositionTracked(hand);
        Vector3 handPos = OVRInput.GetLocalControllerPosition(hand);

        if (optical) {
            if (opticalLostAt >= 0f) {           // оптика только что вернулась
                recoveredAt = Time.time;
                recoverFrom = lastPosition;
                opticalLostAt = -1f;
            }

            Vector3 pos = OVRInput.GetLocalControllerPosition(controller);

            if (RecoverLerpSeconds > 0f && recoveredAt >= 0f) {
                float t = (Time.time - recoveredAt) / RecoverLerpSeconds;
                if (t < 1f) pos = Vector3.Lerp(recoverFrom, pos, t);
                else recoveredAt = -1f;
            }

            lastPosition = pos;
            rememberGripOffset(handTracked, handPos);
        } else {
            if (opticalLostAt < 0f) opticalLostAt = Time.time;

            bool imuFresh = Time.time - opticalLostAt <= ImuTrustSeconds
                            && OVRInput.GetControllerPositionValid(controller);
            if (imuFresh) {
                lastPosition = OVRInput.GetLocalControllerPosition(controller);
                rememberGripOffset(handTracked, handPos);   // IMU ещё жив — офсет продолжаем уточнять
            } else if (handTracked && gripOffsetValid) {
                lastPosition = handPos + gripOffset;        // едем за ладонью
            }
            // источников нет — lastPosition остаётся с прошлого кадра
        }

        // вращение — гироскоп работает и при перекрытых датчиках
        if (OVRInput.GetControllerOrientationValid(controller)) {
            lastRotation = OVRInput.GetLocalControllerRotation(controller);
        }

        anchor.localPosition = lastPosition;
        anchor.localRotation = lastRotation;

        /*if (debugLabel != null) {
            debugLabel.text = "optical=" + optical + "\nhand=" + handTracked
                + "\nsrc=" + (optical ? "optical" : Time.time - opticalLostAt <= ImuTrustSeconds ? "imu" : "hand");
        }*/
    }

    private void rememberGripOffset(bool handTracked, Vector3 handPos) {
        if (!handTracked) return;
        gripOffset = lastPosition - handPos;
        gripOffsetValid = true;
    }
}
