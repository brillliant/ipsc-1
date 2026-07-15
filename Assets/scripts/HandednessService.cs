using TMPro;
using UnityEngine;

// Якорь стороны пистолета всегда следует за КОНТРОЛЛЕРОМ — запрещает системе
// перекидывать пистолет на руку, когда датчики перекрыты держателем/ладонями.
// Пока рантайм доверяет контроллеру — позиция живая; потерял доверие — держим
// последнюю позицию. Вращение всегда по гироскопу контроллера.
public class HandednessService : MonoBehaviour {
    [SerializeField] private OVRCameraRig cameraRig;
    [SerializeField] private bool isLeftHandedPlayer; // true: пистолет в левом контроллере
    [SerializeField] private TMP_Text debugLabel;     // отдельный текст, который игра не прячет

    private Vector3 lastPosition;
    private Quaternion lastRotation = Quaternion.identity;

    void Start() {
        cameraRig.UpdatedAnchors += forceControllerPoseOnPistolSide;
    }

    private void forceControllerPoseOnPistolSide(OVRCameraRig rig) {
        OVRInput.Controller controller = isLeftHandedPlayer ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        Transform anchor = isLeftHandedPlayer ? rig.leftHandAnchor : rig.rightHandAnchor;

        OVRInput.Controller active = OVRInput.GetActiveControllerForHand(
            isLeftHandedPlayer ? OVRInput.Handedness.LeftHanded : OVRInput.Handedness.RightHanded);
        bool controllerTrusted = active == controller && OVRInput.GetControllerPositionTracked(controller);

        if (controllerTrusted) {
            lastPosition = OVRInput.GetLocalControllerPosition(controller);
        }

        // вращение — гироскоп работает и при перекрытых датчиках
        if (OVRInput.GetControllerOrientationValid(controller)) {
            lastRotation = OVRInput.GetLocalControllerRotation(controller);
        }
        anchor.localPosition = lastPosition;
        anchor.localRotation = lastRotation;

        /*if (debugLabel != null) {
            debugLabel.text = "src=" + (controllerTrusted ? "controller" : "frozen") + "\nactive=" + active;
        }*/
    }
}
