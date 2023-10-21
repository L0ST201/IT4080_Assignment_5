using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCards : VisualElement
{
    private ScrollView list => this.Q<ScrollView>("CardScroll");      
    private VisualTreeAsset playerCardTemplate = Resources.Load<VisualTreeAsset>("PlayerCard");

    public new class UxmlFactory : UxmlFactory<PlayerCards> {}
    public PlayerCards() { }

    public new void Clear()
    {
        list.Clear();        
    }

    public PlayerCard AddCard(string name)
    {
        var templateInst = playerCardTemplate.Instantiate();
        PlayerCard toReturn = templateInst.Q<PlayerCard>();
        list.Add(toReturn);
        toReturn.playerName = name;
        toReturn.Init();
                
        return toReturn;       
    }

    public void UpdatePlayerName(ulong clientId, string newName)
    {
        PlayerCard cardToUpdate = FindCardByClientId(clientId);
        if(cardToUpdate != null)
        {
            cardToUpdate.playerName = newName;
            cardToUpdate.UpdateDisplay();
        }
    }

    public void UpdatePlayerColor(ulong clientId, Color newColor)
    {
        PlayerCard cardToUpdate = FindCardByClientId(clientId);
        if(cardToUpdate != null)
        {
            cardToUpdate.color = newColor;
            cardToUpdate.UpdateDisplay();
        }
    }

    private PlayerCard FindCardByClientId(ulong clientId)
    {
        foreach(PlayerCard card in list.Children())
        {
            if(card.clientId == clientId)
            {
                return card;
            }
        }
        return null;
    }
}
