using System;
using UnityEngine;

public class StageBuilderScript : MonoBehaviour {
    private CompetitionModeService competitionModeService;
    private GameObject codeObject;

    public void Awake() {
        codeObject = GameObject.Find("codeObject");
        competitionModeService = codeObject.GetComponent<CompetitionModeService>();
    }

    private void OnDisable() {
        competitionModeService.stateEnum = StateEnum.Idle;
    }
}
