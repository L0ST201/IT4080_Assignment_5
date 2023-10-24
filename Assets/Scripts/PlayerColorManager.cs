using UnityEngine;
using Unity.Netcode;

public class PlayerColorManager : NetworkBehaviour
{
    // Network Variables
    private readonly NetworkVariable<Color> _networkedColor = new NetworkVariable<Color>(
        Color.clear, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    // Renderer references
    private Renderer _headRenderer;
    private Renderer _legsRenderer;
    private Renderer _torsoRenderer;

    // State management
    private bool _isApplicationQuitting;
    private bool _hasDespawned;

    private void Start()
    {
        InitializeRenderers();
        _networkedColor.OnValueChanged += OnColorChanged;

        if (IsOwner)
        {
            RequestColorServerRpc();
        }
    }

    private void InitializeRenderers()
    {
        _headRenderer = transform.Find("SK_Soldier_Head")?.GetComponent<Renderer>();
        _legsRenderer = transform.Find("SK_Soldier_Legs")?.GetComponent<Renderer>();
        _torsoRenderer = transform.Find("SK_Soldier_Torso")?.GetComponent<Renderer>();

        if (_headRenderer == null) Debug.LogError("Head Renderer not found on player.");
        if (_legsRenderer == null) Debug.LogError("Legs Renderer not found on player.");
        if (_torsoRenderer == null) Debug.LogError("Torso Renderer not found on player.");
    }

    private void OnColorChanged(Color oldValue, Color newValue)
    {
        Debug.Log($"OnColorChanged triggered for {NetworkManager.LocalClientId}. Old Value: {oldValue}, New Value: {newValue}");
        SetPlayerColor(newValue);
        UpdatePlayerColorClientRpc(NetworkManager.LocalClientId, newValue);
    }

    public void SetPlayerColor(Color color)
    {
        if (!_headRenderer || !_legsRenderer || !_torsoRenderer) 
        {
            Debug.LogError("Renderers not found on player.");
            return;
        }

        Debug.Log($"Actually setting color to: {color}");
        _headRenderer.material.color = color;
        _legsRenderer.material.color = color;
        _torsoRenderer.material.color = color;
    }

    public void SetPlayerColorDirectly(Color color)
    {
        SetPlayerColor(color);
    }

        public void UpdateNetworkedColor(Color newColor)
    {
        _networkedColor.Value = newColor;
    }

    [ServerRpc]
    public void RequestColorServerRpc()
    {
        var networkedPlayers = FindObjectOfType<NetworkedPlayers>();
        Color newColor;

        if (networkedPlayers != null)
        {
            newColor = networkedPlayers.AssignColorToClient(OwnerClientId);
            Debug.Log($"Server assigning color {newColor} to client {OwnerClientId}");
            _networkedColor.Value = newColor;
        }
        else
        {
            Debug.LogError("NetworkedPlayers instance not found on the server.");
        }
    }

    [ClientRpc]
    public void UpdatePlayerColorClientRpc(ulong clientId, Color color)
    {
        Debug.Log($"Client {NetworkManager.LocalClientId} received color update RPC. ClientID: {clientId}, Color: {color}");
        if (NetworkManager.LocalClientId != clientId)
        {
            Debug.Log($"Setting color for client {clientId} to {color}");
            SetPlayerColor(color);
        }
    }

    private void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
    }

    public override void OnNetworkDespawn()
    {
        if (_isApplicationQuitting || _hasDespawned) 
        {
            return;
        }

        if (IsServer)
        {
            ReturnColorToPool();
        }

        _hasDespawned = true;
    }

    private void ReturnColorToPool()
    {
        if (_networkedColor == null)
        {
            Debug.LogError("Networked color is not initialized.");
            return;
        }

        var networkedPlayers = FindObjectOfType<NetworkedPlayers>();
        if (networkedPlayers != null && IsServer)
        {
            networkedPlayers.ReturnColor(_networkedColor.Value);
            _networkedColor.Value = Color.gray;
        }
    }
}
