using Fusion;
using UnityEngine;


// on the player prefab that gets spawned on the network
public class PlayerController : NetworkBehaviour
{
    private int playerID = -1;

    public override void Spawned()
    {
        playerID = Object.InputAuthority.PlayerId;

        if (Object.HasInputAuthority)
        {
            Debug.Log($"[PlayerController] Spawned local player {playerID}");
        }
        else
        {
            Debug.Log($"[PlayerController] Spawned remote player {playerID}");
        }

        // Optional: attach camera or UI if this is the local player
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
        Debug.Log($"[PlayerController] Player {playerID} despawned");

        // Optional: cleanup
        PaddleManager.Instance?.UnassignPaddleForPlayer(playerID);
    }

    public override void Render()
    {
        // Optional: render-side animation updates
    }

    public int GetPlayerID()
    {
        return playerID;
    }
}

