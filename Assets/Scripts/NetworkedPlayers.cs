using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class NetworkedPlayers : NetworkBehaviour
{
    public NetworkList<NetworkPlayerInfo> allNetPlayers = new NetworkList<NetworkPlayerInfo>();

    private static readonly List<Color> _availableColors = new List<Color>
    {
        Color.black,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta
    };

    private Dictionary<ulong, Color> _assignedColors = new Dictionary<ulong, Color>();

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (IsServer)
        {
            InitializeServer();
        }
    }

    private void InitializeServer()
    {
        RegisterServerCallbacks();
        SetupHostPlayer();
    }

    private void RegisterServerCallbacks()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += ServerOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += ServerOnClientDisconnected;
    }

    private void SetupHostPlayer()
    {
        var hostInfo = new NetworkPlayerInfo(NetworkManager.LocalClientId)
        {
            ready = true,
            color = AssignColorToClient(NetworkManager.LocalClientId),
            playerName = "The Host"
        };
        allNetPlayers.Add(hostInfo);
    }

    public Color AssignColorToClient(ulong clientId)
    {
        if (_assignedColors.TryGetValue(clientId, out Color assignedColor))
        {
            return assignedColor;
        }

        assignedColor = NextColor();
        _assignedColors[clientId] = assignedColor;
        return assignedColor;
    }

    private void ServerOnClientConnected(ulong newClientId)
    {
        if (FindPlayerIndex(newClientId) != -1) return;

        var clientInfo = new NetworkPlayerInfo(newClientId)
        {
            ready = false,
            color = AssignColorToClient(newClientId),
            playerName = $"Player {newClientId}"
        };
        allNetPlayers.Add(clientInfo);

        UpdateAllClientsAboutNewPlayer(newClientId);
    }

    private void UpdateAllClientsAboutNewPlayer(ulong newClientId)
    {
        var newClientPlayerObject = NetworkManager.Singleton.ConnectedClients[newClientId]?.PlayerObject;
        var newClientColorManager = newClientPlayerObject?.GetComponent<PlayerColorManager>();

        foreach (var playerInfo in allNetPlayers)
        {
            if (playerInfo.clientId != newClientId)
            {
                newClientColorManager?.UpdatePlayerColorClientRpc(playerInfo.clientId, playerInfo.color);
            }
        }

        foreach (var clientEntry in NetworkManager.Singleton.ConnectedClients)
        {
            if (clientEntry.Key != newClientId)
            {
                var existingClientPlayerObject = clientEntry.Value.PlayerObject;
                var existingClientColorManager = existingClientPlayerObject?.GetComponent<PlayerColorManager>();
                existingClientColorManager?.UpdatePlayerColorClientRpc(newClientId, _assignedColors[newClientId]);
            }
        }
    }

    private void ServerOnClientDisconnected(ulong clientId)
    {
        int idx = FindPlayerIndex(clientId);
        if (idx != -1) 
        {
            ReturnColor(allNetPlayers[idx].color);
            allNetPlayers.RemoveAt(idx);
            _assignedColors.Remove(clientId);
        }
    }

    public Color NextColor()
    {
        if (_availableColors.Count == 0) return Color.gray;

        var newColor = _availableColors[0];
        _availableColors.RemoveAt(0);
        return newColor;
    }

    public void ReturnColor(Color color)
    {
        if (!_availableColors.Contains(color) && color != Color.black)
        {
            _availableColors.Add(color);
        }
    }

    public int FindPlayerIndex(ulong clientId)
    {
        for (int i = 0; i < allNetPlayers.Count; i++)
        {
            if (allNetPlayers[i].clientId == clientId)
                return i;
        }
        return -1;
    }

    public void UpdateReady(ulong clientId, bool ready)
    {
        int idx = FindPlayerIndex(clientId);
        if (idx == -1) return;

        var playerInfo = allNetPlayers[idx];
        playerInfo.ready = ready;
        allNetPlayers[idx] = playerInfo;
    }

    public void UpdatePlayerName(ulong clientId, string playerName)
    {
        int idx = FindPlayerIndex(clientId);
        if (idx == -1) return;

        var playerInfo = allNetPlayers[idx];
        playerInfo.playerName = playerName;
        allNetPlayers[idx] = playerInfo;
    }

    public NetworkPlayerInfo? GetPlayerInfoByClientId(ulong clientId)
    {
        int idx = FindPlayerIndex(clientId);
        return idx != -1 ? (NetworkPlayerInfo?)allNetPlayers[idx] : null;
    }

    public bool AllPlayersReady() 
    {
        foreach (var player in allNetPlayers)
        {
            if (!player.ready)
                return false;
        }
        return true;
    }
}
