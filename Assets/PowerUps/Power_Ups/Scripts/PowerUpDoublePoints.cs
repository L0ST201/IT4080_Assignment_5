using System.Collections;
using UnityEngine;

public class PowerUpDoublePoints : BasePowerUp
{
    [SerializeField] private float powerUpDuration = 10f;
    [SerializeField] private float multiplier = 2f;

    protected override bool ApplyToPlayer(Player thePickerUpper)
    {
        thePickerUpper.StartCoroutine(HandleDoublePoints(thePickerUpper));

        return true;
    }

    private IEnumerator HandleDoublePoints(Player player)
    {
        float originalMultiplier = player.scoreMultiplier;

        player.scoreMultiplier = multiplier;

        yield return new WaitForSeconds(powerUpDuration);

        player.scoreMultiplier = originalMultiplier;
    }
}
