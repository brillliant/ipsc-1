using System.Collections;
using System.Collections.Generic;
using Meta.XR.EnvironmentDepth;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class DepthMaskSetup : MonoBehaviour {
    [SerializeField] private float _maxOcclusionDistance = 0.8f;

    void Start() {
        Shader.SetGlobalFloat("_MaxOcclusionDistance", _maxOcclusionDistance);
    }
}