using Fusion;
using UnityEngine;
using Fusion.Addons.Physics;


public class AirHockeyBallController : BallControllerBase
{
    private float speedCap = 15f;


    public override void OnCollisionEnter(Collision collision)
    {
        if (!HasStateAuthority || collision.gameObject.CompareTag("ScoringZoneLeft") || collision.gameObject.CompareTag("ScoringZoneRight")) return;

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

    /*public virtual void ResetBall()
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
    }*/

    public override void ModifyVelocity(float multiplier)
    {
        base.ModifyVelocity(multiplier);

        // Cap the maximum speed.
        if (rb.Rigidbody.velocity.magnitude > speedCap)
        {
            rb.Rigidbody.velocity = rb.Rigidbody.velocity.normalized * speedCap;
        }
    }
}
