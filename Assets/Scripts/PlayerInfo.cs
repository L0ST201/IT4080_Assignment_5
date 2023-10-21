using UnityEngine;

// PlayerInfo.cs
[System.Serializable]
public class PlayerInfo
{
    public ulong ClientId { get; set; }
    public string PlayerName { get; set; }
    public Color PlayerColor { get; set; }
    public bool IsReady { get; set; } = false;

    public PlayerInfo(ulong clientId, string playerName, Color color)
    {
        ClientId = clientId;
        PlayerName = playerName;
        PlayerColor = color;
    }
}
