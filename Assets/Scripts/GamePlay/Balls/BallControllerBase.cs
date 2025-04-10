using Fusion;
using UnityEngine;
using Fusion.Addons.Physics;

public interface IBallController
{
    void Initialize(IScoreService scoreService);
    void ResetBall();
    void ModifyVelocity(float multiplier);
    int GetLastTouchPlayerID();
}


public abstract class BallControllerBase : NetworkBehaviour, IBallController
{
    [Header("Ball Settings")]
    public float initialForce = 20f;
    public float bounceAngleRandomness = 5f;
    public NetworkRigidbody3D rb;

    [Networked] public int LastTouchPlayerID { get; set; }
    [Networked] public bool ShouldReset { get; set; }

    protected IScoreService scoreService;

    public virtual void Initialize(IScoreService scoreService)
    {
        rb = GetComponent<NetworkRigidbody3D>();
        this.scoreService = scoreService;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (ShouldReset)
        {
            ResetBall();
            ShouldReset = false;
        }
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;

        if (other.CompareTag("ScoringZoneLeft"))
        {
            scoreService.ScorePoint(2);
            ShouldReset = true;
        }
        else if (other.CompareTag("ScoringZoneRight"))
        {
            scoreService.ScorePoint(1);
            ShouldReset = true;
        }
    }

    public virtual void OnCollisionEnter(Collision collision)
    {
        if (!HasStateAuthority || collision.gameObject.CompareTag("ScoringZone")) return;

        Vector3 velocity = rb.Rigidbody.velocity;

        if (collision.gameObject.CompareTag("Paddle"))
        {
            ModifyVelocity(1.25f);
            if (collision.gameObject.TryGetComponent(out NetworkObject playerNetObj))
            {
                LastTouchPlayerID = playerNetObj.InputAuthority.PlayerId;
            }
        }

        if (velocity.magnitude > 0.1f)
        {
            float randomAngle = UnityEngine.Random.Range(-bounceAngleRandomness, bounceAngleRandomness);
            Quaternion rotation = Quaternion.Euler(0, randomAngle, 0);
            rb.Rigidbody.velocity = rotation * velocity;
        }
    }

    public virtual void ResetBall()
    {
        Transform tf = transform;
        tf.position = Vector3.zero;
        rb.Rigidbody.velocity = Vector3.zero;
        LastTouchPlayerID = -1;

        Vector3 randomDirection;
        float angle;
        do
        {
            randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-0.5f, 0.5f)).normalized;
            angle = Mathf.Abs(Mathf.Atan2(randomDirection.x, randomDirection.z) * Mathf.Rad2Deg);
        } while (angle < 20f || angle > 160f);

        rb.Rigidbody.AddForce(randomDirection * initialForce, ForceMode.Impulse);
    }

    public virtual void ModifyVelocity(float multiplier)
    {
        rb.Rigidbody.velocity *= multiplier;
    }

    public int GetLastTouchPlayerID() => LastTouchPlayerID;
}
