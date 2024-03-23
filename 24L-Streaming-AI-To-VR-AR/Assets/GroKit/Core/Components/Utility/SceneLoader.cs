
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core3lb
{
    public class SceneLoader : MonoBehaviour
    {
        public string gotoScene;

        public void _GotSetScene()
        {
            SceneManager.LoadScene(gotoScene);
        }

        public void _LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void _LoadScene(int sceneIndex)
        {
            SceneManager.LoadScene(sceneIndex);
        }

        public void _ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
