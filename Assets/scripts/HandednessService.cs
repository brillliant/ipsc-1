using UnityEngine;

// Якорь стороны пистолета всегда следует за КОНТРОЛЛЕРОМ, даже когда система
// (перекрытые датчики в держателе) переключает эту сторону на руку.
public class HandednessService : MonoBehaviour {
    [SerializeField] private OVRCameraRig cameraRig;
    [SerializeField] private bool isLeftHandedPlayer; // true: пистолет в левом контроллере

    private Vector3 lastControllerPosition;
    private Quaternion lastControllerRotation = Quaternion.identity;

    void Start() {
        cameraRig.UpdatedAnchors += forceControllerPoseOnPistolSide;
    }

    private void forceControllerPoseOnPistolSide(OVRCameraRig rig) {
        OVRInput.Controller controller = isLeftHandedPlayer ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        Transform anchor = isLeftHandedPlayer ? rig.leftHandAnchor : rig.rightHandAnchor;

        if (OVRInput.GetControllerPositionValid(controller)) {
            lastControllerPosition = OVRInput.GetLocalControllerPosition(controller);
            lastControllerRotation = OVRInput.GetLocalControllerRotation(controller);
        }
        // трекинг потерян — держим последнюю позу контроллера, на руку не прыгаем
        anchor.localPosition = lastControllerPosition;
        anchor.localRotation = lastControllerRotation;
    }
}