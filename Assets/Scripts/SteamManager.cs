using System.Collections.Generic;
using System.Threading.Tasks;
using FishNet;
using FishNet.Managing;
using Steamworks;
using UnityEngine;

/// <summary>
/// This handles all the steam-related callbacks and anything Steamworks, really.
/// </summary>
public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance { get; private set; }

    private NetworkManager _networkManager;
    private FishySteamworks.FishySteamworks _fishySteamworks;
    private ulong _currentLobbyID;

    private Callback<LobbyCreated_t> _lobbyCreated;
    private Callback<GameLobbyJoinRequested_t> _joinRequested;
    private Callback<LobbyEnter_t> _lobbyEntered;
    private Callback<LobbyMatchList_t> _lobbyListRequested;

    private ELobbyType _lobbyType = ELobbyType.k_ELobbyTypePrivate;
    private int _memberLimit = 4;
    private List<CSteamID> _lobbyList = new List<CSteamID>();
    private TaskCompletionSource<bool> _lobbyListTaskCompletionSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        _networkManager = InstanceFinder.NetworkManager;
        _fishySteamworks = _networkManager.GetComponent<FishySteamworks.FishySteamworks>();
    }

    private void Start()
    {
        _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        _joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
        _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        _lobbyListRequested = Callback<LobbyMatchList_t>.Create(OnLobbyListRequested);
    }

    #region Public Methods

    /// <summary>
    /// Creates a new matchmaking lobby.
    /// </summary>
    /// <remarks>You may want to update the lobby type (using SetLobbyType) and the member limit (using SetLobbyMemberLimit).</remarks>
    public void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(_lobbyType, _memberLimit);
    }

    /// <summary>
    /// Leave a lobby that the user is currently in.
    /// </summary>
    /// <remarks>If the host of the server has left, then it will also shut down the server.</remarks>
    public void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(new CSteamID(_currentLobbyID));
        _currentLobbyID = 0;

        _fishySteamworks.StopConnection(false);
        if (_networkManager.IsServerStarted)
        {
            _fishySteamworks.StopConnection(true);
        }
    }

    /// <summary>
    /// Joins an existing lobby.
    /// </summary>
    /// <param name="lobbyID">The Steam ID of the lobby to join.</param>
    public void JoinLobby(CSteamID lobbyID)
    {
        SteamMatchmaking.JoinLobby(lobbyID);
    }

    /// <summary>
    /// Gets the metadata associated with the specified key from the specified lobby.
    /// </summary>
    /// <param name="lobbyID">The Steam ID of the lobby to get the metadata from.</param>
    /// <param name="key">The key to get the value of.</param>
    /// <returns>
    /// Returns the metadata associated with the specified key from the specified lobby.
    /// Returns an empty string if no value is set for this key, or the specified lobby does not exist.
    /// </returns>
    public string GetLobbyData(CSteamID lobbyID, string key)
    {
        return SteamMatchmaking.GetLobbyData(lobbyID, key);
    }

    /// <summary>
    /// Sets a key/value pair in the current lobby metadata.
    /// </summary>
    /// <param name="key">The key to set the data for.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>
    /// true if the data has been successfully set.
    /// false if there was an issue.
    /// </returns>
    public bool SetLobbyData(string key, string value)
    {
        bool setLobbyData = SteamMatchmaking.SetLobbyData(new CSteamID(_currentLobbyID), key, value);
        if (!setLobbyData)
        {
            Debug.LogError("SteamManager: Unable to set lobby data.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Updates what type of lobby the current lobby is.
    /// </summary>
    /// <param name="eLobbyType">The new lobby type that will be set.</param>
    /// <returns>
    /// true if the lobby type has been successfully set.
    /// false if there was an issue (this can only be set by the lobby owner).
    /// </returns>
    public bool SetLobbyType(ELobbyType eLobbyType)
    {
        bool setLobbyType = SteamMatchmaking.SetLobbyType(new CSteamID(_currentLobbyID), eLobbyType);
        if (!setLobbyType)
        {
            Debug.LogError("SteamManager: Unable to set lobby type.");
            return false;
        }

        _lobbyType = eLobbyType;
        return true;
    }

    /// <summary>
    /// Set the maximum number of players that can join the current lobby.
    /// </summary>
    /// <param name="maxMembers">The maximum number of players allowed in the current lobby.</param>
    /// <returns>
    /// true if the limit was successfully set.
    /// false if there was an issue (this can only be set by the lobby owner).
    /// </returns>
    public bool SetLobbyMemberLimit(int maxMembers)
    {
        bool setLobbyMemberLimit = SteamMatchmaking.SetLobbyMemberLimit(new CSteamID(_currentLobbyID), maxMembers);
        if (!setLobbyMemberLimit)
        {
            Debug.LogError("SteamManager: Unable to set lobby member limit.");
            return false;
        }

        _memberLimit = maxMembers;
        return true;
    }

    /// <summary>
    /// Gets the maximum number of players that can join the specified lobby.
    /// </summary>
    /// <param name="lobbyID">The Steam ID of the lobby.</param>
    /// <returns>The maximum number of players allowed in the specified lobby.</returns>
    public int GetLobbyMemberLimit(CSteamID lobbyID)
    {
        return SteamMatchmaking.GetLobbyMemberLimit(lobbyID);
    }

    /// <summary>
    /// Gets the number of members currently in the specified lobby.
    /// </summary>
    /// <param name="lobbyID">The Steam ID of the lobby.</param>
    /// <returns>The number of members currently in the specified lobby.</returns>
    public int GetNumLobbyMembers(CSteamID lobbyID)
    {
        return SteamMatchmaking.GetNumLobbyMembers(lobbyID);
    }

    /// <summary>
    /// Get the current lobby ID.
    /// </summary>
    /// <returns>The current lobby ID as a CSteamID.</returns>
    public CSteamID GetCurrentLobbyID()
    {
        return new CSteamID(_currentLobbyID);
    }

    /// <summary>
    /// Retrieves the lobby list.
    /// </summary>
    /// <param name="refresh">If true, will refresh the lobby list before retrieving it. false by default.</param>
    /// <returns>A list of lobby IDs (CSteamID) representing the lobby list.</returns>
    /// <remarks>You need to refresh the lobby list if it is your first time retrieving it.</remarks>
    public async Task<List<CSteamID>> GetLobbyList(bool refresh = false)
    {
        if (refresh)
        {
            await RefreshLobbyList();
        }
        return _lobbyList;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Refreshes the internal lobby list.
    /// </summary>
    private Task RefreshLobbyList()
    {
        if (_lobbyList.Count > 0)
        {
            _lobbyList.Clear();
        }

        _lobbyListTaskCompletionSource = new TaskCompletionSource<bool>();

        SteamMatchmaking.AddRequestLobbyListResultCountFilter(50);
        SteamMatchmaking.RequestLobbyList();

        return _lobbyListTaskCompletionSource.Task;
    }

    /// <summary>
    /// Received when a lobby has been created. At this point, the lobby has been joined and is ready for use.
    /// </summary>
    /// <remarks>OnLobbyEntered will also be received (since the local user is joining their own lobby).</remarks>
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("SteamManager: There was an issue creating a lobby.");
            return;
        }

        _currentLobbyID = callback.m_ulSteamIDLobby;
        SetLobbyData("HostAddress", SteamUser.GetSteamID().ToString());
        SetLobbyData("name", $"{SteamFriends.GetPersonaName()}'s lobby");
        _fishySteamworks.SetClientAddress(SteamUser.GetSteamID().ToString());
        _fishySteamworks.StartConnection(true);
    }

    /// <summary>
    /// Called when the user tries to join a lobby from their friends list or from an invite.
    /// </summary>
    /// <remarks>User will attempt to join the requested lobby when this is received.</remarks>
    private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    /// <summary>
    /// Received upon attempting to enter a lobby.
    /// </summary>
    /// <remarks>Lobby metadata is available to use immediately after receiving this.</remarks>
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        _currentLobbyID = callback.m_ulSteamIDLobby;
        _fishySteamworks.SetClientAddress(GetLobbyData(GetCurrentLobbyID(),"HostAddress"));
        _fishySteamworks.StartConnection(false);
    }

    /// <summary>
    /// Received when requesting the lobby list. Iterates through all the found lobbies and adds it to the lobby list.
    /// </summary>
    private void OnLobbyListRequested(LobbyMatchList_t callback)
    {
        for (int i = 0; i < callback.m_nLobbiesMatching; i++)
        {
            _lobbyList.Add(SteamMatchmaking.GetLobbyByIndex(i));
        }

        _lobbyListTaskCompletionSource?.SetResult(true);
    }

    #endregion
}
