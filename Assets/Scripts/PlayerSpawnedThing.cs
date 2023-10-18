using UnityEngine;

public class PlayerSpawnedThing : MonoBehaviour
{
    public float lifetime = 5f;
    public GameObject sourcePlayer;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.gameObject != sourcePlayer)
        {
            // Example: Increase the score of sourcePlayer or any other interaction
            sourcePlayer.GetComponent<Player>().IncreaseScore(1);
        }
    }
}
