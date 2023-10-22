using UnityEngine;
using Unity.Netcode;

public class BulletSpawner : NetworkBehaviour
{
    public Rigidbody BulletPrefab;
    private float bulletSpeed = 80f;
    public float timeBetweenBullets = 0.5f;
    private float shotCountDown = 0f;

    private void Update() 
    {
        UpdateShotCooldown();
    }

    private void UpdateShotCooldown() 
    {
        if (IsServer && shotCountDown > 0) 
        {
            shotCountDown -= Time.deltaTime;    
        }
    }

    [ServerRpc]
    public void FireServerRpc(ServerRpcParams rpcParams = default)
    {
        if (CanFire()) 
        {
            SpawnBullet(rpcParams);
        }
    }

    private bool CanFire() 
    {
        return shotCountDown <= 0;
    }

    private void SpawnBullet(ServerRpcParams rpcParams) 
    {
        Rigidbody newBullet = Instantiate(BulletPrefab, transform.position, transform.rotation);
        newBullet.velocity = transform.forward * bulletSpeed;
        
        NetworkObject networkObject = newBullet.gameObject.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
        }
        
        Destroy(newBullet.gameObject, 3);
        shotCountDown = timeBetweenBullets;
    }
}
