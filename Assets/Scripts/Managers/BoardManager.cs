using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public struct SpawnBounds
{
    public float xMin, xMax, zMin, zMax;
}

public class BoardManager : NetworkBehaviour
{
    public static BoardManager Instance { get; private set; }

    [SerializeField] private GameModeDatabase gameModeDatabase;
    [SerializeField] private Transform boardParent;
    private TextMeshProUGUI player1Score;
    private TextMeshProUGUI player2Score;


    private Dictionary<GameMode, GameObject> boardInstances = new();
    private GameObject currentBoard;
    private GameMode? currentMode;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public SpawnBounds GetSpawnBounds()
    {
        var modeData = gameModeDatabase.GetData(currentMode.Value);
        return new SpawnBounds
        {
            xMin = modeData.spawnRangeXMin,
            xMax = modeData.spawnRangeXMax,
            zMin = modeData.spawnRangeZMin,
            zMax = modeData.spawnRangeZMax
        };
    }

    public Vector3 GetRandomSpawnPoint()
    {
        var bounds = GetSpawnBounds();
        float x = UnityEngine.Random.Range(bounds.xMin, bounds.xMax);
        float z = UnityEngine.Random.Range(bounds.zMin, bounds.zMax);
        return new Vector3(x, 0f, z); // assuming Y = 0 plane
    }

    public void LoadBoardForMode(GameMode mode)
    {
        if (currentMode == mode) return;
        currentMode = mode;

        // Deactivate current board
        if (currentBoard != null)
        {
            //currentBoard.SetActive(false);
            Destroy(currentBoard);
            currentBoard = null;
        }

        var modeData = gameModeDatabase.GetData(mode);
        if (modeData == null || modeData.boardPrefab == null)
        {
            Debug.LogError($"Missing board prefab for mode: {mode}");
            return;
        }

        // Instantiate board if needed
        if (!boardInstances.TryGetValue(mode, out var newBoard))
        {
            newBoard = Instantiate(modeData.boardPrefab, boardParent);
            boardInstances[mode] = newBoard;
        }

        newBoard.SetActive(true);
        currentBoard = newBoard;

        player1Score = newBoard.transform.Find("Player1ScoreText")?.GetComponent<TextMeshProUGUI>();
        player2Score = newBoard.transform.Find("Player2ScoreText")?.GetComponent<TextMeshProUGUI>();
    }

    public virtual void UpdateScoreText(int player1Score, int player2Score)
    {
        this.player1Score.text = player1Score.ToString();
        this.player2Score.text = player2Score.ToString();
    }

    public virtual RectTransform[] GetScoreTransforms()
    {
        return new RectTransform[] { player1Score.GetComponent<RectTransform>(), player2Score.GetComponent<RectTransform>() };
    }

    public GameObject GetCurrentBoard() => currentBoard;
}
