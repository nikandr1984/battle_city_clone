using UnityEngine;

public static class DirectionExtensions
{

    // ѕреобразует направление в вектор дл€ движени€ или raycast
    public static Vector2 ToVector2(this Direction direction)
    {                
        return direction switch
        {
            Direction.Up => Vector2.up,
            Direction.Down => Vector2.down,
            Direction.Left => Vector2.left,
            Direction.Right => Vector2.right,
            Direction.None => Vector2.zero,
            _ => Vector2.zero
        };
    }


    // ¬озвращает противоположное направление
    public static Direction GetOpposite(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.None => Direction.None,
            _ => Direction.Up
        };
    }


    // ѕоворачивает направление на 90 градусов
    public static Direction Rotate90(this Direction direction, bool clockwise = true)
    {
        return direction switch
        {
            Direction.Up => clockwise ? Direction.Right : Direction.Left,
            Direction.Right => clockwise ? Direction.Down : Direction.Up,
            Direction.Down => clockwise ? Direction.Left : Direction.Right,
            Direction.Left => clockwise ? Direction.Up : Direction.Down,
            Direction.None => Direction.None,
            _ => Direction.Up
        };
    }
}
