using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using static BallManager;

public class PowerUpManager : NetworkBehaviour
{
    public static PowerUpManager Instance;

    [SerializeField] private List<PowerUpData> allPowerUps;
    [Header("Powerup Prefabs By GameMode")]
    public List<PowerupPrefabEntry> powerupPrefabEntries;

    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int maxPowerups = 5;
    [SerializeField] private float spawnRadius = 1f;
    [SerializeField] private float minimumSpawnDistance = 1f;

    private readonly Dictionary<string, PowerUpData> powerupLookup = new();
    private Dictionary<GameMode, NetworkPrefabRef> prefabMap = new();
    private Dictionary<GameMode, List<PowerUpData>> modePowerups = new();
    private readonly List<ActivePowerUp> activeGlobalPowerups = new();
    private readonly Dictionary<int, List<ActivePowerUp>> activePlayerPowerups = new();
    private readonly Dictionary<BallControllerBase, List<ActivePowerUp>> activeBallPowerups = new();

    private float timeSinceLastSpawn;
    private int powerupBoxCount = 0;

    [Serializable]
    public struct PowerupPrefabEntry
    {
        public GameMode gameMode;
        public NetworkPrefabRef prefab;
    }

    private void Awake()
    {
        Instance = this;
        InitializePowerUps();
    }

    private void InitializePowerUps()
    {
        foreach (PowerUpData pu in allPowerUps)
        {
            if (!string.IsNullOrWhiteSpace(pu.id))
            {
                powerupLookup[pu.id] = pu;
                foreach (GameMode mode in System.Enum.GetValues(typeof(GameMode)))
                {
                    if (pu.gameModes.Contains(mode))
                        modePowerups[mode].Add(pu);
                }
            }
                
        }
        InitializePrefabMap();
    }

    private void InitializePrefabMap()
    {
        foreach (var entry in powerupPrefabEntries)
        {
            prefabMap[entry.gameMode] = entry.prefab;
        }
    }

    public PowerUpData GetPowerUpByID(string id) => powerupLookup.GetValueOrDefault(id);

    public PowerUpData GetRandomPowerup(GameMode mode)
    {
        if (!modePowerups.TryGetValue(mode, out List<PowerUpData> powerups) || powerups.Count == 0)
        {
            Debug.LogWarning($"No powerups available for mode {mode}");
            return null;
        }

        float totalWeight = powerups.Sum(p => p.spawnChance);
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0;

        foreach (PowerUpData powerup in powerups)
        {
            cumulative += powerup.spawnChance;
            if (randomValue <= cumulative)
                return powerup;
        }

        return null;
    }

    private void TrySpawnPowerUp()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();

        if (IsSpawnPositionValid(spawnPosition))
        {
            PowerUpData powerup = GetRandomPowerup(GameManager.Instance.CurrentGameMode);
            if (powerup != null)
            {
                SpawnPowerup(GameManager.Instance.CurrentGameMode, spawnPosition, powerup);
            }
        }
        else
        {
            TrySpawnPowerUp(); // Retry
        }
    }

    public void SpawnPowerup(GameMode mode, Vector3 position, PowerUpData powerup)
    {
        if (Runner.IsServer) // Ensure this only happens on the host
        {
            NetworkObject powerUpObject = Runner.Spawn(prefabMap[mode], position); // Network-spawned object
            PowerupBox powerUpBox = powerUpObject.GetComponent<PowerupBox>();

            // Set the PowerupID on the PowerupBox component
            powerUpBox.PowerupID = powerup.id;

            // Increment the power-up box count on the host
            IncrementPowerupBoxCount();
        }
    }

    private bool IsSpawnPositionValid(Vector3 spawnPosition)
    {
        Collider[] colliders = Physics.OverlapSphere(spawnPosition, spawnRadius);

        foreach (Collider c in colliders)
        {
            if (c.bounds.Contains(spawnPosition)) return false;
            if (Vector3.Distance(spawnPosition, c.ClosestPoint(spawnPosition)) < minimumSpawnDistance) return false;

        }

        foreach (RectTransform rect in BoardManager.Instance.GetScoreTransforms())
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, Camera.main.WorldToScreenPoint(spawnPosition)))
            {
                return false;
            }
        }

        return true;
    }

    public void IncrementPowerupBoxCount() => powerupBoxCount++;
    public void DecrementPowerupBoxCount() => powerupBoxCount--;

    private Vector3 GetRandomSpawnPosition()
    {
        return BoardManager.Instance.GetRandomSpawnPoint();
    }

    /*private Vector3 GetRandomSpawnPosition()
    {
        // Create a spawn position on the spawn plane (assumes Y is constant)
        Vector3 spawnPosition = BoardManager.Instance.GetRandomSpawnPoint();

        // Ensure the spawn position is valid
        if (IsSpawnPositionValid(spawnPosition))
        {
            return spawnPosition;
        }
        else
        {
            // If invalid, try again recursively (to find a valid spawn)
            return GetRandomSpawnPosition();
        }
    }*/

    private List<PlayerController> GetFilteredPlayers(PowerUpData powerup, PowerUpContext context)
    {
        List<PlayerController> allPlayers = context.players;
        int instigatorId = context.triggeringPlayerId;

        return powerup.playerTarget switch
        {
            PlayerTarget.Owner => allPlayers.Where(p => p.GetPlayerID() == instigatorId).ToList(),
            PlayerTarget.Enemy => allPlayers.Where(p => p.GetPlayerID() != instigatorId).ToList(),
            PlayerTarget.Both => new List<PlayerController>(allPlayers),
            _ => new List<PlayerController>()
        };
    }

    public void ApplyPowerUp(string powerupID, int instigatorID, BallControllerBase triggeringBall = null)
    {
        PowerUpData powerup = GetPowerUpByID(powerupID);
        if (powerup == null) return;

        switch (powerup.targetType)
        {
            case EffectTargetType.Global:
                ApplyPowerUpByID(powerup.id, triggeringBall);
                break;

            case EffectTargetType.Player:
                ApplyPowerUpToPlayers(powerup, instigatorID);
                break;

            case EffectTargetType.Ball:
                ApplyPowerUpToAllBalls(powerup);
                break;
        }
    }

    public void ApplyPowerUp(PowerUpData powerup, PowerUpContext context)
    {
        if (!powerup.AreAllConditionsMet(ConditionType.Start, context))
            return;

        powerup.ApplyAllEffects(context);

        if (powerup.isTimed)
        {
            ActivePowerUp active = new ActivePowerUp(powerup, context);

            switch (powerup.targetType)
            {
                case EffectTargetType.Global:
                    activeGlobalPowerups.Add(active);
                    break;

                case EffectTargetType.Player:
                    foreach (PlayerController player in GetFilteredPlayers(powerup, context))
                        AddActivePlayerPowerup(player.GetPlayerID(), active);
                    break;

                case EffectTargetType.Ball:
                    foreach (BallControllerBase ball in context.balls)
                        AddActiveBallPowerup(ball, active);
                    break;
            }

            StartCoroutine(RemoveTimedEffect(active));
        }
    }

    public void ApplyPowerUpByID(string id, BallControllerBase triggeringBall)
    {
        PowerUpData powerup = GetPowerUpByID(id);
        if (powerup != null)
        {
            PowerUpContext context = CreateContext(triggeringBall, triggeringBall.GetLastTouchPlayerID());
            ApplyPowerUp(powerup, context);
        }
    }

    public void ApplyPowerUpToPlayers(PowerUpData powerup, int instigatorID)
    {
        PowerUpContext context = CreateContext(null, instigatorID);
        ApplyPowerUp(powerup, context);
    }

    public void ApplyPowerUpToAllBalls(PowerUpData powerup)
    {
        GameManager gm = GameManager.Instance;
        List<BallControllerBase> balls = gm.GetActiveBalls();

        foreach (BallControllerBase ball in balls)
        {
            PowerUpContext context = CreateContext(ball, -1);
            ApplyPowerUp(powerup, context);
        }
    }

    private void AddActivePlayerPowerup(int playerId, ActivePowerUp active)
    {
        if (!activePlayerPowerups.ContainsKey(playerId))
            activePlayerPowerups[playerId] = new List<ActivePowerUp>();

        activePlayerPowerups[playerId].Add(active);
    }

    private void AddActiveBallPowerup(BallControllerBase ball, ActivePowerUp active)
    {
        if (!activeBallPowerups.ContainsKey(ball))
            activeBallPowerups[ball] = new List<ActivePowerUp>();

        activeBallPowerups[ball].Add(active);
    }

    private void Update()
    {
        if (!IsGameRunning() || !Runner.IsServer) return;

        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn >= spawnInterval && powerupBoxCount < maxPowerups)
        {
            timeSinceLastSpawn = 0f;
            TrySpawnPowerUp();
        }

        UpdatePowerups(Time.deltaTime); // Already exists
    }

    public void UpdatePowerups(float deltaTime)
    {
        if (!Runner.IsServer)
            return;

        // Update global power-ups
        UpdateActivePowerUps(activeGlobalPowerups, deltaTime);

        // Update player-specific power-ups
        foreach (List<ActivePowerUp> activePowerups in activePlayerPowerups.Values)
        {
            UpdateActivePowerUps(activePowerups, deltaTime);
        }

        // Update ball-specific power-ups
        foreach (List<ActivePowerUp> activePowerups in activeBallPowerups.Values)
        {
            UpdateActivePowerUps(activePowerups, deltaTime);
        }

        int totalActivePowerups = activeGlobalPowerups.Count +
                                  activePlayerPowerups.Values.Sum(list => list.Count) +
                                  activeBallPowerups.Values.Sum(list => list.Count);

        if (totalActivePowerups >= maxPowerups)
        {
            // Trigger the mode switch or any other logic related to this condition
            GameManager.Instance.OnModeChange((GameMode)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(GameMode)).Length)); // Call your custom logic to handle mode switching
        }
    }

    private void UpdateActivePowerUps(List<ActivePowerUp> activePowerups, float deltaTime)
    {
        for (int i = activePowerups.Count - 1; i >= 0; i--)
        {
            ActivePowerUp active = activePowerups[i];
            active.elapsedTime += deltaTime;

            if (active.IsExpired || active.powerup.AreAllConditionsMet(ConditionType.Stop, active.context))
            {
                active.powerup.StopAllEffects(active.context);
                activePowerups.RemoveAt(i);
            }
        }
    }

    private PowerUpContext CreateContext(BallControllerBase triggeringBall, int triggeringPlayerId)
    {
        return new PowerUpContext
        {
            players = GameManager.Instance.GetAllPlayers(),
            balls = GameManager.Instance.GetActiveBalls(),
            gameManager = GameManager.Instance,
            triggeringBall = triggeringBall,
            triggeringPlayerId = triggeringPlayerId
        };
    }

    private IEnumerator RemoveTimedEffect(ActivePowerUp active)
    {
        yield return new WaitForSeconds(active.powerup.duration);
        active.powerup.StopAllEffects(active.context);

        switch (active.powerup.targetType)
        {
            case EffectTargetType.Global:
                activeGlobalPowerups.Remove(active);
                break;

            case EffectTargetType.Player:
                foreach (PlayerController player in GetFilteredPlayers(active.powerup, active.context))
                    activePlayerPowerups[player.GetPlayerID()]?.Remove(active);
                break;

            case EffectTargetType.Ball:
                foreach (BallControllerBase ball in active.context.balls)
                    activeBallPowerups[ball]?.Remove(active);
                break;
        }
    }

    public IReadOnlyList<ActivePowerUp> GetActiveGlobalPowerUps() => activeGlobalPowerups;

    private bool IsGameRunning()
    {
        // Implement logic to determine if the game is running.
        return GameManager.Instance.CurrentGameState == GameState.Running; // Placeholder, replace with actual game state check.
    }
}
