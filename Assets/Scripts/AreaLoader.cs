using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaLoader : MonoBehaviour
{
    [SerializeField] private string sceneNameToLoad = "BackyardScene";
    private bool isLoaded = false;  // prevent multiple loads

    private void OnTriggerEnter(Collider other)
    {
        // Assuming your player has a tag "Player"
        if (other.CompareTag("Player") && !isLoaded)
        {
            isLoaded = true;
            SceneManager.LoadSceneAsync(sceneNameToLoad, LoadSceneMode.Additive);
        }
    }
}
