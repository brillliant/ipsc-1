using Meta.XR.EnvironmentDepth;
using UnityEngine;

public class EnvironmentDepthGuard : MonoBehaviour {
    void Awake() {
#if UNITY_EDITOR
        GetComponent<EnvironmentDepthManager>().enabled = false;
#endif
    }
}
