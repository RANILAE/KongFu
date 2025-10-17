using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 场景过渡管理器 —— 每个场景独立挂载，负责本场景切换时的黑屏过渡效果
/// 修复：同步加载导致动画被中断的问题
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    [Header("黑屏过渡效果")]
    public Image fadeImage; // 引用本场景的 UI Image
    public float fadeDuration = 1.0f;

    private void Awake()
    {
        if (fadeImage != null)
        {
            // 强制初始化，避免残留状态
            fadeImage.color = Color.clear;
            fadeImage.raycastTarget = false;
        }
    }

    private void Start()
    {
        // ✅ 新场景自动淡出：如果进来是黑屏，就淡出
        if (fadeImage != null && Mathf.Approximately(fadeImage.color.a, 1f))
        {
            FadeOut();
        }
    }

    /// <summary>
    /// 加载指定场景，带完整黑屏过渡
    /// </summary>
    public void LoadSceneWithTransition(string sceneName)
    {
        StartCoroutine(SceneTransitionCoroutine(sceneName));
    }

    private IEnumerator SceneTransitionCoroutine(string sceneName)
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("fadeImage 未赋值，直接加载场景");
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        // 1️⃣ 开始异步加载场景（但先不激活）
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        if (asyncLoad == null)
        {
            Debug.LogError("场景加载失败: " + sceneName);
            yield break;
        }

        asyncLoad.allowSceneActivation = false; // ⚠️ 关键！先别切场景

        // 2️⃣ 播放淡入动画（完整播放，不会被中断）
        fadeImage.raycastTarget = true;
        yield return StartCoroutine(FadeImage(Color.clear, Color.black, fadeDuration));

        // 3️⃣ 动画播完，再激活场景（此时才真正切换）
        asyncLoad.allowSceneActivation = true;

        // ⚠️ 此后本脚本被销毁，后续淡出由新场景自动处理
    }

    public void FadeOut()
    {
        if (fadeImage != null)
        {
            StartCoroutine(FadeOutRoutine());
        }
    }

    private IEnumerator FadeOutRoutine()
    {
        yield return StartCoroutine(FadeImage(Color.black, Color.clear, fadeDuration));
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = false;
        }
    }

    private IEnumerator FadeImage(Color startColor, Color endColor, float duration)
    {
        if (fadeImage == null) yield break;

        fadeImage.color = startColor;
        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            fadeImage.color = Color.Lerp(startColor, endColor, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = endColor;
    }
}