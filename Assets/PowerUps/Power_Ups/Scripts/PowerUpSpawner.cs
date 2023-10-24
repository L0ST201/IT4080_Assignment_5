using UnityEngine;
using Unity.Netcode;

public class PowerUpSpawner : NetworkBehaviour
{
    [SerializeField] private BasePowerUp powerUp;
    [SerializeField] private float spawnDelay = 3.0f;

    private BasePowerUp spawnedPowerUp;
    private float timeSinceDespawn = 0f;

    private void Start()
    {
        if (IsServer)
        {
            SpawnPowerUp();
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            HandlePowerUpRespawn();
        }
    }

    private void HandlePowerUpRespawn()
    {
        if (spawnedPowerUp == null)
        {
            timeSinceDespawn += Time.deltaTime;

            if (timeSinceDespawn >= spawnDelay)
            {
                SpawnPowerUp();
                ResetDespawnTimer();
            }
        }
    }

    public void SpawnPowerUp()
    {
        if (!powerUp)
        {
            Debug.LogWarning("PowerUp prefab not assigned in PowerUpSpawner!", this);
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        spawnedPowerUp = Instantiate(powerUp, spawnPosition, transform.rotation);

        NetworkObject networkObject = spawnedPowerUp?.GetComponent<NetworkObject>();
        if (networkObject)
        {
            networkObject.Spawn();
        }
        else
        {
            Debug.LogError("Spawned power-up does not have a NetworkObject component!", spawnedPowerUp);
        }
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 pos = transform.position;
        pos.y += 2;
        return pos;
    }

    private void ResetDespawnTimer()
    {
        timeSinceDespawn = 0f;
    }
}
