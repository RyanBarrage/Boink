using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerService
{
    private Dictionary<int, PlayerController> players = new();

    public void InitializePlayers()
    {
        players.Clear();

        foreach (var pc in Object.FindObjectsOfType<PlayerController>())
        {
            int id = pc.GetPlayerID();
            if (!players.ContainsKey(id))
            {
                players.Add(id, pc);
                Debug.Log($"[PlayerService] Registered player {id}");
            }
        }
    }

    public PlayerController GetPlayer(int id) =>
        players.TryGetValue(id, out var player) ? player : null;

    public List<PlayerController> GetAllPlayers() => players.Values.ToList();
}
