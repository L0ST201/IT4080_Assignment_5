using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [SerializeField] private GameObject powerUpPrefab;
    [SerializeField] private GameObject secondPowerUpPrefab;
    [SerializeField] private float respawnTime = 10f;

    // Define the range for random spawning
    private float spawnAreaXMin = -2f;
    private float spawnAreaXMax = 2f;
    private float spawnAreaZMin = -2f;
    private float spawnAreaZMax = 2f;
    private float spawnHeightY = .6f;

    private void Start()
    {
        SpawnPowerUp();
    }

    public void SpawnPowerUp()
    {
        // Randomly select a power-up type to spawn
        GameObject toSpawn = Random.value > 0.5f ? powerUpPrefab : secondPowerUpPrefab;

        // Generate a random position within the defined range
        Vector3 randomPosition = new Vector3(
            Random.Range(spawnAreaXMin, spawnAreaXMax),
            spawnHeightY,
            Random.Range(spawnAreaZMin, spawnAreaZMax)
        );

        // Instantiate the power-up at the random position
        GameObject spawnedPowerUp = Instantiate(toSpawn, randomPosition, Quaternion.identity);
        
        // Scale down to 20% original size
        spawnedPowerUp.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        
        // Set the collider to be a trigger so players can pass through
        Collider powerUpCollider = spawnedPowerUp.GetComponent<Collider>();
        if(powerUpCollider != null)
        {
            powerUpCollider.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("No collider found on the spawned power-up. Players might not be able to interact with it.");
        }
    }

    public void RespawnPowerUp()
    {
        Invoke("SpawnPowerUp", respawnTime);
    }
}
