using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpContext
{
    public List<PlayerController> players;
    public List<BallControllerBase> balls;
    public GameManager gameManager;
    public BallControllerBase triggeringBall;
    public int triggeringPlayerId;
}
