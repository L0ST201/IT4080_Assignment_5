using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : NetworkBehaviour
{
    [Header("UI Elements")]
    public Button startButton;
    public TMP_Text statusLabel;
    public TMP_InputField playerNameInput;
    public Button readyButton;
    public LobbyUi lobbyUi; // Reference to LobbyUi
    public PlayerCards playerCards; // Reference to PlayerCards
    
    private readonly List<string> prohibitedWords = new List<string> { "exampleBadWord1", "exampleBadWord2" };

    private void Start()
    {
        InitializeLobby();
        
        startButton.onClick.AddListener(OnStartButtonClicked);
        readyButton.onClick.AddListener(ToggleReadyStatus);

        NetworkManager.Singleton.OnClientStarted += OnClientStarted;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;

        if (playerNameInput != null)
        {
            playerNameInput.onEndEdit.AddListener(ChangePlayerName);
        }
    }

    private void InitializeLobby()
    {
        startButton.gameObject.SetActive(false);
        statusLabel.text = "Start something, like the server or the host or the client.";
    }

    private void OnClientStarted()
    {
        if (!IsHost)
        {
            statusLabel.text = "Waiting for host to start the game.";
        }
    }

    private void OnServerStarted()
    {
        startButton.gameObject.SetActive(true);
        statusLabel.text = "You are the host, please press start game when you are ready.";
    }

    public void ChangePlayerName(string newName)
    {
        SetPlayerNameServerRpc(NetworkManager.Singleton.LocalClientId, newName);
    }

    [ServerRpc]
    public void SetPlayerNameServerRpc(ulong clientId, string playerName)
    {
        if (!IsValidName(playerName))
        {
            DenyPlayerNameClientRpc(clientId);
            return;
        }

        // Update the PlayerCard in the UI
        playerCards.UpdatePlayerName(clientId, playerName);
    }

    [ClientRpc]
    public void DenyPlayerNameClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            playerNameInput.text = "";
        }
    }

    private bool IsValidName(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length < 3 || name.Length > 12)
        {
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(name, "^[a-zA-Z0-9]*$"))
        {
            return false;
        }

        foreach (string word in prohibitedWords)
        {
            if (name.ToLower().Contains(word.ToLower()))
            {
                return false;
            }
        }

        return true;
    }

    private void ToggleReadyStatus()
    {
        // Logic to toggle the player's ready status
    }

    private void OnStartButtonClicked()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            StartGameServerRpc();
        }
        else
        {
            Debug.Log("This instance does not recognize itself as a server.");
        }
    }

    [ServerRpc]
    public void StartGameServerRpc()
    {
        StartGame();
        StartGameClientRpc();
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        // Client-specific logic here.
    }

    public void StartGame()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("ArenaOne", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    public void OnQuitGameButtonClicked()
    {
        startButton.gameObject.SetActive(false);
        statusLabel.text = "Start something, like the server or the host or the client.";
    }

    public override void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
    }
}