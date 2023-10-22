using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq; // Added for Linq operations

public class Lobby : NetworkBehaviour
{
    public LobbyUi lobbyUi;
    public NetworkedPlayers networkedPlayers;

    // List of invalid words
    private readonly List<string> InvalidWords = new List<string> { "badword1", "badword2" };

    private void Start()
    {
        SetupCallbacksBasedOnRole();
        lobbyUi.OnChangeNameClicked += OnChangeNameClicked;
    }

    private void SetupCallbacksBasedOnRole()
    {
        if (IsServer)
        {
            networkedPlayers.allNetPlayers.OnListChanged += ServerOnNetworkPlayersChanged;
            ServerPopulateCards();
            lobbyUi.ShowStart(true);
            lobbyUi.OnStartClicked += ServerStartClicked;
        }
        else
        {
            ClientPopulateCards();
            networkedPlayers.allNetPlayers.OnListChanged += ClientOnNetworkPlayersChanged;
            lobbyUi.ShowStart(false);
            lobbyUi.OnReadyToggled += ClientOnReadyToggled;
            NetworkManager.OnClientDisconnectCallback += ClientOnClientDisconnect;
        }
    }

    private void PopulateMyInfo()
    {
        NetworkPlayerInfo? myInfo = networkedPlayers.GetPlayerInfoByClientId(NetworkManager.LocalClientId);
        if (myInfo.HasValue && myInfo.Value.clientId != ulong.MaxValue)
        {
            lobbyUi.SetPlayerName(myInfo.Value.playerName.ToString());
        }
    }

    private void ServerPopulateCards()
    {
        PopulateCards(true);
    }

    private void ClientPopulateCards()
    {
        PopulateCards(false);
    }

    private void PopulateCards(bool isServer)
    {
        lobbyUi.playerCards.Clear();
        foreach (var info in networkedPlayers.allNetPlayers)
        {
            var card = lobbyUi.playerCards.AddCard("Some player");
            card.ready = info.ready;
            card.color = info.color;
            card.clientId = info.clientId;
            card.playerName = info.playerName.ToString();
            if (isServer)
            {
                card.ShowKick(info.clientId != NetworkManager.LocalClientId);
                card.OnKickClicked += ServerOnKickClicked;
            }
            else
            {
                card.ShowKick(false);
            }
            card.UpdateDisplay();
        }
    }

    private void ServerStartClicked()
    {
        NetworkManager.SceneManager.LoadScene("ArenaOne", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void ClientOnReadyToggled(bool isReady)
    {
        UpdateReadyStatusServerRpc(isReady);
    }

    private void ServerOnNetworkPlayersChanged(NetworkListEvent<NetworkPlayerInfo> changeEvent)
    {
        ServerPopulateCards();
        PopulateMyInfo();
        lobbyUi.EnableStart(networkedPlayers.AllPlayersReady());
    }

    private void ServerOnKickClicked(ulong clientId)
    {
        NetworkManager.DisconnectClient(clientId);
    }

    private void ClientOnNetworkPlayersChanged(NetworkListEvent<NetworkPlayerInfo> changeEvent)
    {
        ClientPopulateCards();
        PopulateMyInfo();
    }

    private void ClientOnClientDisconnect(ulong clientId)
    {
        if (lobbyUi == null) return;
        lobbyUi.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateReadyStatusServerRpc(bool isReady, ServerRpcParams rpcParams = default)
    {
        networkedPlayers.UpdateReady(rpcParams.Receive.SenderClientId, isReady);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerNameServerRpc(string newName, ServerRpcParams rpcParams = default)
    {
        networkedPlayers.UpdatePlayerName(rpcParams.Receive.SenderClientId, newName);
    }

    private bool IsValidName(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length > 16) // character limit
            return false;

        if (name.Any(char.IsWhiteSpace) || name.Any(ch => !char.IsLetterOrDigit(ch))) // deny names with spaces or non-alphanumeric characters
            return false;

        if (InvalidWords.Any(word => name.ToLowerInvariant().Contains(word))) // deny names containing invalid words
            return false;

        return true;
    }

    private void OnChangeNameClicked(string newName)
    {
        if (IsValidName(newName))
        {
            UpdatePlayerNameServerRpc(newName);
        }
        else
        {
            // If name is invalid, reset the player's name in the lobby UI
            NetworkPlayerInfo? myInfo = networkedPlayers.GetPlayerInfoByClientId(NetworkManager.LocalClientId);
            if (myInfo.HasValue)
            {
                lobbyUi.SetPlayerName(myInfo.Value.playerName.ToString());
            }
        }
    }
}