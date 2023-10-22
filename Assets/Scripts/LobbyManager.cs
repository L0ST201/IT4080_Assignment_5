using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    [Header("UI Elements")]
    public TMP_Text statusLabel;
    public TMP_Text statusText;
    public Button startClientButton;
    public Button hostGameButton;
    public Button startServerButton;

    public enum GameState
    {
        Lobby,
        InGame
    }

    public static GameState CurrentGameState = GameState.Lobby;

    private void Start()
    {
        EnsurePersistence();
        InitializeUI();
    }

    private void EnsurePersistence()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void InitializeUI()
    {
        if (startClientButton == null || hostGameButton == null || startServerButton == null)
        {
            Debug.LogError("One or more UI buttons are not initialized in LobbyManager.");
            return;
        }

        startClientButton.onClick.AddListener(OnStartClientButtonClicked);
        hostGameButton.onClick.AddListener(OnHostGameButtonClicked);
        startServerButton.onClick.AddListener(OnStartServerButtonClicked);

        if (statusText == null)
        {
            Debug.LogWarning("statusText is not initialized in LobbyManager.");
        }
    }

    private void HandleNetworkInstance()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
        {
            Debug.LogWarning("Network instance is already running. Shutting down previous instance.");
            NetworkManager.Singleton.Shutdown();
        }
    }

    private void TransitionToLobby()
    {
        if (IsServer)
        {
            CurrentGameState = GameState.Lobby;
            NetworkManager.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        }
    }

    private void OnStartClientButtonClicked()
    {
        HandleNetworkInstance();
        NetworkManager.Singleton.StartClient();
        TransitionToLobby();
    }

    private void OnHostGameButtonClicked()
    {
        HandleNetworkInstance();
        NetworkManager.Singleton.StartHost();
        TransitionToLobby();
    }

    private void OnStartServerButtonClicked()
    {
        HandleNetworkInstance();
        NetworkManager.Singleton.StartServer();
        TransitionToLobby();
    }

    [ServerRpc]
    public void StartGameServerRpc()
    {
        StartMatch();
        StartGameClientRpc();
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        // Client-specific logic here.
    }

    public void StartMatch()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            CurrentGameState = GameState.InGame;
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
    }

    public void OnQuitGameButtonClicked()
    {
        // Logic for quitting the game from the start menu (temp removed)
    }
}
