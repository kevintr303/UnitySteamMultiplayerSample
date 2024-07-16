using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This handles any singleplayer scene changes. Use this instead of NetworkSceneLoader in a singleplayer context.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private GameObject loadMenu;
    [SerializeField] private TextMeshProUGUI loadingText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Loads the specified scene asynchronously and closes all the specified scenes.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    /// <param name="scenesToClose">A list of scenes to close. Default is null.</param>
    /// <remarks>This should only be used for singleplayer scene changes. For multiplayer, use NetworkSceneLoader instead.</remarks>
    public void LoadScene(string sceneName, List<string> scenesToClose = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneLoader: LoadScene called with invalid scene name.");
            return;
        }
        
        StartCoroutine(LoadSceneAsync(sceneName));
        if (scenesToClose == null) return;
        foreach (var scene in scenesToClose)
        {
            CloseScene(scene);
        }
    }

    /// <summary>
    /// Closes the specified scene asynchronously.
    /// </summary>
    /// <param name="sceneName">The name of the scene to close.</param>
    public void CloseScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneLoader: CloseScene called with invalid scene name.");
            return;
        }
        
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }
        else
        {
            Debug.LogWarning($"SceneLoader: Scene {sceneName} is not loaded.");
        }
    }

    /// <summary>
    /// Loads the specified scene asynchronously using SceneManager.LoadSceneAsync and updates the loading UI.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    /// <returns>An IEnumerator for the coroutine.</returns>
    /// <remarks>Should only be called by LoadScene() in the SceneLoader.</remarks>
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (operation == null)
        {
            Debug.LogError($"SceneLoader: Failed to load scene {sceneName}. Make sure the scene name is correct and the scene is added to the build settings.");
            yield break;
        }
        
        // Tell unity to activate the scene as soon as it is ready
        operation.allowSceneActivation = true;
        
        // Enable the loading menu
        loadMenu.SetActive(true);
        
        // While the asynchronous operation to load the new scene is not yet complete, continue updating the progress
        while (!operation.isDone)
        {
            // Update the loading text
            var progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingText.text = "Loading... " + (progress * 100f).ToString("F0") + "%";
            yield return null;
        }
        
        // Disable the loading menu once the scene has been successfully loaded
        loadMenu.SetActive(false);
    }
}
