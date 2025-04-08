using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public abstract class EffectData : ScriptableObject
{
    public abstract void Apply(GameMode mode, PowerUpData powerup, PowerUpContext context);
    public abstract void Stop(GameMode mode, PowerUpData powerup, PowerUpContext context);
    public abstract void Update(GameMode mode, PowerUpData powerup, PowerUpContext context);

    protected IEnumerable<PlayerController> FilterPlayers(PowerUpData powerup, PowerUpContext context)
    {
        int instigatorId = context.triggeringPlayerId;

        return powerup.playerTarget switch
        {
            PlayerTarget.Owner => context.players.Where(p => p.GetPlayerID() == instigatorId),
            PlayerTarget.Enemy => context.players.Where(p => p.GetPlayerID() != instigatorId),
            PlayerTarget.Both => context.players,
            _ => Enumerable.Empty<PlayerController>()
        };
    }
}
