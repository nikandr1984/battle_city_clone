using UnityEngine;

public class LevelState  // Хранит изменяемое состояние уровня во время игры.
{
    private readonly int _width;
    private readonly int _height;
    private readonly TileType[] _tiles;

    public int Width => _width;
    public int Height => _height;


    // Конструктор класса
    public LevelState(LevelData levelData)
    {
        // Настраиваем поля
        _width = LevelData.FieldWidth;
        _height = LevelData.FieldHeight;
        _tiles = new TileType[_width * _height];

        // Копируем данные из ассета в нашу runtime-копию
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                _tiles[y * _width + x] = levelData.GetTileType(x, y);
            }
    }



    // МЕТОД: по переданным координатам возвращает тип тайла, хранящийся в массиве _tiles 
    public TileType GetTile(int x, int y)
    {
        if (!InBounds(x, y)) return TileType.Empty;
             
        return _tiles[y * _width + x];
        
    }


    // МЕТОД: перезаписывает тип тайла в массив 
    public void SetTile(int x, int y, TileType tileType)
    {
        if (!InBounds(x, y)) return;

        _tiles[y * _width + x] = tileType;
    }

    // ХЕЛПЕР: проверяет находится ли тайл в пределах игрового поля
    public bool InBounds(int x, int y)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height;
    }
}
