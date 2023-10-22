using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using UnityEngine;

public class NetworkHelper : MonoBehaviour
{
    public Text statusText;
    public NetworkHandler networkHandler;
    public LobbyManager lobbyManager;
    private NetworkManager _netMgr;
    private UnityTransport _transport;

    private void Start()
    {
        _netMgr = NetworkManager.Singleton;
        _transport = _netMgr.GetComponent<UnityTransport>();
    }

    private void Update()
    {
        if (statusText == null)
            return;

        string transportTypeName = _netMgr.NetworkConfig.NetworkTransport.GetType().Name;
        string serverPort = "?";
        if (_transport != null)
        {
            serverPort = $"{_transport.ConnectionData.Address}:{_transport.ConnectionData.Port}";
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

    public string GetNetworkMode() 
    {
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

    public void Log(NetworkBehaviour what, string msg) 
    {
        ulong ownerId = what.GetComponent<NetworkObject>().OwnerClientId;
        Debug.Log($"[{GetNetworkMode()} {_netMgr.LocalClientId}] [{what.GetType().Name}]: {msg}");
    }
}
