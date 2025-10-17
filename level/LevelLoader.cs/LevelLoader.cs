using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    // 在 Inspector 中设置你要加载的关卡名称
    public string levelName = "MyLevel";

    // 这个方法可以被 UI Button 直接调用
    public void LoadLevel()
    {
        Debug.Log("Loading level: " + levelName);
        SceneManager.LoadScene(levelName);
    }
}