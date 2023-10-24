using System.Collections;
using UnityEngine;

public class PowerUpSpeedUp : BasePowerUp
{
    [SerializeField] private float speedBoost = 12f; 
    [SerializeField] private float powerUpDuration = 10f; 

    protected override bool ApplyToPlayer(Player thePickerUpper)
    {
        thePickerUpper.StartCoroutine(HandleSpeedBoost(thePickerUpper));

        return true;
    }

    private IEnumerator HandleSpeedBoost(Player player)
    {
        float originalSpeed = player.GetMovementSpeed();

        player.SetMovementSpeed(speedBoost);

        yield return new WaitForSeconds(powerUpDuration);

        player.SetMovementSpeed(originalSpeed);
    }
}
