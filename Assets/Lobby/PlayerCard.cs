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

    public PlayerCard() { }

    public void Init()
    {
        VisualElement tr = this.Q<VisualElement>("top-row");
        lblPlayerName = tr.Q<Label>("player-name");
        lblStatus = tr.Q<Label>("status");
        btnKick = this.Q<Button>();
        btnKick.clicked += BtnKick_clicked;
        elemColor = this.Q<VisualElement>("bottom-row").Q<VisualElement>("color");
        UpdateDisplay();
    }

    private void BtnKick_clicked()
    {
        OnKickClicked?.Invoke(clientId);
    }

    public void UpdateDisplay()
    {
        string strClientId = clientId.ToString();
        if(clientId == ulong.MaxValue)
        {
            strClientId = "?";
        }
        lblPlayerName.text = $"({strClientId}) {playerName}";
        elemColor.style.backgroundColor = color;
        if (ready)
        {
            lblStatus.text = "Ready!!";
        }
        else
        {
            lblStatus.text = "NOT READY";
        }
    }

    public void ShowKick(bool should)
    {
        btnKick.visible = should;
    }
}
