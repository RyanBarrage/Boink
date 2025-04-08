using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PaddleManager : NetworkBehaviour
{
    public static PaddleManager Instance;

    public Dictionary<GameMode, GameObject> paddlePrefabMap = new();
    public Dictionary<int, PaddleControllerBase> playerPaddles = new();

    public override void Spawned()
    {
        Instance = this;
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

    public void UnassignPaddleForPlayer(int playerId)
    {
        if (playerPaddles.TryGetValue(playerId, out var paddle))
        {
            Destroy(paddle.gameObject);
            playerPaddles.Remove(playerId);
        }
    }
}

