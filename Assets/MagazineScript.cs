using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class MagazineScript : MonoBehaviour {
    private int roundCount = 3;//17;

    public int getRoundCount() {
        return roundCount;
    }

    public void decrementRoundCount() {
        roundCount--;
    }
    void Start() {
    }

    void Update() {
        
    }
}