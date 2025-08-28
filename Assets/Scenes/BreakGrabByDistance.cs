using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

/**
 * коллайдер грэпа - болше чем визуальные коллайдеры пальцев. так что если рука визуально не касается предмета, грэп все еще касается.
 * это можно убрать, уменьшив коллайдер грэпа, но тогда хватать будет не так хорошо. не буду трогать
 */
public class BreakGrabByDistance : MonoBehaviour {
    private float releaseDistance = 0.00005f;   // бесконечное уменьшение не помогает.  лдолжно быть больше чем hysteresis
    private float confirmTime     = 0.10f;   // держать разрыв ≥100 мс
    private float hysteresis      = 0.00001f;   // 0,1 см возврат

    [Header("Коллайдер слайдера")]
    [SerializeField] Collider slideCollider;

    HandGrabInteractable handGrab;
    GrabInteractable grab;

    Transform currentHandRoot;
    readonly List<Collider> handCols = new List<Collider>(32);
    float sepTimer;
    bool inCooldown;

    void Awake() {
        handGrab = GetComponentInChildren<HandGrabInteractable>(true);
        grab     = GetComponentInChildren<GrabInteractable>(true);
        if (slideCollider == null)
            slideCollider = GetComponentInChildren<Collider>(true);
    }

    void LateUpdate() {
        // никем не выбран — выходим
        bool noHandGrab = (handGrab == null) || handGrab.SelectingInteractors.Count == 0;
        bool noGrab     = (grab     == null) || grab.SelectingInteractors.Count == 0;
        if (noHandGrab && noGrab) { 
            currentHandRoot = null; 
            sepTimer = 0f; 
            return; 
        }
        if (inCooldown) return;

        var handRoot = GetCurrentHandRoot();
        if (handRoot != currentHandRoot) {
            currentHandRoot = handRoot;
            RebuildHandColliders();
            sepTimer = 0f;
        }
        if (!currentHandRoot) { sepTimer = 0f; return; }

        // 1) пока есть контакт пальцев со слайдером — не отпускать
        float minDist;
        bool touching = HandTouchingSlider(out minDist);
        if (touching) { sepTimer = 0f; return; }

        // 2) после разрыва контакта — требуем стабильный уход дальше порога
        if (minDist > releaseDistance) {
            sepTimer += Time.deltaTime;
            if (sepTimer >= confirmTime) StartCoroutine(ForceDrop());
        } else if (minDist < releaseDistance - hysteresis) {
            sepTimer = 0f;
        }
    }

    Transform GetCurrentHandRoot() {
        if (handGrab) {
            foreach (var it in handGrab.SelectingInteractors) {
                var mb = it as MonoBehaviour; if (mb) return mb.transform;
            }
        }
        if (grab) {
            foreach (var gi in grab.SelectingInteractors) return gi.transform;
        }
        return null;
    }

    void RebuildHandColliders() {
        handCols.Clear();
        if (!currentHandRoot) return;
        currentHandRoot.GetComponentsInChildren(true, handCols);
        // оставить только включённые коллайдеры
        for (int i = handCols.Count - 1; i >= 0; --i) {
            if (!handCols[i] || !handCols[i].enabled) handCols.RemoveAt(i);
        }
    }

    bool HandTouchingSlider(out float minDistance) {
        minDistance = float.MaxValue;
        if (handCols.Count == 0) return false;

        foreach (var hc in handCols) {
            if (!hc) continue;
            
            if (!slideCollider || !slideCollider.enabled) continue;

            // точная проверка пересечения
            bool overlap = Physics.ComputePenetration(
                hc, hc.transform.position, hc.transform.rotation,
                slideCollider, slideCollider.transform.position, slideCollider.transform.rotation,
                out _, out float dist
            );
            if (overlap) { minDistance = 0f; return true; }

            // минимальная дистанция между парами
            Vector3 p1 = hc.ClosestPoint(slideCollider.bounds.center);
            Vector3 p2 = slideCollider.ClosestPoint(hc.bounds.center);
            float d = Vector3.Distance(p1, p2);
            if (d < minDistance) minDistance = d;
        }
        return false;
    }

    IEnumerator ForceDrop() {
        inCooldown = true;
        sepTimer = 0f;

        if (handGrab) handGrab.enabled = false;
        if (grab)     grab.enabled     = false;

        yield return null;                 // отпустить хват
        yield return new WaitForSeconds(0.15f); // анти-ре-граб

        if (grab)     grab.enabled     = true;
        if (handGrab) handGrab.enabled = true;

        inCooldown = false;
    }
}
