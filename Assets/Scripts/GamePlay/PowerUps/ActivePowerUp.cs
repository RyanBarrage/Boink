public class ActivePowerUp
{
    public PowerUpData powerup { get; private set; }
    public float elapsedTime { get; set; }
    public PowerUpContext context { get; private set; }

    public ActivePowerUp(PowerUpData powerup, PowerUpContext context)
    {
        this.powerup = powerup;
        this.context = context;
        this.elapsedTime = 0f;
    }

    public bool IsExpired => elapsedTime >= powerup.duration;
}
