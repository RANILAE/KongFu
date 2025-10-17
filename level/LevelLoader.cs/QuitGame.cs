using UnityEngine;

public class QuitGame : MonoBehaviour
{
    // 这个方法可以被 UI Button 直接调用
    public void Quit()
    {
        Debug.Log("退出游戏");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // 在编辑器中停止播放
#else
        Application.Quit(); // 在构建版本中退出游戏
#endif
    }
}