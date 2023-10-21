using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerColorManager : NetworkBehaviour
{
    private static List<Color> availableColors = new List<Color>
    {
        Color.black,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta
    };

    private NetworkVariable<Color> networkedColor = new NetworkVariable<Color>(Color.clear, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    private Renderer headRenderer;
    private Renderer legsRenderer;
    private Renderer torsoRenderer;
    private bool isApplicationQuitting = false;
    private bool hasDespawned = false;

    private bool IsHostPlayer()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId;
    }

    private void Start()
    {
        headRenderer = transform.Find("SK_Soldier_Head").GetComponent<Renderer>();
        legsRenderer = transform.Find("SK_Soldier_Legs").GetComponent<Renderer>();
        torsoRenderer = transform.Find("SK_Soldier_Torso").GetComponent<Renderer>();

        if (!headRenderer || !legsRenderer || !torsoRenderer) 
        {
            Debug.LogError("Renderers not found on player.");
        }

        networkedColor.OnValueChanged += OnColorChanged;

        if (IsOwner && !IsHostPlayer())
        {
            GetComponent<PlayerColorManager>().RequestColorServerRpc();
        }
    }

    private void OnColorChanged(Color oldValue, Color newValue)
    {
        SetPlayerColor(newValue);
    }

    public void SetPlayerColor(Color color)
    {
        if (headRenderer != null) headRenderer.material.color = color;
        if (legsRenderer != null) legsRenderer.material.color = color;
        if (torsoRenderer != null) torsoRenderer.material.color = color;
    }

    [ServerRpc]
    public void RequestColorServerRpc()
    {
        if (OwnerClientId == NetworkManager.ServerClientId)
        {
            networkedColor.Value = Color.white;
        }
        else if (availableColors.Count > 0)
        {
            Color assignedColor = availableColors[0];
            availableColors.RemoveAt(0);
            networkedColor.Value = assignedColor;
        }
        else
        {
            networkedColor.Value = Color.gray;
        }

        // Update the lobby with the player's color.
        LobbyManager lobbyManager = FindObjectOfType<LobbyManager>();
        if (lobbyManager != null && lobbyManager.playerCards != null)
        {
            // Assuming the UpdatePlayerColor function exists in PlayerCards and takes client ID and color as parameters.
            lobbyManager.playerCards.UpdatePlayerColor(OwnerClientId, networkedColor.Value);
        }
    }

    void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    public override void OnNetworkDespawn()
    {
        if (isApplicationQuitting || hasDespawned) 
        {
            return;
        }

        if (IsServer)
        {
            if (availableColors.Count <= 5)
            {
                availableColors.Add(networkedColor.Value);
            }
            networkedColor.Value = Color.gray;
        }

        hasDespawned = true;
    }
}