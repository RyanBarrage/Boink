using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerService
{
    private Dictionary<int, PlayerController> players = new();

    public void InitializePlayers()
    {
        foreach (PlayerController player in Object.FindObjectsOfType<PlayerController>())
        {
            int id = player.GetPlayerID();
            if (!players.ContainsKey(id))
                players.Add(id, player);
        }
    }

    public PlayerController GetPlayer(int id) =>
        players.TryGetValue(id, out PlayerController p) ? p : null;

    public List<PlayerController> GetAllPlayers() => players.Values.ToList();

}
