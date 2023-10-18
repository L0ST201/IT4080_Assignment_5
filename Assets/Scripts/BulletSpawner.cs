using UnityEngine;
using Unity.Netcode;

public class BulletSpawner : NetworkBehaviour
{
    [SerializeField] private int bulletDamage = 5;
    [SerializeField] private float shootingRange = 100f;

    [ServerRpc]
    public void PerformHitscanServerRpc(ServerRpcParams rpcParams = default)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, shootingRange))
        {
            // Apply damage if the hit object can take damage
            IDamageable target = hit.transform.GetComponent<IDamageable>();
            if (target != null)
            {
                target.ApplyDamage(bulletDamage);
            }
        }
    }
}
