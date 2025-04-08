using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class BoardManager : NetworkBehaviour
{
    public static BoardManager Instance;

    [SerializeField] private GameObject[] boardObjects;
    private Dictionary<GameMode, GameObject> boards = new();
    [SerializeField] private GameObject[] ballPrefabs;
    private Dictionary<GameMode, GameObject> ballPrefabMap = new();
    public Dictionary<GameMode, List<GameObject>> activeBalls = new();
    private BallManager ballManager;

    private RectTransform[] scoreTransforms;
    public float spawnRangeXMin, spawnRangeXMax, spawnRangeZMin, spawnRangeZMax; // Spawn area bounds

    private void Awake()
    {
        Instance = this;
    }

    public void InitializeBoard(GameMode mode)
    {
       //
    }

    
}
