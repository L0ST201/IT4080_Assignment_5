using UnityEngine;

public class PowerUpChangeColor : BasePowerUp
{
    protected override bool ApplyToPlayer(Player thePickerUpper)
    {
        var networkedPlayers = FindObjectOfType<NetworkedPlayers>();

        if (networkedPlayers != null)
        {
            Color randomColor = networkedPlayers.GetRandomAvailableColor();
            thePickerUpper.ChangeColor(randomColor);  // This line sets the new color
            return true;
        }
        else
        {
            Debug.LogError("NetworkedPlayers instance not found.");
            return false;
        }
    }
}
