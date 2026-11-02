using UnityEngine;

/// <summary>
/// Панель готовых стейджей: при показе перестраивает список кнопок.
/// Список лежит в Scroll View с маской, зоны луча и тыка равны всей панели — подгонять их не нужно.
/// </summary>
public class ReadyStagesMenu : MonoBehaviour {
    private BuilderService builderService;

    private void OnEnable() {
        if (builderService == null)
            builderService = FindFirstObjectByType<BuilderService>();
        builderService.PopulateReadyStagesMenu();
    }
}
