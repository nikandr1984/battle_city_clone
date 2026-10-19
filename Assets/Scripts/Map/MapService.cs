using UnityEngine;

public class MapService : IMapService
{
    private readonly LevelManager _levelManager;

    private const int FieldOffsetX = 2;
    private const int FieldOffsetY = 1;

    public MapService(LevelManager levelManager)
    {
        _levelManager = levelManager;
    }


    // Проверяет может ли объект с заданным размером поместиться в указанную позицию
    public bool CanMoveTo(Vector2 worldPosition, Vector2 size)
    {
        return false;
    }




    // Проверяет находится ли точка в непроходимом тайле
    public bool IsBlocked(Vector2 worldPosition)
    {
        return false;
    }


    // Конвертирует мировые координаты в координаты сетки LevelData
    private Vector2Int WorldToGrid(Vector2 worldPos)
    {
        return Vector2Int.zero;
    }


    // Проверяет находится ли позиция в пределах карты
    private bool IsInBounds(Vector2Int gridPos)
    {
        return gridPos.x >= 0 &&
               gridPos.x < LevelData.FieldWidth &&
               gridPos.y >= 0 &&
               gridPos.y < LevelData.FieldHeight;
    }



    // Определяет является ли тайл непроходимым
    private bool IsTileBlocking(TileType tileType)
    {
        switch (tileType)
        {
            // Проходимые
            case TileType.Empty:
            case TileType.Trees:
            case TileType.Ice:
                return false;

            // Непроходимые
            case TileType.Brick:
            case TileType.Steel:
            case TileType.Water:
                return true;

            // Неизвестный = непроходимый 
            default:
                return true;
        }
    }

}
