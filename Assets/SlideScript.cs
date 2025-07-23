using UnityEngine;

public class SlideScript : MonoBehaviour {
    private Vector3 localPosition0;
    private Vector3 localRotation0;
    
    float maxOffset = 0.009f;

    private void Awake() {
        localPosition0 = transform.localPosition;
        localRotation0 = transform.localEulerAngles;
    }
	
    void Update () {
        Vector3 currentLocal = transform.localPosition;
        float dz = currentLocal.z - localPosition0.z;
        dz = Mathf.Clamp(dz, 0f, maxOffset);
        
        transform.localPosition = new Vector3(localPosition0.x, localPosition0.y, localPosition0.z + dz);
        transform.localEulerAngles = localRotation0;
    }
}
