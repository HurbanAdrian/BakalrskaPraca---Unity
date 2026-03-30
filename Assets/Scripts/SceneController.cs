using UnityEngine;
using UnityEngine.SceneManagement;

[System.Obsolete("Na nahradenie")]
public class SceneController : MonoBehaviour
{
    public void SceneChange(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        Time.timeScale = 1;     // reset time scale
    }


}
