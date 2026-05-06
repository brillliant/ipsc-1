using UnityEngine;
using UnityEngine.EventSystems;

public class RayColorController : MonoBehaviour {
    private LineRenderer rayLine;
    public GameObject camera;

    private Color _modeColor = Color.white;          // что должно быть «по умолчанию» в текущем режиме
    private bool  _hoveringUI;

    void Start() {
        var rayInteractorObj = camera.transform.Find(
            "[BuildingBlock] Interaction/[BuildingBlock] Controller Interactions/" +
            "LeftController/ControllerInteractors/ControllerRayInteractor"
        );
        
        if (rayInteractorObj != null) {
            rayLine = rayInteractorObj.GetComponentInChildren<LineRenderer>(true);
        }
    }
    
    void Update() {
        bool overUI = EventSystem.current != null && 
                      EventSystem.current.IsPointerOverGameObject();

        // переключаем только при изменении состояния, чтобы не дёргать каждый кадр
        if (overUI != _hoveringUI) {
            _hoveringUI = overUI;
            Apply(overUI ? Color.white : _modeColor);
        }
    }

    private void Apply(Color c) {
        rayLine.startColor = c;
        Color end = c; end.a = 0f;
        rayLine.endColor = end;
        if (rayLine.material != null) rayLine.material.color = c;
    }

    /// <summary>Вызывай при смене mode-цвета извне, если луч сейчас не над UI.</summary>
    public void RefreshIfNeeded() {
        if (!_hoveringUI) Apply(_modeColor);
    }
}
