using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    // �� Inspector ��������Ҫ���صĹؿ�����
    public string levelName = "MyLevel";

    // ����������Ա� UI Button ֱ�ӵ���
    public void LoadLevel()
    {
        Debug.Log("Loading level: " + levelName);
        SceneManager.LoadScene(levelName);
    }
}