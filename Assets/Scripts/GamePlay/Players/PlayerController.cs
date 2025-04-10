using Fusion;
using UnityEngine;


// on the player prefab that gets spawned on the network
public class PlayerController : NetworkBehaviour
{
    [Networked] public int PlayerID { get; private set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            //PlayerID = Object.InputAuthority.PlayerId;
            PlayerID = 0;
        }
        else
        {
            PlayerID = 1;
        }

        if (Object.HasInputAuthority)
        {
            Debug.Log($"[PlayerController] Local player {PlayerID} spawned");
            // Attach camera or local controls here
        }
        else
        {
            Debug.Log($"[PlayerController] Remote player {PlayerID} spawned");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out PlayerInput input))
        {
            PaddleManager.Instance.GetPaddleForPlayer(input.PlayerID)?.HandleInput(input);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        Debug.Log($"[PlayerController] Player {PlayerID} despawned");

        // Optional: cleanup
        PaddleManager.Instance?.UnassignPaddleForPlayer(PlayerID);
    }

    public override void Render()
    {
        // Optional: render-side animation updates
    }

    public int GetPlayerID()
    {
        return PlayerID;
    }
}

