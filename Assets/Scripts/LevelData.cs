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
    public const int FildHeight = 26;


    [Header("Level")]
    [Min(1)] [SerializeField] private int _levelNumber = 1;


    [HideInInspector][SerializeField]
    private TileType[] tiles = new TileType[FieldWidth * FildHeight];

    [Header("Spawns ( оординаты левого верхнего угла танка 2х2)")]
    [SerializeField] private Vector2Int[] _enemySpawns = new[]
    {
        new Vector2Int(0, 0),  // слева
        new Vector2Int(12, 0), // центр
        new Vector2Int(24, 0), // справа
    };

    [SerializeField] private Vector2Int[] _playerSpawns = new[]
    {
        new Vector2Int(8, 24),  // 1P
        new Vector2Int(16, 24), // 2P
    };

    [SerializeField] private Vector2Int _basePosition = new Vector2Int(12, 24);


    // --- –езерв на будущее ---
    // TODO: последовательность спавна врагов (тип, пор€док, паузы)
    // TODO: параметры уровн€ (типы бонусов, шанс бонуса и т.п.).


    // --- ƒоступ дл€ загрузчика ---
    public int LevelNumber => _levelNumber;
    public Vector2Int BasePosition => _basePosition;
    public IReadOnlyList<Vector2Int> EnemySpawns => _enemySpawns;
    public IReadOnlyList<Vector2Int> PlayerSpawns => _playerSpawns;

    public TileType GetTile(int x, int y) => tiles[y * FieldWidth + x];
    public void SetTile(int x, int y, TileType value) => tiles[y * FieldWidth + x] = value; 
    public bool InBounds(int x, int y) => x >= 0 && x < FieldWidth && y >= 0 && y < FildHeight;

}
