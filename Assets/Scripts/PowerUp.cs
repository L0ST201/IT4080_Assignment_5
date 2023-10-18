using UnityEngine;

public class PowerUp : MonoBehaviour
{
    private PowerUpSpawner spawner;

    private void Start()
    {
        spawner = FindObjectOfType<PowerUpSpawner>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyEffect(other.gameObject);
            Destroy(gameObject);
            spawner.RespawnPowerUp();
        }
    }

    public virtual void ApplyEffect(GameObject player)
    {
        // Logic to apply effect to the player
    }
}
