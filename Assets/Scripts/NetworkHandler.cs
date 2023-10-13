using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NetworkHandler : NetworkBehaviour
{
    public NetworkHelper NetworkHelper;

    [SerializeField]
    private TextMeshProUGUI statusText;

    private NetworkManager _netMgr;

    void Start()
    {
        _netMgr = NetworkManager.Singleton;
        NetworkManager.Singleton.OnClientStarted += OnClientStarted;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    private void Update()
    {
        if (statusText == null) return;

        if (NetworkHelper == null)
        {
            Debug.LogError("NetworkHelper reference is not set in NetworkHandler.");
            return;
        }

        // Update the statusText UI component with latest status
        statusText.text = GetCurrentStatus();
    }

    private string GetCurrentStatus()
    {
        if (IsHost)
        {
            return "I am the Host!";
        }
        else if (_netMgr.IsServer)
        {
            return "I am the Server!";
        }
        else if (_netMgr.IsClient)
        {
            return "I am a Client!";
        }
        return "... Nothing Yet!";
    }

    private void OnClientStarted()
    {
        if (IsHost)
        {
            NetworkHelper.Log("!! Client Started !!", _netMgr, true);
            NetworkHelper.Log($"I AM a Server! {_netMgr.LocalClientId}", _netMgr, true);
            NetworkHelper.Log($"I AM a Host! {_netMgr.LocalClientId}/{_netMgr.ConnectedClients.Count - 1}", _netMgr, true);
            NetworkHelper.Log($"I AM a Client! {_netMgr.LocalClientId}", _netMgr, true);
        }
        else
        {
            NetworkHelper.Log("!! Client Started !!", _netMgr);
            NetworkHelper.Log("I AM Nothing yet", _netMgr);
        }
        SubscribeClientEvents();
    }
   
    private void OnServerStarted()
    {
        NetworkHelper.Log("!! Server Started !!", _netMgr, true);
        NetworkHelper.Log($"I AM a Server! {_netMgr.LocalClientId}", _netMgr, true);
        if (IsHost)
        {
            NetworkHelper.Log($"I AM a Host! {_netMgr.LocalClientId}/{_netMgr.ConnectedClients.Count - 1}", _netMgr, true);
            NetworkHelper.Log($"I AM a Client! {_netMgr.LocalClientId}", _netMgr, true);
        }
        SubscribeServerEvents();
    }

    private void SubscribeClientEvents()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += ClientOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientOnClientDisconnected;
        NetworkManager.Singleton.OnClientStopped += ClientOnClientStopped;
    }

    private void UnsubscribeClientEvents()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= ClientOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= ClientOnClientDisconnected;
        NetworkManager.Singleton.OnClientStopped -= ClientOnClientStopped;
    }

    private void SubscribeServerEvents()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += ServerOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += ServerOnClientDisconnected;
        NetworkManager.Singleton.OnServerStopped += ServerOnServerStopped;
    }

    private void UnsubscribeServerEvents()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= ServerOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= ServerOnClientDisconnected;
        NetworkManager.Singleton.OnServerStopped -= ServerOnServerStopped;
    }

    private void ClientOnClientConnected(ulong clientId)
    {
        if (IsHost)
        {
            NetworkHelper.Log($"Client {clientId} connected to the server", _netMgr, true);
            if (clientId == _netMgr.LocalClientId)
            {
                NetworkHelper.Log($"I have connected {clientId}", _netMgr, true);
            }
        }
        else if (clientId == _netMgr.LocalClientId)
        {
            NetworkHelper.Log($"I have connected {clientId}", _netMgr);
        }
    }

   private void ClientOnClientDisconnected(ulong clientId)
    {
        // If this instance is the host, it should always log client disconnects.
        if (IsHost)
        {
            NetworkHelper.Log($"Client {clientId} disconnected from the server", _netMgr, true);
            return;
        }

        // If it's not the host, only log a message if the disconnecting client is itself.
        if (clientId == _netMgr.LocalClientId)
        {
            NetworkHelper.Log("Disconnected from the server", _netMgr);
        }
    }

    private void ClientOnClientStopped(bool obj)
    {
        if (IsHost)
        {
            NetworkHelper.Log("!! Client Stopped !!", _netMgr, true);
            NetworkHelper.Log($"I AM a Server! {_netMgr.LocalClientId}", _netMgr, true);
            NetworkHelper.Log($"I AM a Host! {_netMgr.LocalClientId}/{_netMgr.ConnectedClients.Count - 1}", _netMgr, true);
            NetworkHelper.Log($"I AM a Client! {_netMgr.LocalClientId}", _netMgr, true);
        }
        else
        {
            NetworkHelper.Log("!! Client Stopped !!", _netMgr);
            NetworkHelper.Log($"I AM a Client! {_netMgr.LocalClientId}", _netMgr);
        }
    }

    private void ServerOnClientConnected(ulong clientId)
    {
        // Placeholder for future logic
    }

    private void ServerOnClientDisconnected(ulong clientId)
    {
        // Placeholder for future logic
    }

    private void ServerOnServerStopped(bool obj)
    {
        NetworkHelper.Log("!! Server Stopped !!", _netMgr, true);
        NetworkHelper.Log($"I AM a Server! {_netMgr.LocalClientId}", _netMgr, true);
        if (IsHost)
        {
            NetworkHelper.Log($"I AM a Host! {_netMgr.LocalClientId}/{_netMgr.ConnectedClients.Count - 1}", _netMgr, true);
            NetworkHelper.Log($"I AM a Client! {_netMgr.LocalClientId}", _netMgr, true);
        }
    }

    public void ShutdownServer()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
