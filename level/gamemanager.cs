using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ===== 核心场景加载方法 =====

    public void LoadMainMenu() => LoadScene("MainMenu");
    public void LoadPlayerState() => LoadScene("PlayerState");
    public void LoadMapScene() => LoadScene("MapScene");

    // 动态关卡加载方法
    public void LoadLevel(string scenePath)
    {
        LoadScene(scenePath);
    }

    // 完成当前关卡
    public void CompleteLevel()
    {
        if (LevelProgressController.Instance != null)
        {
            LevelProgressController.Instance.CompleteCurrentLevel();
        }

        LoadMapScene();
    }

    // ===== 底层加载方法 =====

    private void LoadScene(string sceneName)
    {
        // ✅ 查找当前场景的 SceneTransitionManager
        SceneTransitionManager transition = FindObjectOfType<SceneTransitionManager>();

        if (transition != null && transition.fadeImage != null)
        {
            // 使用过渡管理器加载场景
            transition.LoadSceneWithTransition(sceneName);
        }
        else
        {
            // 降级：无过渡直接加载（调试用）
            Debug.LogWarning("未找到 SceneTransitionManager 或 fadeImage，直接加载场景: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
    }

    // ✅ 新增：退出游戏（供 Button 调用）
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // ❌ 已移除 DontDestroyOnLoad —— GameManager 将随场景切换被正常销毁
            Debug.Log("GameManager初始化完成");
        }
        else
        {
            Destroy(gameObject);
        }
    }
}