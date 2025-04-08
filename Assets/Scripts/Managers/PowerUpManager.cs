using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class PowerUpManager : NetworkBehaviour
{
    public static PowerUpManager Instance;

    [SerializeField] private List<PowerUpData> allPowerUps;

    private readonly Dictionary<string, PowerUpData> powerupLookup = new();
    private readonly List<ActivePowerUp> activeGlobalPowerups = new();
    private readonly Dictionary<int, List<ActivePowerUp>> activePlayerPowerups = new();
    private readonly Dictionary<BallControllerBase, List<ActivePowerUp>> activeBallPowerups = new();



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
                powerupLookup[pu.id] = pu;
        }
    }

    public PowerUpData GetPowerUpByID(string id)
    {
        powerupLookup.TryGetValue(id, out PowerUpData powerup);
        return powerup;
    }

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

    public void UpdatePowerups(float deltaTime)
    {
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

    private void Update()
    {
        if (IsGameRunning()) // Check if the game is actively running.
        {
            UpdatePowerups(Time.deltaTime); // Call the UpdatePowerups method.
        }
    }

    private bool IsGameRunning()
    {
        // Implement logic to determine if the game is running.
        return GameManager.Instance.GameState == GameState.Running; // Placeholder, replace with actual game state check.
    }
}
