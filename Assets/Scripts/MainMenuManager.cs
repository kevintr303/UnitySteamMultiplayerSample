using System.Collections;
using System.Collections.Generic;
using FishNet;
using Steamworks;
using TMPro;
using UnityEngine;

/// <summary>
/// This manages the main menu, its interface, the multiplayer lobbies, and the transition to the game scene.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private GameObject mainMenu, multiplayerMenu, lobbyJoinMenu, lobbyMenu, lobbySettingsMenu;
    [SerializeField] private TextMeshProUGUI lobbyTitle;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private GameObject lobbySettingsButton;
    [SerializeField] private TMP_Dropdown lobbyTypeDropdown;
    [SerializeField] private TMP_Dropdown memberLimitDropdown;

    [Header("Lobby variables")] 
    [SerializeField] private GameObject lobbyView;
    [SerializeField] private GameObject lobbyItemPrefab;
    [SerializeField] private GameObject lobbyListContent;

    private List<GameObject> _lobbyItemList = new List<GameObject>();
    protected Callback<LobbyEnter_t> LobbyEntered;

    private void Start()
    {
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        OpenMainMenu();
    }

    #region Public Methods

    /// <summary>
    /// Creates a new lobby.
    /// </summary>
    /// <remarks>Default settings are invite only, and 4 player capacity.</remarks>
    public void CreateLobby()
    {
        SteamManager.Instance.CreateLobby();
    }

    /// <summary>
    /// Leaves the current lobby and opens the main menu.
    /// </summary>
    public void LeaveLobby()
    {
        SteamManager.Instance.LeaveLobby();
        OpenMainMenu();
    }

    /// <summary>
    /// Starts the game either offline or online.
    /// </summary>
    /// <param name="isOffline">If true, starts the game offline. Otherwise, starts the game online.</param>
    public void StartGame(bool isOffline)
    {
        if (isOffline)
        {
            SceneLoader.Instance.LoadScene(gameSceneName, new List<string> { "MainMenu" });
        }
        else
        {
            NetworkSceneLoader.Instance.LoadScene(gameSceneName, new List<string> { "MainMenu" });
        }
    }

    /// <summary>
    /// Sets the type of the lobby.
    /// </summary>
    /// <param name="lobbyType">The type of the lobby (0 for private, 1 for friends only, etc.).</param>
    public void SetLobbyType(int lobbyType)
    {
        SteamManager.Instance.SetLobbyType((ELobbyType)lobbyType);
    }

    /// <summary>
    /// Sets the member limit for the lobby.
    /// </summary>
    /// <remarks>This should only be called using the memberLimitDropdown OnValueChanged event.</remarks>
    public void SetLobbyMemberLimit(int maxMembers)
    {
        SteamManager.Instance.SetLobbyMemberLimit(maxMembers + 2); 
    }

    /// <summary>
    /// Opens the lobby join menu and refreshes the lobby list.
    /// </summary>
    public void OpenLobbyJoin()
    {
        CloseAllScreens();
        lobbyJoinMenu.SetActive(true);
        RefreshLobbyList();
    }

    /// <summary>
    /// Refreshes the lobby list by retrieving the latest lobbies and updating the UI.
    /// </summary>
    public async void RefreshLobbyList()
    {
        DestroyLobbyItems();

        List<CSteamID> lobbyList = await SteamManager.Instance.GetLobbyList(true);
        foreach (var steamID in lobbyList)
        {
            var lobbyObject = Instantiate(lobbyItemPrefab, lobbyListContent.transform);
            var lobbyItem = lobbyObject.GetComponent<LobbyItem>();
            lobbyItem.SetLobbyData(steamID, SteamManager.Instance.GetLobbyData(steamID, "name"), 
                SteamManager.Instance.GetNumLobbyMembers(steamID), SteamManager.Instance.GetLobbyMemberLimit(steamID));
            _lobbyItemList.Add(lobbyObject);
        }
    }

    /// <summary>
    /// Closes the application.
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Opens the main menu.
    /// </summary>
    private void OpenMainMenu()
    {
        CloseAllScreens();
        mainMenu.SetActive(true);
    }

    /// <summary>
    /// Opens the lobby menu.
    /// </summary>
    private void OpenLobby()
    {
        CloseAllScreens();
        lobbyMenu.SetActive(true);
    }

    /// <summary>
    /// Destroys all lobby items in the UI.
    /// </summary>
    private void DestroyLobbyItems()
    {
        foreach (var lobbyItem in _lobbyItemList)
        {
            Destroy(lobbyItem);
        }
        _lobbyItemList.Clear();
    }

    /// <summary>
    /// Closes all menu screens.
    /// </summary>
    private void CloseAllScreens()
    {
        mainMenu.SetActive(false);
        multiplayerMenu.SetActive(false);
        lobbyMenu.SetActive(false);
        lobbyJoinMenu.SetActive(false);
        lobbySettingsMenu.SetActive(false);
    }

    /// <summary>
    /// Called when a lobby is entered. Opens the lobby menu and updates UI elements.
    /// </summary>
    /// <param name="callback">The callback data for the lobby enter event.</param>
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        OpenLobby();
        lobbyTitle.text = SteamManager.Instance.GetLobbyData(SteamManager.Instance.GetCurrentLobbyID(),"name");
        lobbyTypeDropdown.value = 0;
        memberLimitDropdown.value = 2;
        startGameButton.SetActive(false);
        lobbySettingsButton.SetActive(false);
        StartCoroutine(CheckHostStarted());
    }

    /// <summary>
    /// Coroutine that checks if the host has started and enables the start game button.
    /// </summary>
    private IEnumerator CheckHostStarted()
    {
        while (!InstanceFinder.NetworkManager.IsHostStarted)
        {
            yield return null;
        }

        startGameButton.SetActive(true);
        lobbySettingsButton.SetActive(true);
    }

    #endregion
}
