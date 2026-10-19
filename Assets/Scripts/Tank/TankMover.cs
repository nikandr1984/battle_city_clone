using UnityEngine;


[RequireComponent(typeof(Tank))]
public class TankMover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tank _tank;

    private Direction _requestedDirection = Direction.None; // Буфер: направление, в которое танк хочет повернуть.

    private const float GridSnapTolerance = 0.05f; // Допуск для проверки выравнивания (в юнитах) 

    private void Awake()
    {
        // Если ссылка не назначена в инспекторе - получаем компонент атоматически
        if (_tank == null)
        {
            _tank = GetComponent<Tank>();
        }
    }


    private void Update()
    {
        // Двигаемся только в состоянии Alive
        if (_tank == null || !_tank.IsAlive) return;

        // 1. ОБРАБОТКА ЗАПРОСА НА ПОВОРОТ
        if (_requestedDirection != Direction.None && _requestedDirection != _tank.CurrentDirection)
        {
            // Проверяем выровнен ли танк по сетке
            if (IsAlignedToGrid(transform.position))
            {
                // Выровнен! Выполняем поворот через ядро Tank
                _tank.SetDirection(_requestedDirection);

                // Сбрасываем запрос, так как он выполнен
                _requestedDirection = Direction.None;
            }
        }


        // 2. РАСЧЕТ ДВИЖЕНИЯ
        // Получаем единичный вектор направления        
        Vector2 moveDirection = _tank.CurrentDirection.ToVector2();

        // Берем скорость из данных танка
        float speed = _tank.TankData.MoveSpeed;

        // Рассчитываем смещение за текущий кадр
        Vector2 movement = moveDirection * speed * Time.deltaTime;


        // 3. ПРИМЕНЕНИЕ ДВИЖЕНИЯ
        transform.position += (Vector3)movement;


    }


    // Вызывается извне для запроса смены направления
    public void RequetDirectionChange(Direction newDirection)
    {
        // Защита от спама: если направление то же самое, игнорируем
        if (newDirection == _tank.CurrentDirection)
        {
            _requestedDirection = Direction.None;
            return;
        }

        _requestedDirection = newDirection;
    }



    // ХЭЛПЕР: Проверяет, близки ли координаты к целым числам (линиям сетки)
    private bool IsAlignedToGrid(Vector2 position)
    {
        // Находим расстояние от текущей координаты до ближайшего целого числа
        float distanceToNearestX = Mathf.Abs(position.x - Mathf.Round(position.x));
        float distanceToNearestY = Mathf.Abs(position.y - Mathf.Round(position.y));

        // Если оба расстояния меньше допуска, считаем танк выровненным
        return distanceToNearestX < GridSnapTolerance && distanceToNearestY < GridSnapTolerance;
    }
}
