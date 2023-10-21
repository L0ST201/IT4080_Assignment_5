using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using UnityEngine;

public class NetworkHelper : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public Button serverButton;
    public Button shutdownButton;
    public Text statusText;

    public NetworkHandler networkHandler;
    public LobbyManager lobbyManager;
    private NetworkManager _netMgr;

    private void Start()
    {
         _netMgr = NetworkManager.Singleton;

        if (hostButton != null)
            hostButton.onClick.AddListener(() => _netMgr.StartHost());
        if (clientButton != null)
            clientButton.onClick.AddListener(() => _netMgr.StartClient());
        if (serverButton != null)
            serverButton.onClick.AddListener(() => _netMgr.StartServer());

        if (shutdownButton != null)
        {
            shutdownButton.onClick.AddListener(() => 
            {
                if (networkHandler != null)
                {
                    networkHandler.ShutdownServer();
                }
                else
                {
                    _netMgr.Shutdown();
                }

                if (lobbyManager != null)
                {
                    lobbyManager.OnQuitGameButtonClicked();
                }
            });
        }
    }

    private void Update()
    {
        UpdateButtonVisibility();
        UpdateStatusText();
    }

    private void UpdateButtonVisibility()
    {
        if (NetworkManager.Singleton != null && hostButton != null && clientButton != null && serverButton != null && shutdownButton != null)
        {
            if (!_netMgr.IsClient && !_netMgr.IsServer)
            {
                hostButton.gameObject.SetActive(true);
                clientButton.gameObject.SetActive(true);
                serverButton.gameObject.SetActive(true);
                shutdownButton.gameObject.SetActive(false);
            }
            else
            {
                hostButton.gameObject.SetActive(false);
                clientButton.gameObject.SetActive(false);
                serverButton.gameObject.SetActive(false);
                shutdownButton.gameObject.SetActive(true);
            }
        }
    }

    private void UpdateStatusText()
    {
        if (statusText == null)
            return;

        string transportTypeName = _netMgr.NetworkConfig.NetworkTransport.GetType().Name;
        UnityTransport transport = _netMgr.GetComponent<UnityTransport>();
        string serverPort = "?";
        if (transport != null)
        {
            serverPort = $"{transport.ConnectionData.Address}:{transport.ConnectionData.Port}";
        }

        string mode = "Client";
        if (_netMgr.IsHost)
        {
            mode = "Host";
        }
        else if (_netMgr.IsServer)
        {
            mode = "Server";
        }

        if (_netMgr.IsClient)
        {
            statusText.text = $"Transport: {transportTypeName} [{serverPort}]\nMode: {mode}\nClientId = {_netMgr.LocalClientId}";
        }
        else
        {
            statusText.text = $"Transport: {transportTypeName} [{serverPort}]\nMode: {mode}";
        }
    }

    public string GetNetworkMode() {
        string type = "Client";
        if(_netMgr.IsHost) {
            type = "Host";
        } else if(_netMgr.IsServer) {
            type = "Server";
        }
        return type;
    }

    public static void Log(string message, NetworkManager netMgr, bool isHost = false)
    {
        if (isHost)
        {
            UnityEngine.Debug.Log($"[Host {netMgr.LocalClientId}]: {message}");
        }
        else
        {
            UnityEngine.Debug.Log($"[client {netMgr.LocalClientId}]:  {message}");
        }
    }

    public void Log(NetworkBehaviour what, string msg) {
        ulong ownerId = what.GetComponent<NetworkObject>().OwnerClientId;
        Debug.Log($"[{GetNetworkMode()} {_netMgr.LocalClientId}] [{what.GetType().Name}]: {msg}");
    }
}