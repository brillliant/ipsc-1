using UnityEngine;

/// <summary>
/// Выбор режима патронов (Normal / Infinite) из дропдауна меню. Значение по умолчанию задаётся в инспекторе
/// и при старте передаётся в CompetitionModeService, чтобы совпадать с выбранным пунктом дропдауна.
/// </summary>
public class RoundsCountController : MonoBehaviour {
    
    private CompetitionModeService competitionModeService;

    void Awake() {
        competitionModeService = GetComponent<CompetitionModeService>();
    }

    void Start() {
        competitionModeService.setRoundsCount(roundsCount);
    }
    
    public RoundsCount roundsCount = RoundsCount.Infinite;
    
    public void onGameModeSelected(int index) {
        roundsCount = (RoundsCount)index;
        
        competitionModeService.setRoundsCount(roundsCount);
    }
}
