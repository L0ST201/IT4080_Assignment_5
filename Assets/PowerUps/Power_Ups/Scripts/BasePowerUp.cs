using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(SphereCollider))]
public abstract class BasePowerUp : NetworkBehaviour
{
    private void Awake()
    {
        SphereCollider collider = GetComponent<SphereCollider>();
        collider.isTrigger = true; // Set the collider to be a trigger so it doesn't physically stop the player
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            Player player = other.GetComponent<Player>();
            if (player)
            {
                ServerPickUp(player);
            }
        }
    }

    public void ServerPickUp(Player thePickerUpper)
    {
        if (IsServer && ApplyToPlayer(thePickerUpper))
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    protected abstract bool ApplyToPlayer(Player thePickerUpper);
}
