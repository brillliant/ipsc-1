using UnityEngine;

// Якорь стороны пистолета всегда следует за КОНТРОЛЛЕРОМ и только за ним —
// запрещает системе перекидывать пистолет на руку, когда датчики перекрыты
// держателем/ладонями. Никакого фолбэка на hand tracking: даже если контроллер
// перекрыт и замер, берём его системную позу как есть (рантайм продолжает вести
// её по IMU/фьюжну). На скелет руки не переключаемся ни при каких условиях.
public class HandednessService : MonoBehaviour {
    [SerializeField] private OVRCameraRig cameraRig;
    [SerializeField] private bool isLeftHandedPlayer; // true: пистолет в левом контроллере

    void Start() {
        cameraRig.UpdatedAnchors += forceControllerPoseOnPistolSide;
    }

    private void forceControllerPoseOnPistolSide(OVRCameraRig rig) {
        OVRInput.Controller controller = isLeftHandedPlayer ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        Transform anchor = isLeftHandedPlayer ? rig.leftHandAnchor : rig.rightHandAnchor;

        anchor.localPosition = OVRInput.GetLocalControllerPosition(controller);
        anchor.localRotation = OVRInput.GetLocalControllerRotation(controller);
    }
}
