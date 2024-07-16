using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using TMPro;
using UnityEngine;

/// <summary>
/// This handles any multiplayer scene changes. Use this instead of SceneLoader in a multiplayer context.
/// </summary>
public class NetworkSceneLoader : NetworkBehaviour
{
    public static NetworkSceneLoader Instance { get; private set; }

    [SerializeField] private GameObject loadMenu;
    [SerializeField] private TextMeshProUGUI loadingText;

    private NetworkManager _networkManager;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        _networkManager = InstanceFinder.NetworkManager;
    }
    
    private void OnEnable()
    {
        _networkManager.SceneManager.OnLoadStart += OnSceneLoadStart;
        _networkManager.SceneManager.OnLoadPercentChange += OnLoadPercentChange;
        _networkManager.SceneManager.OnLoadEnd += OnSceneLoadEnd;
    }

    private void OnDisable()
    {
        _networkManager.SceneManager.OnLoadStart -= OnSceneLoadStart;
        _networkManager.SceneManager.OnLoadPercentChange -= OnLoadPercentChange;
        _networkManager.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
    }
    
    /// <summary>
    /// Changes the network scene and closes specified scenes.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    /// <param name="scenesToClose">Array of scene names to close.</param>
    public void LoadScene(string sceneName, List<string> scenesToClose = null)
    {
        if (scenesToClose != null)
        {
            foreach (var scene in scenesToClose)
            {
                CloseScene(scene);
            }
        }
        
        var sld = new SceneLoadData(sceneName);
        var conns = _networkManager.ServerManager.Clients.Values.ToArray();
        _networkManager.SceneManager.LoadConnectionScenes(conns, sld);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void CloseScene(string sceneToClose)
    {
        CloseSceneObserver(sceneToClose);
    }

    [ObserversRpc]
    private void CloseSceneObserver(string sceneToClose)
    {
        SceneLoader.Instance.CloseScene(sceneToClose);
    }
    
    private void OnSceneLoadStart(SceneLoadStartEventArgs obj)
    {
        loadMenu.SetActive(true);
        loadingText.text = loadingText.text = "Loading... ";
    }
    
    private void OnSceneLoadEnd(SceneLoadEndEventArgs obj)
    {
        loadMenu.SetActive(false);
    }
    
    private void OnLoadPercentChange(SceneLoadPercentEventArgs obj)
    {
        loadingText.text = "Loading... " + (obj.Percent * 100f).ToString("F0") + "%";
    }
}
