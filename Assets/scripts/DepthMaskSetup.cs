using System.Collections;
using System.Collections.Generic;
using Meta.XR.EnvironmentDepth;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class DepthMaskSetup : MonoBehaviour {
    [SerializeField] private float _maxOcclusionDistance = 0.01f; //see unity
    
    [SerializeField] private Transform _excludeBox;
    
    //[SerializeField] private Transform _controllerTransform;
    //[SerializeField] private Vector3 _boxMin = new Vector3(-0.03f, 0.02f, -0.15f);
    //[SerializeField] private Vector3 _boxMax = new Vector3(0.03f, 0.08f, 0.15f);

    void Start() {
        Shader.SetGlobalFloat("_MaxOcclusionDistance", _maxOcclusionDistance);
    }
    
    void Update() {
        if (_excludeBox != null) {
            Shader.SetGlobalMatrix("_ControllerWorldToLocal", 
                _excludeBox.worldToLocalMatrix);
            Shader.SetGlobalVector("_ExcludeBoxMin", new Vector3(-0.5f, -0.5f, -0.5f));
            Shader.SetGlobalVector("_ExcludeBoxMax", new Vector3(0.5f, 0.5f, 0.5f));
        }
    }
}