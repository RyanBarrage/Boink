using Fusion;
using UnityEngine;

public class PowerupBox : NetworkBehaviour
{
    [Networked] public string PowerupID { get; set; }
    public PowerUpData powerup;

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        if (other.CompareTag("Ball"))
        {
            BallControllerBase ball = other.GetComponent<BallControllerBase>();
            if (ball == null) return;

            //PowerUpManager manager = FindObjectOfType<PowerUpManager>();
            //manager?.ApplyPowerUpByID(PowerupID, ball);
            PowerUpManager.Instance.ApplyPowerUp(PowerupID, ball.GetLastTouchPlayerID(), ball);

            Runner.Despawn(Object);
        }
    }
}
