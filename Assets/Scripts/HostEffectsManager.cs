using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HostEffectsManager : NetworkBehaviour
{
    [SerializeField]
    private ParticleSystem hostParticleEffectPrefab;

    private ParticleSystem instantiatedEffect;

    private void Start()
    {
        if (IsHostPlayer())
        {
            var effectInstance = Instantiate(hostParticleEffectPrefab, transform.position, Quaternion.identity);

            var networkObject = effectInstance.GetComponent<NetworkObject>();
            if (networkObject)
            {
                networkObject.Spawn();
            }

            effectInstance.transform.SetParent(transform, true);
            instantiatedEffect = effectInstance.GetComponent<ParticleSystem>();
            instantiatedEffect.Play();
        }
    }

    private bool IsHostPlayer()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId;
    }
}
