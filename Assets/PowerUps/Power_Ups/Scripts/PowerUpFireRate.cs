using UnityEngine;
using System.Collections;

public class PowerUpFireRate : BasePowerUp
{
    [SerializeField] private float timeBetweenBullets = .3f;
    [SerializeField] private float powerUpDuration = 5f; 
    protected override bool ApplyToPlayer(Player thePickerUpper)
    {
        Debug.Log("Applying fire rate power-up...");
        if (thePickerUpper.bulletSpawner.timeBetweenBullets <= timeBetweenBullets)
        {
            return false;
        }

        float originalFireRate = thePickerUpper.bulletSpawner.timeBetweenBullets;
        thePickerUpper.bulletSpawner.timeBetweenBullets = timeBetweenBullets;
        thePickerUpper.StartCoroutine(ResetFireRateAfterDuration(thePickerUpper, originalFireRate));

        return true;
    }

    private IEnumerator ResetFireRateAfterDuration(Player player, float originalFireRate)
    {
        yield return new WaitForSeconds(powerUpDuration);
        player.bulletSpawner.timeBetweenBullets = originalFireRate;
    }
}
