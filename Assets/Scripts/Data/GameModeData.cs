using UnityEngine;

[CreateAssetMenu(fileName = "GameModeData", menuName = "Boink/GameModeData")]
public class GameModeData : ScriptableObject
{
    public GameMode mode;
    public GameObject boardPrefab;
    public float spawnRangeXMin, spawnRangeXMax;
    public float spawnRangeZMin, spawnRangeZMax;
    public string description;
    // Add more board-specific settings here if needed
}