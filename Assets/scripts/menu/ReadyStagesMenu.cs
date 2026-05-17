using UnityEngine;

public class ReadyStagesMenu : MonoBehaviour {
    private BuilderService builderService;

    private void OnEnable() {
        Debug.LogWarning("OnEnable");
        if (builderService == null)
            builderService = FindFirstObjectByType<BuilderService>();
        builderService.PopulateReadyStagesMenu();
    }
}