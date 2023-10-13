using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ChatServer : NetworkBehaviour
{
    public ChatUi chatUi;
    private const ulong SYSTEM_ID = ulong.MaxValue;
    private ulong[] dmClientIds = new ulong[2];

    private readonly Color COLOR_SYSTEM = Color.green;
    private readonly Color COLOR_USER_SELF = Color.magenta;
    private readonly Color COLOR_SERVER = Color.green;
    private readonly Color COLOR_ERROR = Color.red;

    void Start()
    {
        InitializeChatServer();
    }

    private void InitializeChatServer()
    {
        chatUi.printEnteredText = false;
        chatUi.MessageEntered += OnChatUiMessageEntered;

        if (IsServer)
        {
            RegisterServerEventHandlers();
            DisplayServerOrHostMessage();
        }
        else
        {
            DisplayClientMessage();
        }
    }

    private void RegisterServerEventHandlers()
    {
        NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
        NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnect;
    }

    private void DisplayServerOrHostMessage()
    {
        string message = IsHost 
            ? $"Welcome, you are the host AND client {NetworkManager.LocalClientId}" 
            : "You are the server";
        DisplayMessageLocally(SYSTEM_ID, message, false, COLOR_SYSTEM);
    }

    private void DisplayClientMessage()
    {
        DisplayMessageLocally(SYSTEM_ID, $"Welcome to the server, you are client {NetworkManager.LocalClientId}", false, COLOR_SYSTEM);
    }

    private void ServerOnClientConnected(ulong clientId)
    {
        SendGlobalMessageExcept($"Player {clientId} has connected to the server.", clientId);
    }

    private void ServerOnClientDisconnect(ulong clientId)
    {
        SendGlobalMessage($"Player {clientId} has disconnected from the server.");
    }

    private void DisplayMessageLocally(ulong from, string message, bool isWhisper = false, Color overrideColor = default)
    {
        string fromStr;
        Color textColor;

        if (from == SYSTEM_ID)
        {
            fromStr = "SYS";
            textColor = COLOR_SYSTEM;
        }
        else if (from == NetworkManager.LocalClientId)
        {
            fromStr = isWhisper ? $"You whispered" : "you";
            textColor = COLOR_USER_SELF;
        }
        else
        {
            fromStr = isWhisper ? $"Player {from} whispered you" : $"Player {from}";
            textColor = chatUi.defaultTextColor;
        }

        if (overrideColor != Color.clear) 
        {
            textColor = overrideColor;
        }

        chatUi.addEntry(fromStr, message, textColor);
    }

    private void OnChatUiMessageEntered(string message)
    {
          if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        SendChatMessageServerRpc(message);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default)
    {
        if (message.StartsWith("@"))
        {
            HandleDirectMessage(message, serverRpcParams.Receive.SenderClientId);
        }
        else
        {
            ReceiveChatMessageClientRpc(message, serverRpcParams.Receive.SenderClientId);
        }
    }

    private void HandleDirectMessage(string message, ulong senderClientId)
    {
        string[] parts = message.Split(" ");
        string clientIdStr = parts[0].Replace("@", "");
        if (ulong.TryParse(clientIdStr, out ulong toClientId))
        {
            if (toClientId == senderClientId)
            {
                SendChatNotificationServerRpc("You cannot whisper to yourself.", senderClientId, COLOR_ERROR);
                return;
            }
            
            if (NetworkManager.Singleton.ConnectedClients.ContainsKey(toClientId))
            {
                string whisperMessage = string.Join(" ", parts, 1, parts.Length - 1);
                ServerSendDirectMessage(whisperMessage, senderClientId, toClientId);
            }
            else
            {
                SendChatNotificationServerRpc($"The message could not be sent. Player {toClientId} is not connected.", senderClientId, COLOR_ERROR);
            }
        }
        else
        {
            SendChatNotificationServerRpc($"Invalid client ID: {clientIdStr}", senderClientId, COLOR_ERROR);
        }
    }

    private void ServerSendDirectMessage(string message, ulong from, ulong to)
    {
        dmClientIds[0] = from;
        dmClientIds[1] = to;
        ClientRpcParams rpcParams = new ClientRpcParams
        {
            Send = { TargetClientIds = dmClientIds }
        };

        ReceiveWhisperMessageClientRpc(message, from, to, rpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendChatNotificationServerRpc(string message, ulong targetClientId, Color color = default, ServerRpcParams serverRpcParams = default)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = { TargetClientIds = new ulong[] { targetClientId } }
        };
        ReceiveChatMessageClientRpc(message, SYSTEM_ID, color, clientRpcParams);
    }

    [ClientRpc]
    public void ReceiveChatMessageClientRpc(string message, ulong from, Color color = default, ClientRpcParams clientRpcParams = default)
    {
        DisplayMessageLocally(from, message, false, color);
    }

    [ClientRpc]
    public void ReceiveWhisperMessageClientRpc(string message, ulong from, ulong to, ClientRpcParams clientRpcParams = default)
    {
        if (NetworkManager.LocalClientId == to)
        {
            DisplayMessageLocally(from, message, isWhisper: true);
        }
        else if (NetworkManager.LocalClientId == from)
        {
            DisplayWhisperedMessage(to, message);
        }
    }

    private void DisplayWhisperedMessage(ulong to, string message)
    {
        string toStr = $"You whispered to Player {to}";
        chatUi.addEntry(toStr, message, COLOR_USER_SELF);
    }

    private void SendGlobalMessage(string message)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            ReceiveChatMessageClientRpc(message, SYSTEM_ID, Color.clear, new ClientRpcParams { Send = { TargetClientIds = new ulong[] { client.Value.ClientId } } });
        }
    }

    private void SendGlobalMessageExcept(string message, ulong exceptionClientId)
    {
        List<ulong> targetClientIds = new List<ulong>();
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            if (client.Value.ClientId != exceptionClientId)
            {
                targetClientIds.Add(client.Value.ClientId);
            }
        }
        ReceiveChatMessageClientRpc(message, SYSTEM_ID, Color.clear, new ClientRpcParams { Send = { TargetClientIds = targetClientIds.ToArray() } });
    }

    private new void OnDestroy()
    {
        if (chatUi != null)
        {
            chatUi.MessageEntered -= OnChatUiMessageEntered;
        }
        if (IsServer && NetworkManager != null)
        {
            NetworkManager.OnClientConnectedCallback -= ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= ServerOnClientDisconnect;
        }
    }
}
