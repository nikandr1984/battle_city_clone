using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    // Ссылки на тайлмапы сцены
    [Header("Tilemaps")]
    [SerializeField] private Tilemap _tilemapWalls;
    [SerializeField] private Tilemap _tilemapIce;
    [SerializeField] private Tilemap _tilemapTrees;
    [SerializeField] private Tilemap _tilemapWater;


    // Ссылка на конфиг тайлсета: маппинг TileType -> Tile
    [Header("Config")]
    [SerializeField] private TilesetConfig _tilesetConfig;


    [Header("Prefabs")]
    [SerializeField] private GameObject _eaglePrefab;

    private GameObject _spawnMarkersRoot; // Родительский объект для маркеров спавна
    private LevelState _currentState;     // Текущее состояние уровня в памяти


    // МЕТОД: загружает уровень
    public void LoadLevel(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("LevelLoader: LevelData is null");
            return;
        }

        Debug.Log($"LevelLoader: Loading level {levelData.LevelNumber}...");

        // Очищаем все тайлмапы перед загрузкой
        ClearAllTilemaps();

        // Инициализируем runtime-состояния уровня
        _currentState = new LevelState(levelData);
        Debug.Log("LevelLoader: Runtime LevelState initialized.");

        // Раскладываем тайлы
        PlaceTiles(levelData);

        // Устанавливаем базу-орла
        SpawnEagle(levelData);

        // Устанавливаем маркеры спавнов врагов и игроков
        SpawnMarkers(levelData);
    }


    // МЕТОД: получает тип тайла в клетке для текущего состояния уровня
    public TileType GetTile(int x, int y)
    {
        return _currentState != null ? _currentState.GetTile(x, y) : TileType.Empty;
    }


    // МЕТОД: меняет тип тайла в клетке для текущего состояния уровня
    public void SetTile(int x, int y, TileType newType)
    {
        if (_currentState == null) return;
        
        // Обновляем логическое состояние
        _currentState.SetTile(x, y, newType);

        // Обновляем визуал на тайлмапе
        UpdateTileVisual(x, y, newType);
    }




    // МЕТОД: очищает все тайлмапы от предыдущего уровня
    private void ClearAllTilemaps()
    {
        _tilemapWalls?.ClearAllTiles();
        _tilemapIce  ?.ClearAllTiles();
        _tilemapTrees?.ClearAllTiles();
        _tilemapWater?.ClearAllTiles();

        Debug.Log("LevelLoader: All tilemaps cleared.");
    }


    // МЕТОД: раскладывает тайлы из LevelData по тайлмапам сцены
    private void PlaceTiles(LevelData levelData)
    {
        if (_tilesetConfig == null)
        {
            Debug.LogError("LevelLoader: TilesetConfig not assigned!");
            return;
        }

        int width = LevelData.FieldWidth;
        int height = LevelData.FieldHeight;

        // Проход по всем клеткам поля
        for (int dataY = 0; dataY < height; dataY++)
            for (int dataX = 0; dataX < width; dataX++)
            {
                // Определяем какой тип тайла стоит в данных координатах
                TileType tileType = levelData.GetTileType(dataX, dataY);

                // Пустые клетки пропускаем
                if (tileType == TileType.Empty)
                {
                    continue;
                }

                // Получаем тайл из конфига на основании типа тайла из LevelData
                TileBase tile = _tilesetConfig.GetTile(tileType);                
                
                if (tile == null)
                {
                    Debug.LogWarning($"LevelLoader: No tile for {tileType} in TilesetConfig, skipping");
                    continue;
                }

                // Определяем на какой тайлмап класть
                Tilemap targetTilemap = GetTilemapForType(tileType);

                if (targetTilemap == null)
                {
                    Debug.LogWarning($"LevelLoader: No tilemap for {tileType}, skipping");
                    continue;
                }

                // Конвертируем координаты: LevelData (Y вниз) -> Unity (Y вверх)
                int cellX = dataX;                
                int cellY = height - 1 - dataY;
                Vector3Int cellPosition = new Vector3Int(cellX, cellY, 0);

                // Устанавливаем тайл
                targetTilemap.SetTile(cellPosition, tile);
            }

        Debug.Log($"LevelLoader: Tiles placed from level {levelData.LevelNumber}.");

    }


    // ХЕЛПЕР: определяет на какой тайлмап класть тайл
    private Tilemap GetTilemapForType (TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Brick:
            case TileType.Steel:
                return _tilemapWalls;

            case TileType.Ice:
                return _tilemapIce;

            case TileType.Trees:
                return _tilemapTrees;

            case TileType.Water:
                return _tilemapWater;

            default:
                return null;

        }        
    }

    // ХЕЛПЕР: обновляет визуал тайла при изменении состояния
    private void UpdateTileVisual(int x, int y, TileType newType)
    {
        // Конвертируем коордмнаты (Y вниз -> Y вверх)
        int cellY = LevelData.FieldHeight - 1 - y;
        Vector3Int cellPosition = new Vector3Int(x, cellY, 0);

        // Очищаем все тайлмапы в этой клетке
        _tilemapWalls?.SetTile(cellPosition, null);
        _tilemapIce  ?.SetTile(cellPosition, null);
        _tilemapTrees?.SetTile(cellPosition, null);
        _tilemapWater?.SetTile(cellPosition, null);

        // Если новый тип не пустой, ставим нужный тайл
        if (newType != TileType.Empty)
        {
            TileBase tile = _tilesetConfig.GetTile(newType);
            Tilemap target = GetTilemapForType(newType);

            if (tile != null && target != null)
            {
                target.SetTile(cellPosition, tile);
            }
        }
    }


    // МЕТОД: спавнит объект базу-орла
    private void SpawnEagle(LevelData levelData)
    {
        // Страховка от null
        if (_eaglePrefab == null)
        {
            Debug.LogWarning("LevelLoader: Eagle prefab not assigned, skipping.");
            return;
        }

        // Получаем локальные координаты расположения базы из LevelData
        Vector2Int basePosEditor = levelData.BasePosition;

        // Конвертируем координаты: LevelData (Y вниз) -> Unity (Y вверх)
        int cellX = basePosEditor.x;
        int cellY = LevelData.FieldHeight - 2 - basePosEditor.y;        
        
        // Переводим координаты в трехмерную систему
        Vector3Int basePosUnity = new Vector3Int(cellX, cellY, 0);

        // Конвертируем в мировые координаты через тайлмап
        Vector3 basePosWorld = _tilemapWalls.CellToWorld(basePosUnity);

        // Сдвиг на половину клетки, чтобы центр орла был в центре области 2х2
        float cellSize = 1f;
        basePosWorld += new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0);

        // Инстанцируем орла
        GameObject eagle = Instantiate(_eaglePrefab, basePosWorld, Quaternion.identity);
        eagle.name = "Eagle";

        Debug.Log($"LevelLoader: Eagle spawned at {basePosWorld} (cell {basePosEditor}).");
    }

    // МЕТОД: спавнит маркеры
    private void SpawnMarkers(LevelData levelData)
    {
        // Очистка старых маркеров
        if (_spawnMarkersRoot != null)
        {
            Destroy(_spawnMarkersRoot);
        }

        // Создаем новый родительский объект для маркеров
        _spawnMarkersRoot = new GameObject("SpawnMarkers");

        // Создаем простой белый квадратный спрайт 1х1
        Sprite squareSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), 
                                            new Vector2(0.5f, 0.5f), 1f);

        // Спавним маркеры врагов (красные)
        var enemySpawns = levelData.EnemySpawns;
        for (int i = 0; i < enemySpawns.Count; i++)
        {
            CreateSpawnMarker(_spawnMarkersRoot, $"EnemySpawn_{i}", enemySpawns[i], squareSprite, Color.red);
        }

        // Спавнем маркеры игроков (зеленые)
        var playerSpawns = levelData.PlayerSpawns;
        for (int i = 0;i < playerSpawns.Count; i++)
        {
            CreateSpawnMarker(_spawnMarkersRoot, $"PlayerSpawn_{i}", playerSpawns[i], squareSprite, Color.green);
        }

        Debug.Log($"LevelLoader: Spawn markers created.");
    }

    // ХЕЛПЕР: создание одного маркера 2х2
    private void CreateSpawnMarker(GameObject parent, string name, Vector2Int dataPos, 
                                   Sprite sprite, Color color)
    {
        // Конвертируем координаты: LevelData (Y вниз) -> Unity (Y вверх)
        int cellX = dataPos.x;
        int cellY = LevelData.FieldHeight - 2 - dataPos.y;

        // Переводим координаты в трехмерную систему
        Vector3Int cellPosition = new Vector3Int(cellX, cellY, 0);

        // Конвертируем в мировые координаты через тайлмап и сдвигаем на единицу
        Vector3 worldPos = _tilemapWalls.CellToWorld(cellPosition);
        worldPos += new Vector3(1f, 1f, 0);

        // Создаем объект и выставляем его характеристики        
        GameObject marker = new GameObject(name);
        marker.transform.SetParent(parent.transform);
        marker.transform.position = worldPos;
        marker.transform.localScale = new Vector3(2f, 2f, 1f);

        // Прикрепляем к объекту компонент SpriteRenderer
        SpriteRenderer sr = marker.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingLayerName = "UI";
        sr.sortingOrder = 10;
    }





    

}
