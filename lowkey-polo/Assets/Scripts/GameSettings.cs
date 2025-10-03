using UnityEngine;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Memory Game/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Board Configuration")]
    public Vector2Int[] availableBoardSizes = { 
        new Vector2Int(2, 2), 
        new Vector2Int(4, 4), 
        new Vector2Int(6, 4) 
    };
    
    [Header("Gameplay")]
    public float cardFlipDuration = 0.3f;
    public float mismatchViewTime = 1.0f;
    public float comboTimeLimit = 3.0f;
    public int baseScore = 100;
    public float comboMultiplier = 1.5f;
    
    [Header("Performance")]
    public int maxPoolSize = 50;
    public bool useObjectPooling = true;
}
