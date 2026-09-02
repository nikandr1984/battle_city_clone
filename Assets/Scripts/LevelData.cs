using UnityEngine;
using System.Collections.Generic;

public enum TileType: byte
{
    Empty  = 0,  
    Brick  = 1, 
    Steel  = 2,
    Water  = 3,
    Forest = 4,
    Ice    = 5,
}

[CreateAssetMenu(fileName = "Level_", menuName = "Battle City/Level")]
public class LevelData : ScriptableObject
{
    public const int ScreenWidth = 32;
    public const int ScreenHeight = 28;
    public const int FieldWidth = 26;
    public const int FieldHeight = 26;


    [Header("Level")]
    [Min(1)] [SerializeField] private int _levelNumber = 1;


    [HideInInspector][SerializeField]
    private TileType[] _tiles = new TileType[FieldWidth * FieldHeight];

    
    // Спавн врагов
    [Header("Spawns (Координаты левого верхнего угла танка 2х2)")]
    [SerializeField] private Vector2Int[] _enemySpawns = new[]
    {
        new Vector2Int(0, 0),  // слева
        new Vector2Int(12, 0), // центр
        new Vector2Int(24, 0), // справа
    };

    
    // Спавн игроков
    [SerializeField] private Vector2Int[] _playerSpawns = new[]
    {
        new Vector2Int(8, 24),  // 1P
        new Vector2Int(16, 24), // 2P
    };

    
    // Позиция базы
    [SerializeField] private Vector2Int _basePosition = new Vector2Int(12, 24);


    // --- Резерв на будущее ---
    // TODO: последовательность спавна врагов (тип, порядок, паузы)
    // TODO: параметры уровня (типы бонусов, шанс бонуса и т.п.).


    // --- ГЕТТЕРЫ ---
    public int LevelNumber => _levelNumber;
    public Vector2Int BasePosition => _basePosition;
    public IReadOnlyList<Vector2Int> EnemySpawns => _enemySpawns;
    public IReadOnlyList<Vector2Int> PlayerSpawns => _playerSpawns;

    public TileType GetTile(int x, int y) => _tiles[y * FieldWidth + x];
    
    public bool InBounds(int x, int y) => 
                                          x >= 0 
                                       && x < FieldWidth 
                                       && y >= 0 
                                       && y < FieldHeight;


    // --- СЕТТЕРЫ ---

    // Сеттер позиции базы
    public void SetBasePosition (Vector2Int pos) => _basePosition = pos;
    
    // Сеттер позиций спавна врагов
    public void SetEnemySpawn (int index, Vector2Int pos)
    {
        if (index >= 0 && index < _enemySpawns.Length)
        {
            _enemySpawns[index] = pos;
        }
    }

    // Сеттер позиции спавна игроков
    public void SetPlayerSpawn (int index, Vector2Int pos)
    {
        if (index >= 0 && index < _playerSpawns.Length)
        {
            _playerSpawns[index] = pos;
        }
    }

    // Сеттер тайлов
    public void SetTile(int x, int y, TileType value) => _tiles[y * FieldWidth + x] = value;





}
