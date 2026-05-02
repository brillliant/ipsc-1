using UnityEngine;

public class ShootingModeController : MonoBehaviour {
    public enum ShootingMode {
        Competition = 0, 
        DryFire = 1
    }
    
    public ShootingMode currentMode = ShootingMode.Competition;
    
    public void onGameModeSelected(int index) {
        currentMode = (ShootingMode)index;
    }
}

