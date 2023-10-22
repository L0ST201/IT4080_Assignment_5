using UnityEngine;
using Unity.Netcode;

public class Arena1Game : NetworkBehaviour
{
    [Header("References")]
    public NetworkedPlayers networkedPlayers;

    [Header("Prefabs")]
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private Player _playerWithAuraPrefab;

    [Header("Game Elements")]
    [SerializeField] private Camera _arenaCamera;

    private int _positionIndex = 0;
    private static readonly Vector3[] _startPositions = 
        {
            new Vector3(4, 0, 0),
            new Vector3(-4, 0, 0),
            new Vector3(0, 0, 4),
            new Vector3(0, 0, -4)
        };

    void Start()
    {
        networkedPlayers = FindObjectOfType<NetworkedPlayers>();
        if (!networkedPlayers)
        {
            Debug.LogError("NetworkedPlayers object not found in the scene!");
            return;
        }

        SetCameraAndListenerState();

        if (NetworkManager.Singleton.IsServer)
        {
            SpawnPlayers();
        }
    }

    private void SetCameraAndListenerState()
    {
        _arenaCamera.enabled = !IsClient;
        _arenaCamera.GetComponent<AudioListener>().enabled = !IsClient;
    }

    private Vector3 NextPosition() 
    {
        Vector3 pos = _startPositions[_positionIndex];
        _positionIndex = (_positionIndex + 1) % _startPositions.Length;
        return pos;
    }

    private void SpawnPlayers()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("SpawnPlayers should only be called on the server.");
            return;
        }

        if (!_playerPrefab || !_playerWithAuraPrefab)
        {
            Debug.LogError("Player prefabs are not assigned.");
            return;
        }

         foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Vector3 spawnPosition = NextPosition();
            
            Player playerSpawn;
            if (clientId == NetworkManager.ServerClientId)
            {
                playerSpawn = Instantiate(_playerWithAuraPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                playerSpawn = Instantiate(_playerPrefab, spawnPosition, Quaternion.identity);
            }

            NetworkObject networkObject = playerSpawn.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(clientId);

            var playerColorManager = playerSpawn.GetComponent<PlayerColorManager>();
            if (playerColorManager)
            {
                int idx = networkedPlayers.FindPlayerIndex(clientId);
                if (idx != -1)
                {
                    Color assignedColor = networkedPlayers.allNetPlayers[idx].color;
                    playerColorManager.SetPlayerColorDirectly(assignedColor);
                }
            }
        }
    }
}
