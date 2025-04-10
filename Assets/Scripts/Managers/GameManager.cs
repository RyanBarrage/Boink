using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameMode { Pong, AirHockey, Foosball, Space }
public enum GameState { Started, Running, Ended }

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Networked] public GameMode CurrentGameMode { get; private set; }
    [Networked] public GameState CurrentGameState { get; private set; }

    [SerializeField] private BoardManager boardManager;
    [SerializeField] private PowerUpManager powerUpManager;
    [SerializeField] private BallManager ballManager;
    [SerializeField] private GameMode defaultGameMode = GameMode.AirHockey;

    private ScoreManager scoreManager;
    private PlayerService playerService;

    public override void Spawned()
    {
        if (Instance == null) Instance = this;

        scoreManager = ScoreManager.Instance;
        playerService = new PlayerService();

        if (Object.HasStateAuthority)
        {
            CurrentGameState = GameState.Started;
            CurrentGameMode = defaultGameMode;
            StartGame(CurrentGameMode);
        }
    }

    public void StartGame(GameMode mode)
    {
        if (!Object.HasStateAuthority) return;

        CurrentGameMode = mode;
        CurrentGameState = GameState.Running;
        playerService.InitializePlayers();
        PaddleManager.Instance.UpdatePaddlesForMode(CurrentGameMode);
        boardManager.LoadBoardForMode(mode);
        ballManager.InitializeBall(CurrentGameMode, ScoreManager.Instance);
        
        RPC_SyncGameMode(mode);
    }

    public void OnModeChange(GameMode mode)
    {
        CurrentGameMode = mode;
        //change board
        //clear powerups
        PaddleManager.Instance.UpdatePaddlesForMode(CurrentGameMode);
        BallManager.Instance.UpdateBallsForMode(CurrentGameMode);

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SyncGameMode(GameMode newMode) => CurrentGameMode = newMode;

    public void ScorePoint(int playerID)
    {
        if (!Object.HasStateAuthority) return;

        scoreManager.AddScore(playerID);
        RPC_UpdateScore(scoreManager.Player1Score, scoreManager.Player2Score);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateScore(int p1, int p2) => scoreManager.SetScores(p1, p2);

    public List<BallControllerBase> GetActiveBalls() => BallManager.Instance.GetActiveBalls(CurrentGameMode);
    public List<PlayerController> GetAllPlayers() => playerService.GetAllPlayers();
    public PlayerService GetPlayerService() => playerService;
}
