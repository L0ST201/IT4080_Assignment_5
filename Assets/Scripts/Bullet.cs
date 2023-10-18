using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    public NetworkVariable<int> BulletDamage = new NetworkVariable<int>(3);
    private float bulletSpeed = 20f;

    private void Start()
    {
        // Move the bullet forward at the set speed
        GetComponent<Rigidbody>().velocity = transform.forward * bulletSpeed;

        // Destroy the bullet after 3 seconds to prevent it from existing indefinitely
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Example: Decrease player health when hit by a bullet
        if (other.CompareTag("Player"))
        {
            // Ensure we don't hit the player who shot the bullet (if applicable)
            if (IsOwner)
            {
                return;
            }

            // Decrease health or apply damage to the player hit by the bullet
            other.GetComponent<Player>().ApplyDamage(BulletDamage.Value);

            // Destroy the bullet upon hitting a player
            Destroy(gameObject);
        }
        // Add any other interactions or effects upon the bullet hitting other objects as needed
    }
}
