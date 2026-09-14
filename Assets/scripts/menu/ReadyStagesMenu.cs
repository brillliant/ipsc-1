using System.Collections;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.UI;

public class ReadyStagesMenu : MonoBehaviour {
    private BuilderService builderService;

    private void OnEnable() {
        if (builderService == null)
            builderService = FindFirstObjectByType<BuilderService>();
        builderService.PopulateReadyStagesMenu();
        StartCoroutine(fitInteractionSurfacesNextFrame());
    }

    // Сетка кнопок (GridLayoutGroup + ContentSizeFitter) растёт вниз по мере добавления стейджей,
    // а зоны луча и тыка из префаба Meta остаются размером с исходное окно 3x3 — кнопки ниже не кликаются.
    // Подгоняем RectTransform зон под сетку. Ждём кадр: PopulateReadyStagesMenu удаляет старые кнопки
    // через Destroy, и в этом же кадре сетка ещё считает их своими.
    private IEnumerator fitInteractionSurfacesNextFrame() {
        yield return null;

        GridLayoutGroup grid = GetComponentInChildren<GridLayoutGroup>(true);
        if (grid == null) yield break;
        RectTransform gridRt = grid.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridRt);

        foreach (RayInteractable ray in GetComponentsInChildren<RayInteractable>(true))
            fitTo(ray.GetComponent<RectTransform>(), gridRt);
        foreach (PokeInteractable poke in GetComponentsInChildren<PokeInteractable>(true))
            fitTo(poke.GetComponent<RectTransform>(), gridRt);
    }

    // копируем прямоугольник сетки; RectTransformBoundsClipperDriver на дочернем Surface сам обновит BoundsClipper
    private static void fitTo(RectTransform rt, RectTransform grid) {
        if (rt == null || rt.parent != grid.parent) return;   // только соседи сетки внутри CanvasRoot
        rt.anchorMin = grid.anchorMin;
        rt.anchorMax = grid.anchorMax;
        rt.pivot = grid.pivot;
        rt.anchoredPosition = grid.anchoredPosition;
        rt.sizeDelta = grid.rect.size;
    }
}
