using System;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyUi : MonoBehaviour
{
    // Events
    public event Action<bool> OnReadyToggled;
    public event Action OnStartClicked;
    public event Action<string> OnChangeNameClicked;

    // UI Elements
    public PlayerCards playerCards;

    private VisualElement root;
    private VisualElement playerControls;
    private Button btnStart;
    private Toggle tglReady;
    private TextField txtPlayerName;
    private Button btnChangeName;

    private void Awake() 
    {
        InitializeUIElements();
    }

    private void InitializeUIElements() 
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        playerControls = root.Q<VisualElement>("right").Q<VisualElement>("player-controls");
        btnStart = playerControls.Q<Button>("Start");
        tglReady = playerControls.Q<Toggle>("Ready");
        txtPlayerName = playerControls.Q<TextField>("player-name");
        btnChangeName = playerControls.Q<Button>("change-name");
        playerCards = root.Q<PlayerCards>();

        RegisterUIEvents();
    }

    private void RegisterUIEvents() 
    {
        btnStart.clicked += OnStartButtonClicked;
        btnChangeName.clicked += OnChangeNameButtonClicked;
        tglReady.RegisterValueChangedCallback(evt => OnReadyToggled?.Invoke(evt.newValue));
    }

    private void OnChangeNameButtonClicked() 
    {
        OnChangeNameClicked?.Invoke(txtPlayerName.value);
    }

    private void OnStartButtonClicked() 
    {
        OnStartClicked?.Invoke();
    }

    public void ShowStart(bool shouldShow) 
    {
        btnStart.visible = shouldShow;
        tglReady.visible = !shouldShow;
    }

    public void SetPlayerName(string playerName) 
    {
        txtPlayerName.value = playerName;
    }

    public void EnableStart(bool shouldEnable) 
    {
        btnStart.SetEnabled(shouldEnable);
    }
}
