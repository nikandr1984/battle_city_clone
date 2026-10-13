using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelManager _levelManager;
    [SerializeField] private UIManager _uiManager;

    [Header("Level Data")]
    [Tooltip("The level that will be loaded when the stage starts")]
    [SerializeField] private LevelData _startLevelData;

    private void Start()
    {
        // јвтоматически загружаем уровень при старте сцены, если он назначен
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
            Debug.LogError("GameManager: ѕопытка загрузить null LevelData!");
            return;
        }

        Debug.Log($"GameManager: «апуск уровн€ {levelData.LevelNumber}");

        _levelManager.LoadLevel(levelData);

        _uiManager.InitializeHUD(levelNumber: levelData.LevelNumber,
                                   totalEnemies: 20, // TODO: получить из LevelData
                                   lives1P: 3,
                                   lives2P: 3,
                                   twoPlayers: false);




    }


    // public void NextLevel()
    // public void RestartLevel()


    



}
