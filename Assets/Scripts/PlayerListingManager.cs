using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerListingManager : MonoBehaviour
{
    public Transform playerListingParent;
    public GameObject playerListingPrefab;

    private List<PlayerInfo> connectedPlayers = new List<PlayerInfo>();
    private List<GameObject> playerListingUIElements = new List<GameObject>();

    public void AddPlayer(ulong clientId, string playerName, Color playerColor)
    {
        PlayerInfo newPlayer = new PlayerInfo(clientId, playerName, playerColor);
        connectedPlayers.Add(newPlayer);
        
        // Instantiate a new UI element for the player and update its display
        // You can further enhance this to update the UI based on the PlayerInfo
        GameObject newPlayerUI = Instantiate(playerListingPrefab, playerListingParent);
        // Assuming your prefab has a TextMeshPro component to display player's name
        TMP_Text playerNameText = newPlayerUI.GetComponent<TMP_Text>();
        if (playerNameText)
        {
            playerNameText.text = playerName;
            // You can also add logic to display the color if needed
        }
        playerListingUIElements.Add(newPlayerUI);
    }

    public void RemovePlayer(ulong clientId)
    {
        PlayerInfo playerToRemove = connectedPlayers.Find(p => p.ClientId == clientId);
        if (playerToRemove != null)
        {
            connectedPlayers.Remove(playerToRemove);
            
            // Assuming each UI element has the PlayerInfo attached to it for identification
            GameObject uiToRemove = playerListingUIElements.Find(go => go.GetComponent<PlayerInfo>() == playerToRemove);
            if (uiToRemove)
            {
                playerListingUIElements.Remove(uiToRemove);
                Destroy(uiToRemove);
            }
        }
    }

    public void UpdatePlayerName(ulong clientId, string newName)
    {
        PlayerInfo playerToUpdate = connectedPlayers.Find(p => p.ClientId == clientId);
        if (playerToUpdate != null)
        {
            playerToUpdate.PlayerName = newName;
            
            // Update the associated UI element with the new name
            // Similar logic to the RemovePlayer method can be used to find the UI element
        }
    }

    public void UpdatePlayerColor(ulong clientId, Color newColor)
    {
        PlayerInfo playerToUpdate = connectedPlayers.Find(p => p.ClientId == clientId);
        if (playerToUpdate != null)
        {
            playerToUpdate.PlayerColor = newColor;
            
            // Update the associated UI element with the new color
            // Similar logic to the RemovePlayer method can be used to find the UI element
        }
    }
}
