using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCards : VisualElement
{
    private ScrollView list => this.Q<ScrollView>("CardScroll");
    private VisualTreeAsset playerCardTemplate = Resources.Load<VisualTreeAsset>("PlayerCard");

    public new class UxmlFactory : UxmlFactory<PlayerCards> {}

    public PlayerCards() {}

    public new void Clear()
    {
        list.Clear();
    }

    public PlayerCard AddCard(string playerName)
    {
        var cardInstance = playerCardTemplate.Instantiate();
        PlayerCard newCard = cardInstance.Q<PlayerCard>();
        list.Add(newCard);
        newCard.playerName = playerName;
        newCard.Init();

        return newCard;
    }
}
