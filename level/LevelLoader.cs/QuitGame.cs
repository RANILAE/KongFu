using UnityEngine;

public class QuitGame : MonoBehaviour
{
    // ����������Ա� UI Button ֱ�ӵ���
    public void Quit()
    {
        Debug.Log("�˳���Ϸ");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // �ڱ༭����ֹͣ����
#else
        Application.Quit(); // �ڹ����汾���˳���Ϸ
#endif
    }
}