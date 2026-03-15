using System.Collections;
using System.Collections.Generic;
using Meta.XR.EnvironmentDepth;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class DepthMaskSetup : MonoBehaviour
{
    [SerializeField] private EnvironmentDepthManager _depthManager;

    IEnumerator Start() {
        while (MRUK.Instance == null || !MRUK.Instance.IsInitialized)
            yield return null;

        yield return new WaitForSeconds(2f);

        var room = MRUK.Instance.GetCurrentRoom();
        if (room == null) {
            Debug.LogWarning("[DepthMask] Room not found");
            yield break;
        }

        // ищем все MeshFilter, созданные EffectMesh
        var allFilters = FindObjectsOfType<MeshFilter>();
        var maskFilters = new List<MeshFilter>();

        foreach (var filter in allFilters) {
            if (filter.sharedMesh != null) {
                // проверяем, принадлежит ли объект Room или EffectMesh
                var anchor = filter.GetComponentInParent<MRUKAnchor>();
                if (anchor != null) {
                    maskFilters.Add(filter);
                    Debug.Log($"[DepthMask] Added: {filter.gameObject.name}");
                }
            }
        }

        if (maskFilters.Count > 0) {
            _depthManager.MaskMeshFilters = maskFilters;
            Debug.Log($"[DepthMask] Done! {maskFilters.Count} filters added");
        } else {
            Debug.LogWarning("[DepthMask] No MeshFilters found");
        }
    }
}
