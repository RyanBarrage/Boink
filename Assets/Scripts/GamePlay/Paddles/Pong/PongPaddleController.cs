using Fusion.Addons.Physics;
using UnityEngine;

public class PongPaddleController : PaddleControllerBase
{
    public NetworkRigidbody3D rb;
    public float speed = 10f;

    public override void HandleInput(PlayerInput input)
    {
        if (!HasStateAuthority) return;

        Vector3 direction = new Vector3(0, 0, input.MoveAxis.y);
        rb.Rigidbody.velocity = direction * speed;
    }

    /*public override void ApplyEffect(EffectData effect)
    {
        if (effect.Type == EffectType.IncreaseSize)
            transform.localScale *= 1.5f;
    }*/

    public override void ResetPaddle()
    {
        transform.position = Vector3.zero;
        rb.Rigidbody.velocity = Vector3.zero;
    }
}
