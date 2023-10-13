using System.Collections;
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
    public TMP_Text statusText;

    private void Start()
    {
        InitializeLobby();
        
        startButton.onClick.AddListener(OnStartButtonClicked);
        NetworkManager.Singleton.OnClientStarted += OnClientStarted;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;

        if (statusText == null)
            Debug.LogWarning("statusText is not initialized in LobbyManager.");
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
            statusText.text = "Connected as Client";
        }
    }

    private void OnServerStarted()
    {
        startButton.gameObject.SetActive(true);
        statusLabel.text = "You are the host, please press start game when you are ready.";
        statusText.text = "Running as Host";
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
        // Will put client-specific logic here.
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

    private new void OnDestroy() 
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
