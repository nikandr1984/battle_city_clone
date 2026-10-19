using UnityEngine;

public interface IMapService
{
    // Проверяет может ли объект с заданным размером поместиться в указанную позицию
    // worldPosition - левый нижний угол объекта в мировых координатах
    // Размер объекта в юнитах (для танка 2х2 = Vector2(2, 2)
    bool CanMoveTo(Vector2 worldPosition, Vector2 size); 


    // Проверяет находится ли точка в непроходимом тайле
    bool IsBlocked(Vector2 worldPosition);
}
