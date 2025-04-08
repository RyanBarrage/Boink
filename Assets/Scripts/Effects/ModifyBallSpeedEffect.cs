using Fusion;
using UnityEngine;

[CreateAssetMenu(fileName = "ModifyBallSpeed", menuName = "PowerUp System/Effects/Modify Ball Speed")]
public class ModifyBallSpeedEffect : EffectData
{
    //public float speedMultiplier = 1.5f;

    public override void Apply(GameMode mode, PowerUpData powerup, PowerUpContext context)
    {
        foreach (var ball in context.balls)
        {
            ball.ModifyVelocity(powerup.value == 0 ? 1f : powerup.value);
        }
    }

    public override void Stop(GameMode mode, PowerUpData powerup, PowerUpContext context)
    {
        foreach (var ball in context.balls)
        {
            ball.ModifyVelocity(powerup.value == 0 ? 0 : 1f / powerup.value);
        }
    }

    public override void Update(GameMode mode, PowerUpData powerup, PowerUpContext context) { }
}