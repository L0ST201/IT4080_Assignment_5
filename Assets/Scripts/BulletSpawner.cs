using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletSpawner : NetworkBehaviour
{
    public GameObject BulletPrefab;
    public float bulletSpeed = 80f;
    public Transform bulletSpawnPoint;  
    public float timeBetweenBullets = 0.5f;

    [ServerRpc]
    public void RequestFireServerRpc()
    {
        Debug.Log("Fire request received on server.");
        FireBullet();
    }

    private void FireBullet()
{
    Debug.Log("Instantiating bullet");
    GameObject newBullet = Instantiate(BulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
    Rigidbody rb = newBullet.GetComponent<Rigidbody>();
    if (rb)
    {
        rb.velocity = bulletSpawnPoint.forward * bulletSpeed;
    }
    else
    {
        Debug.LogError("Bullet Rigidbody not found");
    }
    Destroy(newBullet, 3f);
}

}
