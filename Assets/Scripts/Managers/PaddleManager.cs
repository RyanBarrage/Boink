using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BallManager;

public class PaddleManager : NetworkBehaviour
{
    public static PaddleManager Instance;

    [Header("Paddle Prefabs By GameMode")]
    public List<BallPrefabEntry> paddlePrefabEntries;

    public Dictionary<GameMode, NetworkPrefabRef> paddlePrefabMap = new();
    public Dictionary<int, PaddleControllerBase> playerPaddles = new();

    [Serializable]
    public struct PaddlePrefabEntry
    {
        public GameMode gameMode;
        public NetworkPrefabRef prefab;
    }

    public override void Spawned()
    {
        Instance = this;

        InitializePrefabMap();
    }

    private void InitializePrefabMap()
    {
        foreach (var entry in paddlePrefabEntries)
        {
            paddlePrefabMap[entry.gameMode] = entry.prefab;
        }
    }

    public void SpawnPaddleForPlayer(PlayerRef player, GameMode mode)
    {
        if (!paddlePrefabMap.TryGetValue(mode, out var prefab))
        {
            Debug.LogError($"No paddle prefab for mode {mode}");
            return;
        }

        if (!Runner.IsServer) return;

        Vector3 spawnPosition = Vector3.zero;// GetSpawnPositionForPlayer(player);
        var paddleObj = Runner.Spawn(prefab, spawnPosition, Quaternion.identity, player);

        if (paddleObj.TryGetComponent<PaddleControllerBase>(out var nb) &&
            nb is IPaddleController controller)
        {
            controller.Initialize(player, ScoreManager.Instance);
            playerPaddles[player.PlayerId] = nb;
        }
    }

    public IPaddleController GetPaddleForPlayer(int playerId)
    {
        if (playerPaddles.TryGetValue(playerId, out var paddleMono))
        {
            return paddleMono as IPaddleController;
        }

        return null;
    }

    public void ResetAllPaddles()
    {
        foreach (var kvp in playerPaddles)
        {
            if (kvp.Value != null)
                Runner.Despawn(kvp.Value.Object);
        }
        playerPaddles.Clear();
    }


    public void UpdatePaddlesForMode(GameMode newMode)
    {
        ResetAllPaddles();

        foreach (var player in Runner.ActivePlayers)
        {
            SpawnPaddleForPlayer(player, newMode);
        }
    }

    public void UnassignPaddleForPlayer(int playerId)
    {
        if (playerPaddles.TryGetValue(playerId, out var paddle))
        {
            Destroy(paddle.gameObject);
            playerPaddles.Remove(playerId);
        }
    }
}

