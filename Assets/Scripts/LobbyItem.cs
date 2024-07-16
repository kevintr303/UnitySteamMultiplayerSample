using Steamworks;
using TMPro;
using UnityEngine;

/// <summary>
/// This represents each Lobby object in the Main Menu's lobby viewer.
/// </summary>
public class LobbyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text lobbyCapacityText;
    
    private CSteamID _lobbyID;
    private string _lobbyName;

    /// <summary>
    /// Set the lobby data for this lobby item object, and update its UI elements.
    /// </summary>
    /// <param name="lobbyID">The Steam ID of this lobby.</param>
    /// <param name="lobbyName">The name of this lobby.</param>
    /// <param name="lobbyCurrentMembers">The number of current players in this lobby.</param>
    /// <param name="lobbyCapacity">This lobby's max capacity.</param>
    public void SetLobbyData(CSteamID lobbyID, string lobbyName, int lobbyCurrentMembers, int lobbyCapacity)
    {
        _lobbyID = lobbyID;
        _lobbyName = lobbyName;
        lobbyNameText.text = _lobbyName == "" ? "Empty" : _lobbyName;
        lobbyCapacityText.text = $"{lobbyCurrentMembers}/{lobbyCapacity}";
    }

    public void JoinLobby()
    {
        SteamManager.Instance.JoinLobby(_lobbyID);
    }
}
