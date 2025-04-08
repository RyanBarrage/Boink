using UnityEngine;

public enum ConditionType
{
    Start,
    Stop
}

public abstract class ConditionData : ScriptableObject
{
    public ConditionType type;
    public abstract bool IsMet(GameMode mode, PowerUpContext context);
}