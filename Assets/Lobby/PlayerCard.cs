using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCard : VisualElement
{
    public new class UxmlFactory : UxmlFactory<PlayerCard> {}

    private Label lblPlayerName;
    private Label lblStatus;
    private VisualElement elemColor;
    private Button btnKick;

    public string playerName = "Not Set";
    public bool ready = false;
    public Color color = Color.magenta;
    public ulong clientId = ulong.MaxValue;

    public event Action<ulong> OnKickClicked;

    public PlayerCard() {}

    public void Init()
    {
        VisualElement topRow = this.Q<VisualElement>("top-row");
        lblPlayerName = topRow.Q<Label>("player-name");
        lblStatus = topRow.Q<Label>("status");
        elemColor = this.Q<VisualElement>("bottom-row").Q<VisualElement>("color");
        btnKick = this.Q<Button>();
        btnKick.clicked += KickButtonClicked;

        UpdateDisplay();
    }

    private void KickButtonClicked()
    {
        OnKickClicked?.Invoke(clientId);
    }

    public void UpdateDisplay()
    {
        string displayClientId = clientId == ulong.MaxValue ? "?" : clientId.ToString();
        lblPlayerName.text = $"({displayClientId}) {playerName}";
        elemColor.style.backgroundColor = color;
        lblStatus.text = ready ? "Ready!!" : "NOT READY";
    }

    public void ShowKick(bool isVisible)
    {
        btnKick.visible = isVisible;
    }
}
