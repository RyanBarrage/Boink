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
    [Networked] public GameState GameState { get; private set; }

    [SerializeField] private BoardManager boardManager;
    [SerializeField] private PowerUpManager powerUpManager;
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
            CurrentGameMode = defaultGameMode;
            boardManager.InitializeBoard(CurrentGameMode);
        }
    }

    public void StartGame(GameMode mode)
    {
        if (!Object.HasStateAuthority) return;

        CurrentGameMode = mode;
        boardManager.InitializeBoard(mode);
        playerService.InitializePlayers();
        RPC_SyncGameMode(mode);
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

    public void ApplyPowerUp(string powerupID, int instigatorID, BallControllerBase triggeringBall = null)
    {
        PowerUpData powerup = powerUpManager.GetPowerUpByID(powerupID);
        if (powerup == null) return;

        switch (powerup.targetType)
        {
            case EffectTargetType.Global:
                powerUpManager.ApplyPowerUpByID(powerup.id, triggeringBall);
                break;

            case EffectTargetType.Player:
                powerUpManager.ApplyPowerUpToPlayers(powerup, instigatorID);
                break;

            case EffectTargetType.Ball:
                powerUpManager.ApplyPowerUpToAllBalls(powerup);
                break;
        }
    }


    public void UpdatePowerups(float deltaTime)
    {
        powerUpManager.UpdatePowerups(deltaTime);
    }

    public List<BallControllerBase> GetActiveBalls() => BallManager.Instance.GetActiveBalls(CurrentGameMode);
    public List<PlayerController> GetAllPlayers() => playerService.GetAllPlayers();
    public PlayerService GetPlayerService() => playerService;
}
