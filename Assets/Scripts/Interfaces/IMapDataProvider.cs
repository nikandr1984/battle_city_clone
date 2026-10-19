using UnityEngine;

public interface IMapDataProvider
{
    // Возвращает тип тайла в указанных координатах сетки LevelData
    TileType GetTile(int x, int y);

    bool IsBounds(int x, int y);
}
