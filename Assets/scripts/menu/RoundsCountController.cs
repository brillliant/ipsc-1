using UnityEngine;

public class RoundsCountController : MonoBehaviour {
    
    private CompetitionModeService competitionModeService;

    void Awake() {
        competitionModeService = GetComponent<CompetitionModeService>();
    }
    
    public RoundsCount roundsCount = RoundsCount.Normal;
    
    public void onGameModeSelected(int index) {
        roundsCount = (RoundsCount)index;
        
        competitionModeService.setRoundsCount(roundsCount);
    }
}

