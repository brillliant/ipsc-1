using UnityEngine;

public class SlideScript : MonoBehaviour {
    private Vector3 localPosition0;	//original local position
    private Vector3 localRotation0; // оригинальное локальное вращение
    
    float maxOffset = 0.009f;

    void Start() {
        setOriginalLocalPositionAndRotation();
    }
	
    void Update () {
        Vector3 currentLocal = transform.localPosition;
        float dz = currentLocal.z - localPosition0.z;
        dz = Mathf.Clamp(dz, 0f, maxOffset); // только вперёд
        
        transform.localPosition = new Vector3(localPosition0.x, localPosition0.y, localPosition0.z + dz);
        transform.localEulerAngles = localRotation0;
    }

    private void setOriginalLocalPositionAndRotation() {
        localPosition0 = transform.localPosition;
        localRotation0 = transform.localEulerAngles;
    }
}
