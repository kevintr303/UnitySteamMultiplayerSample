using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This handles the start up sequence. Extend this script to handle any boot menus or sponsor screens.
/// It can do whatever you want, just make sure to load the Bootstrap scene at the end of it.
/// </summary>
public class StartupManager : MonoBehaviour
{
    private void Start()
    {
        // TODO: Create the start up sequence. Just immediately load the bootstrap scene for now.
        LoadBootstrapScene(); 
    }
    
    /// <summary>
    /// Loads the bootstrap scene, which contains all the systems and managers.
    /// </summary>
    /// <remarks>This should only ever be called once, at the entry point of the game.</remarks>
    private void LoadBootstrapScene()
    {
        SceneManager.LoadScene("Bootstrap");
    }
}
