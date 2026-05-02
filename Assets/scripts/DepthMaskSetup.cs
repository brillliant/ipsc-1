using System.Collections;
using System.Collections.Generic;
using Meta.XR.EnvironmentDepth;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.XR;

[DefaultExecutionOrder(-1000)]
public class DepthMaskSetup : MonoBehaviour {
    [SerializeField] private float _maxOcclusionDistance = 0.7f;
    [SerializeField] private Transform _excludeBox;
    [SerializeField] private float _depthMargin = 0.15f;

    void Awake() {
#if UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }
    
    void Update() {
        Shader.SetGlobalFloat("_MaxOcclusionDistance", _maxOcclusionDistance);
        if (_excludeBox == null) return;

        Shader.SetGlobalMatrix("_ExcludeBoxWorldToLocal",
            _excludeBox.worldToLocalMatrix);

        Camera cam = Camera.main;
        if (cam != null) {
            Vector3 c = _excludeBox.position;
            Vector3 r = _excludeBox.right * _excludeBox.lossyScale.x * 0.5f;
            Vector3 u = _excludeBox.up * _excludeBox.lossyScale.y * 0.5f;
            Vector3 f = _excludeBox.forward * _excludeBox.lossyScale.z * 0.5f;
            Vector3 camPos = cam.transform.position;

            float minD = float.MaxValue;
            Vector3[] corners = {
                c - r - u - f, c + r - u - f,
                c - r + u - f, c + r + u - f,
                c - r - u + f, c + r - u + f,
                c - r + u + f, c + r + u + f
            };
            for (int i = 0; i < 8; i++) {
                float d = Vector3.Distance(camPos, corners[i]);
                if (d < minD) minD = d;
            }
            Shader.SetGlobalFloat("_ExcludeMinDepth", minD - _depthMargin);
        }
    }
}