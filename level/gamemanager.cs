using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("黑屏过渡效果")]
    public Image fadeImage;
    public float fadeDuration = 1.0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 初始化黑屏效果
            if (fadeImage != null)
                fadeImage.color = Color.clear;

            Debug.Log("GameManager初始化完成");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ===== 核心场景加载方法 =====

    public void LoadMainMenu() => LoadScene("MainMenu");
    public void LoadPlayerState() => LoadScene("PlayerState");
    public void LoadMapScene() => LoadScene("MapScene");

    // 动态关卡加载方法
    public void LoadLevel(string scenePath)
    {
        // 直接加载场景
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
        StartCoroutine(SceneTransitionCoroutine(sceneName));
    }

    private IEnumerator SceneTransitionCoroutine(string sceneName)
    {
        // 淡入黑屏
        if (fadeImage != null)
        {
            yield return StartCoroutine(FadeCoroutine(Color.clear, Color.black, fadeDuration));
        }

        // 加载新场景
        SceneManager.LoadScene(sceneName);

        // 如果是地图场景，刷新所有关卡按钮状态
        if (sceneName == "MapScene")
        {
            yield return new WaitForSeconds(0.1f);
            if (LevelProgressController.Instance != null)
            {
                LevelProgressController.Instance.RefreshAllButtons();
            }
        }

        // 淡出恢复
        if (fadeImage != null)
        {
            yield return StartCoroutine(FadeCoroutine(Color.black, Color.clear, fadeDuration));
        }
    }

    // 渐变效果
    private IEnumerator FadeCoroutine(Color startColor, Color endColor, float duration)
    {
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            if (fadeImage != null)
                fadeImage.color = Color.Lerp(startColor, endColor, timeElapsed / duration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        if (fadeImage != null)
            fadeImage.color = endColor;
    }
}