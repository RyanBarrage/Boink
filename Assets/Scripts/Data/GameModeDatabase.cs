using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GameModeDatabase", menuName = "Boink/GameModeDatabase")]
public class GameModeDatabase : ScriptableObject
{
    public List<GameModeData> modes;

    private Dictionary<GameMode, GameModeData> _lookup;

    public GameModeData GetData(GameMode mode)
    {
        _lookup ??= modes.ToDictionary(m => m.mode);
        return _lookup.TryGetValue(mode, out var data) ? data : null;
    }
}