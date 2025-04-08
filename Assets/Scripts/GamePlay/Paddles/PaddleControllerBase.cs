using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public interface IPaddleController
{
    void Initialize(PlayerRef player, IScoreService scoreService);
    void HandleInput(PlayerInput input);
    //void ApplyEffect(EffectData effect);
    void ResetPaddle();
}

public abstract class PaddleControllerBase : NetworkBehaviour, IPaddleController
{
    [Networked] public int PlayerID { get; set; }
    protected PlayerRef ownerPlayer;
    protected IScoreService scoreService;

    public virtual void Initialize(PlayerRef player, IScoreService scoreService)
    {
        this.ownerPlayer = player;
        this.scoreService = scoreService;
        PlayerID = player.PlayerId;
    }

    public abstract void HandleInput(PlayerInput input);
    //public abstract void ApplyEffect(EffectData effect);
    public abstract void ResetPaddle();
}

