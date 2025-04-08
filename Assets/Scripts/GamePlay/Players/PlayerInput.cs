using Fusion;
using UnityEngine;

// struct for serializing inputs across the network
public struct PlayerInput : INetworkInput
{
    public int PlayerID;

    public Vector2 MoveAxis;
    public float Rotation;
    public bool FirePressed;

    public bool CycleStickLeft;
    public bool CycleStickRight;

    public int SelectedStickIndex;
}

