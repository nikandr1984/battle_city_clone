using UnityEngine;
using UnityEngine.Tilemaps;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap _tilemapFrame;
    [SerializeField] private Tilemap _tilemapHUD;

    
    [Header("Config")]
    [SerializeField] UILayoutConfig _layoutConfig;

    
    [Header("UI Tiles")]
    [Tooltip("Tile for frame around field")]
    [SerializeField] private TileBase _frameTile;
    
    [Tooltip("Array of flag tiles: [TL, TR, BL, BR")]
    [SerializeField] private TileBase[] _flagTiles = new TileBase[4];

    [Tooltip("Array of number tiles (0-9)")]
    [SerializeField] private TileBase[] _numberTiles = new TileBase[10];

    [Tooltip("Tile for 'I' lable")]
    [SerializeField] private TileBase _lableITile;

    [Tooltip("Tile for 'II' lable")]
    [SerializeField] private TileBase _lableIITile;

    [Tooltip("Tile for 'P' lable")]
    [SerializeField] private TileBase _lablePTile;

    [Tooltip("Tile for player tank lable")]
    [SerializeField] private TileBase _playerTankTile;

    [Tooltip("Tile for player tank lable")]
    [SerializeField] private TileBase _enemyTankTile;




    private void Start()
    {
        if (_layoutConfig == null)
        {
            Debug.LogError("UIManager: LayoutConfig not assigned!");
            return;
        }
        
        // Автоматически рисуем рамку при старте сцены
        DrawFrame();

        
    }


    // МЕТОД: Рисует рамку вокруг поля битвы
    private void DrawFrame()
    {
        // 1. Предохранитель
        if (_tilemapFrame == null || _layoutConfig == null || _frameTile == null)
        {
            Debug.LogError("UIManager: Missing references!");
            return;
        }
                
        // 2. Очищаем тайлмап перед рисованием
        

        // 3. Дополнительные переменные
        int screenWidth = UILayoutConfig.ScreenWidth;   // Ширина экрана
        int screenHeight = UILayoutConfig.ScreenHeight; // Высота экрана
        int hudStartX = _layoutConfig.hudPanelX;        // Х-координата начала HUD панели
        int hudWidth = screenWidth - hudStartX;         // Ширина HUD панели
        int hudHeight = screenHeight - 2;               // Высота HUD панели 

        // 4. Верхняя и нижняя рамки
        TileBase[] horizontalLine = new TileBase[screenWidth];
        for (int i = 0; i < screenWidth; i++)
        {
            horizontalLine[i] = _frameTile;
        }

        // BoundsInt(x, y, z, width, height, depth)
        _tilemapFrame.SetTilesBlock(new BoundsInt(0, screenHeight - 1, 0, screenWidth, 1, 1), horizontalLine);
        _tilemapFrame.SetTilesBlock(new BoundsInt(0, 0, 0, screenWidth, 1, 1), horizontalLine);

        // 5. Левая рамка (2 тайла шириной)
        TileBase[] leftColumn = new TileBase[2 * hudHeight];        
        
        int idx = 0;        
        
        for (int y = 1; y < screenHeight - 1; y++)
        {
            leftColumn[idx++] = _frameTile; // x = 0
            leftColumn[idx++] = _frameTile; // x = 1
        }

        _tilemapFrame.SetTilesBlock(new BoundsInt(0, 1, 0, 2, hudHeight, 1), leftColumn);

        // 5. Правая рамка (4 тайла шириной)
        TileBase[] rightColumn = new TileBase[hudWidth *  hudHeight];

        idx = 0;

        for (int y = 1; y < screenHeight - 1; y++)
        {
            for (int x = 0; x < hudWidth; x++)
            {
                rightColumn[idx++] = _frameTile;
            }
        }

        _tilemapFrame.SetTilesBlock(new BoundsInt(hudStartX, 1, 0, hudWidth, hudHeight, 1), rightColumn);


        Debug.Log("UIManager: frame drawn.");
    }


    // МЕТОД: инициализирует HUD при загрузке уровня
    public void InitializeHUD(int levelNumber, int totalEnemies, int lives1P, int lives2P = -1, 
                              bool twoPlayers  = false)
    {
        
        // Предохранитель
        if (_layoutConfig == null || _tilemapFrame == null || _tilemapHUD == null)
        {
            Debug.LogWarning("UIManager: References not assigned!");
            return;
        }

        // Очищаем область HUD
        ClearHUDArea();

        // Рисуем HUD элементы
        DrawEnemyCounter(totalEnemies); // счетчик врагов
        DrawPlayer1Lable();             // этикетка Player 1
        DrawPlayer1Lives(lives1P);      // жизни Player 1

        if (!twoPlayers)
        {
            DrawPlayer2Lable();         // этикетка Player 2
            DrawPlayer2Lives(lives2P);  // жизни Player 2
        }


        DrawFlag();                     // флаг        
        DrawStageNumber(levelNumber);   // номер уровня              
        
    }




    // МЕТОД: Рисует флаг
    private void DrawFlag()
    {
        if (_flagTiles == null || _flagTiles.Length != 4)
        {
            Debug.LogWarning("UIManager: Flag tiles not configured!");
            return;
        }

        Vector2Int origin = _layoutConfig.flagOrigin;

        // Порядок в массиве: [TL, TR, BL, BR]
        // Позиции относительно origin (BL):
        // BL (0, 0), BR (1, 0), TL (0, 1), TR (1, 1)
        int[] dx = { 0, 1, 0, 1 }; // смещение X
        int[] dy = { 1, 1, 0, 0 }; // смещение Y

        for (int i = 0; i < 4; i++)
        {
            Vector3Int pos = new(origin.x + dx[i], origin.y + dy[i], 0);

            _tilemapHUD.SetTile(pos, _flagTiles[i]);
        }
        
    }


    // МЕТОД: рисует номер уровня
    private void DrawStageNumber(int stageNumber)
    {
        if (_numberTiles == null || _numberTiles.Length < 10) return;

        int tens = stageNumber / 10;
        int ones = stageNumber % 10;

        Vector3Int tensPos = new(_layoutConfig.hudColumn1X, _layoutConfig.stageNumberY, 0);
        Vector3Int onesPos = new(_layoutConfig.hudColumn2X, _layoutConfig.stageNumberY, 0);

        _tilemapHUD.SetTile(tensPos, _numberTiles[tens]);
        _tilemapHUD.SetTile(onesPos, _numberTiles[ones]);
    }


    // МЕТОД: Рисуем данные Player 1
    private void DrawPlayer1Lable()
    {
        if (_lableITile == null || _lablePTile == null) return;

        Vector3Int posI = new(_layoutConfig.hudColumn1X, _layoutConfig.player1LabelY, 0);
        Vector3Int posP = new(_layoutConfig.hudColumn2X, _layoutConfig.player1LabelY, 0);

        _tilemapHUD.SetTile(posI, _lableITile);
        _tilemapHUD.SetTile(posP, _lablePTile);
    }


    private void DrawPlayer1Lives(int lives1P)
    {
        if (_numberTiles == null || _numberTiles.Length < 10) return;

        Vector3Int posLable = new(_layoutConfig.hudColumn1X, _layoutConfig.player1LivesY, 0);
        Vector3Int posCounter = new(_layoutConfig.hudColumn2X, _layoutConfig.player1LivesY, 0);

        _tilemapHUD.SetTile(posLable, _playerTankTile);
        _tilemapHUD.SetTile(posCounter, _numberTiles[Mathf.Min(lives1P, 9)]);
    }



    // МЕТОД: Рисуем данные Player 2
    private void DrawPlayer2Lable()
    {
        if (_lableIITile == null || _lablePTile == null) return;

        Vector3Int posII = new(_layoutConfig.hudColumn1X, _layoutConfig.player2LabelY, 0);
        Vector3Int posP = new(_layoutConfig.hudColumn2X, _layoutConfig.player2LabelY, 0);

        _tilemapHUD.SetTile(posII, _lableIITile);
        _tilemapHUD.SetTile(posP, _lablePTile);
    }


    private void DrawPlayer2Lives(int lives2P)
    {
        if (_numberTiles == null || _numberTiles.Length < 10) return;

        Vector3Int posLable = new(_layoutConfig.hudColumn1X, _layoutConfig.player2LivesY, 0);
        Vector3Int posCounter = new(_layoutConfig.hudColumn2X, _layoutConfig.player2LivesY, 0);

        _tilemapHUD.SetTile(posLable, _playerTankTile);
        _tilemapHUD.SetTile(posCounter, _numberTiles[Mathf.Min(lives2P, 9)]);
    }

   

    public void UpdatePlayer1Lives()
    {

    }

    public void UpdatePlayer2Lives()
    {

    }


    // --- СЧЕТЧИК ВРАГОВ ---

    // МЕТОД: рисует счетчик врагов
    private void DrawEnemyCounter(int totalEnemys)
    {
        // Предохранитель
        if (_enemyTankTile == null) return;

        // Ограничитель
        int maxEnemies = Mathf.Min(totalEnemys, 20);

        for (int i = 0; i < maxEnemies; i++)
        {
            // Вычисляем координаты по индексу
            int x = i % 2; // 0 или 1 (колонка) 
            int y = i / 2; // 0..9 (ряд)

            Vector3Int pos = new(_layoutConfig.hudColumn1X + x, _layoutConfig.enemyCounterTopY - y, 0);

            _tilemapHUD.SetTile(pos, _enemyTankTile);
        }
            
    }

    // МЕТОД: Очищает область счетчика врагов
    private void ClearEnemyCounterArea()
    {
        int xLeft = _layoutConfig.hudColumn1X;           // 29
        int xRight = _layoutConfig.hudColumn2X;          // 30
        int yTop = _layoutConfig.enemyCounterTopY;       // 25
        int yBottom = _layoutConfig.enemyCounterBottomY; // 27

        for (int x = xLeft; x <= xRight; x++) 
            for (int y = yBottom;  y <= yTop; y++)
            {
                _tilemapHUD.SetTile(new Vector3Int(x, y, 0), null);
            }
    }

    // МЕТОД: изменяет счетчик врагов
    public void UpdateEnemyCounter(int currentCount)
    {
        // Очищаем область счетчика врагов
        ClearEnemyCounterArea();

        // Рисуем новое колличество
        DrawEnemyCounter(currentCount);
    }    
   

    // МЕТОД: Очищает HUD область целиком
    private void ClearHUDArea()
    {
        int hudLeft = _layoutConfig.hudPanelX;      // 28
        int hudRight = UILayoutConfig.ScreenWidth;  // 32
        int hudBottom = _layoutConfig.fieldOffsetY; // 1
        int hudTop = _layoutConfig.fieldOffsetY + _layoutConfig.fieldHeight; // 27

        for (int x = hudLeft; x < hudRight; x++)
            for (int y = hudBottom; y < hudTop; y++)
            {
                _tilemapHUD.SetTile(new Vector3Int(x, y, 0), null);
            }
    }
    




    // Тестовые методы
    [ContextMenu("Test: Draw Frame")]
    private void TesrDrawFrame()
    {
        DrawFrame();        
    }

    [ContextMenu("Test: Clear UI")]
    private void TestClearUI()
    {
        _tilemapFrame?.ClearAllTiles();
    }

    [ContextMenu("Test: Clear HUD area")]
    private void TestClearHUD()
    {
        ClearHUDArea();
        //ClearEnemyCounterArea();
    }
}
