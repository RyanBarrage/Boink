using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Linq;

public class BallManager : NetworkBehaviour
{
    public static BallManager Instance { get; private set; }

    [Header("Ball Prefabs By GameMode")]
    public List<BallPrefabEntry> ballPrefabEntries;

    private Dictionary<GameMode, NetworkPrefabRef> prefabMap = new();
    private Dictionary<GameMode, List<NetworkObject>> activeBalls = new();

    private NetworkRunner runner;

    [Serializable]
    public struct BallPrefabEntry
    {
        public GameMode gameMode;
        public NetworkPrefabRef prefab;
    }

    public override void Spawned()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        runner = Runner;

        foreach (var entry in ballPrefabEntries)
        {
            prefabMap[entry.gameMode] = entry.prefab;
            activeBalls[entry.gameMode] = new List<NetworkObject>();
        }
    }

    /*public void InitializeBalls(IScoreService scoreService)
    {
        foreach (var mode in prefabMap.Keys)
        {
            SpawnBall(mode, scoreService);
        }
    }*/

    public void InitializeBall(GameMode mode, IScoreService scoreService)
    {
        SpawnBall(mode, scoreService);
    }

    public void SpawnBall(GameMode mode, IScoreService scoreService)
    {
        if (!prefabMap.TryGetValue(mode, out var prefab))
        {
            Debug.LogError($"No prefab assigned for GameMode: {mode}");
            return;
        }

        if (!runner.IsServer)
            return;

        NetworkObject ball = runner.Spawn(prefab, Vector3.zero, Quaternion.identity);
        activeBalls[mode].Add(ball);

        if (ball.GetComponent<IBallController>() is { } controller)
        {
            controller.Initialize(scoreService);
        }

        // Enable only for current game mode
        ball.gameObject.SetActive(mode == GameManager.Instance.CurrentGameMode);
    }

    public void SpawnBalls(GameMode mode, int count, IScoreService scoreService)
    {
        if (!prefabMap.TryGetValue(mode, out var prefab))
        {
            Debug.LogError($"Cannot spawn balls: No prefab mapped for mode {mode}");
            return;
        }

        if (!runner.IsServer) return;

        for (int i = 0; i < count; i++)
        {
            // You can randomize positions if needed
            Vector3 spawnPos = Vector3.zero + Vector3.right * i * 2f; // Spread out a bit
            NetworkObject ball = runner.Spawn(prefab, spawnPos, Quaternion.identity);
            if (!activeBalls.ContainsKey(mode))
            {
                activeBalls[mode] = new List<NetworkObject>();
            }
            activeBalls[mode].Add(ball);

            if (ball.GetComponent<IBallController>() is { } controller)
            {
                controller.Initialize(scoreService);
            }

            // Optional: Activate only if it's the current mode
            ball.gameObject.SetActive(mode == GameManager.Instance.CurrentGameMode);
        }
    }

    public void SpawnBalls(GameMode mode, int count)
    {
        SpawnBalls(mode, count, ScoreManager.Instance);
    }


    public void ResetBall(GameMode mode)
    {
        if (activeBalls.TryGetValue(mode, out var balls) &&
            balls.Count > 0 &&
            balls[0].GetComponent<IBallController>() is { } controller)
        {
            controller.ResetBall();
        }
    }

    public void CleanupBalls(GameMode mode, IScoreService scoreService)
    {
        if (!activeBalls.TryGetValue(mode, out var balls)) return;

        for (int i = 1; i < balls.Count; i++)
        {
            if (runner.IsServer)
                runner.Despawn(balls[i]);
        }

        if (balls.Count > 1)
            balls.RemoveRange(1, balls.Count - 1);

        if (balls.Count == 0)
        {
            SpawnBall(mode, scoreService);
        }
        else
        {
            balls[0].gameObject.SetActive(true);
            balls[0].GetComponent<IBallController>().ResetBall();
        }
    }

    /*public List<NetworkObject> GetActiveBalls(GameMode mode)
    {
        return activeBalls.TryGetValue(mode, out var balls) ? balls : new List<NetworkObject>();
    }*/
public List<BallControllerBase> GetActiveBalls(GameMode mode)
    {
        // Try to get the list of NetworkObjects for the given game mode.
        if (activeBalls.TryGetValue(mode, out var balls))
        {
            // Cast each NetworkObject to BallControllerBase and return the list.
            return balls.OfType<BallControllerBase>().ToList();
        }
        else
        {
            // If no balls are found, return an empty list.
            return new List<BallControllerBase>();
        }
    }
}
