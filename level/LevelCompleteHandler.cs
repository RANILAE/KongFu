using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteHandler : MonoBehaviour
{
    public Button backToMapButton;

    void Start()
    {
        if (backToMapButton != null)
        {
            backToMapButton.onClick.AddListener(CompleteLevelAndReturn);
        }
    }

    public void CompleteLevelAndReturn()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteLevel();
        }
    }
}