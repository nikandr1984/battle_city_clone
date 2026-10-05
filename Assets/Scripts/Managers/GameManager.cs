using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelManager _levelManager;

    [Header("Level Data")]
    [Tooltip("The level that will be loaded when the stage starts")]
    [SerializeField] private LevelData _startLevelData;

    private void Start()
    {
        // Автоматически загружаем уровень при старте сцены, если он назначен
        if (_startLevelData != null && _levelManager != null)
        {
            LoadLevel(_startLevelData);
        }
        else
        {
            Debug.LogWarning("GameManager: LevelLoader или стартовый уровень не назначены");
        }
    }

    public void LoadLevel(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("GameManager: Попытка загрузить null LevelData!");
            return;
        }

        Debug.Log($"GameManager: Запуск уровня {levelData.LevelNumber}");

        _levelManager.LoadLevel(levelData);
    }


    // public void NextLevel()
    // public void RestartLevel()


    



}
