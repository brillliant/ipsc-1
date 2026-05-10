using UnityEngine;

public class RoundsCountController : MonoBehaviour {
    public enum RoundsMode {
        Normal = 0, 
        Infinity = 1
    }
    
    public RoundsMode roundsMode = RoundsMode.Normal;
    
    public void onGameModeSelected(int index) {
        roundsMode = (RoundsMode)index;
    }
}

