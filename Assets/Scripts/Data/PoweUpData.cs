using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public enum EffectTargetType
{
    Global,
    Player,
    Ball
}

public enum PlayerTarget
{
    Owner,
    Enemy,
    Both
}

[CreateAssetMenu(fileName = "PowerUp", menuName = "PowerUp System/PowerUp")]
public class PowerUpData : ScriptableObject
{
    public string id;
    public EffectData[] effects;
    public ConditionData[] conditions;

    public GameMode[] gameModes;
    public EffectTargetType targetType;
    public PlayerTarget playerTarget;

    public float spawnChance;
    public bool isTimed;
    public float duration;
    public int value;


    public bool AreAllConditionsMet(ConditionType type, PowerUpContext context)
    {
        foreach (ConditionData condition in conditions)
        {
            if (condition.type == type && !condition.IsMet(context.gameManager.CurrentGameMode, context))
                return false;
        }
        return true;
    }

    public void ApplyAllEffects(PowerUpContext context)
    {
        foreach (EffectData effect in effects)
        {
            effect.Apply(context.gameManager.CurrentGameMode, this, context);
        }
    }

    public void StopAllEffects(PowerUpContext context)
    {
        foreach (EffectData effect in effects)
        {
            effect.Stop(context.gameManager.CurrentGameMode, this, context);
        }
    }

    public void UpdateAllEffects(PowerUpContext context)
    {
        foreach (EffectData effect in effects)
        {
            effect.UpdateEffect(context.gameManager.CurrentGameMode, this, context);
        }
    }
}