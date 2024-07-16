using System.Collections;
using FishNet;
using UnityEngine;

/// <summary>
/// This handles the Bootstrap scene. Any scene changes should be additive, as the Bootstrap scene must always be in the background.
/// The Bootstrap scene should only ever be run once.
/// </summary>
public class BootstrapManager : MonoBehaviour
{
    [Tooltip("The name of the main menu scene")]
    [SerializeField] private string mainMenuScene = "MainMenu";
        
    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    /// <remarks>This should be called when Steamworks is initialized.</remarks>
    public void LoadMainMenu()
    {
        StartCoroutine(Validate());
    }

    /// <summary>
    /// Validates the bootstrap scene before loading the main menu.
    /// </summary>
    private IEnumerator Validate()
    {
        // Wait until systems and managers exist and are loaded before loading the main menu
        yield return new WaitUntil(() => InstanceFinder.NetworkManager);
        yield return new WaitUntil(() => SteamManager.Instance);
        yield return new WaitUntil(() => SceneLoader.Instance);
        yield return new WaitUntil(() => NetworkSceneLoader.Instance);

        SceneLoader.Instance.LoadScene(mainMenuScene);
    }
}
