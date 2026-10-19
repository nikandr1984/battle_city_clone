using UnityEngine;
using System;


[RequireComponent(typeof(SpriteRenderer))]
public class Tank : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TankData _tankData;


    [Header("Runtime State")]
    [SerializeField] private Direction _currentDirection = Direction.Up;
    [SerializeField] private TankState _currentState = TankState.Alive;
    [SerializeField] private int _currentHealth;

    private bool _isInitialized = false; // Флаг для защиты от двойной инициализации


    // События для подписки других компонентов
    public event Action<Tank> OnDamaged;
    public event Action<Tank> OnDeath;
    public event Action<Tank, Direction> OnDirectionChanged;
    public event Action<Tank, TankState> OnStateChanged;


    // Публичные свойства для чтения состояния
    public TankData TankData => _tankData;
    public Direction CurrentDirection => _currentDirection;
    public TankState CurrentState => _currentState;
    public int CurrentHealth => _currentHealth;
    public bool IsAlive => _currentState == TankState.Alive;


    private void Start()
    {
        if (!_isInitialized && _tankData != null)
        {
            Initialize(_tankData, _currentDirection);
        }
        else if (!_isInitialized && _tankData == null)
        {
            Debug.LogError($"Tank {gameObject.name} не имеет назначенных TankData!", this);
        }
    }
    

    // Инициализация танка данными. Вызывается при спавне
    public void Initialize(TankData data, Direction startDirection = Direction.Up)
    {
        if (_isInitialized) return; 
        
        if (data == null)
        {
            Debug.LogError($"Tank {gameObject.name}: TankData is null!");
            return;
        }

        _tankData = data;
        _currentHealth = data.MaxHealth;
        _currentDirection = startDirection;
        _isInitialized = true;

        SetState(TankState.Spawning);

        // Визуальная настройка (пока заглушка)
        UpdateVisual();
    }


    // МЕТОД: применение урона к танку
    public void TakeDamage(int damage = 1)
    {
        if (!IsAlive || _currentState == TankState.Spawning) return;

        _currentHealth -= damage;

        OnDamaged?.Invoke(this);

        if (_currentHealth <= 0) Die();

    }


    // МЕТОД: инициация процесса смерти
    private void Die()
    {
        SetState(TankState.Dying);

        OnDeath?.Invoke(this);

        SetState(TankState.Dead);
    }



    // МЕТОД: Изменение направления движения
    public void SetDirection(Direction newDirection)
    {
        if (_currentDirection == newDirection) return;

        _currentDirection = newDirection;

        OnDirectionChanged?.Invoke(this, newDirection);

        // Поворачиваем спрайт (пока заглушка)
        UpdateVisual();
    }


    // МЕТОД: изменение состояния танка
    public void SetState(TankState newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;
        OnStateChanged?.Invoke(this, newState);

        // Здесь будет логика включения/выключения компонентов в зависимости от состояния
    }


    // МЕТОД: обновление визуального представления (заглушка)
    public void UpdateVisual()
    {
        // Пока просто меняем цвет в зависимости от направления
        var spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            switch (_currentDirection)
            {
                case Direction.Up:
                    spriteRenderer.color = Color.red;
                    break;
                case Direction.Down:
                    spriteRenderer.color = Color.gray;
                    break;
                case Direction.Left:
                    spriteRenderer.color = Color.cyan;
                    break;
                case Direction.Right:
                    spriteRenderer.color = Color.yellow;
                    break;
            }
        }
    }



    // -- ТЕСТОВЫЕ МЕТОДЫ --

    private void OnGUI()
    {
        if (gameObject.name.Contains("PlayerTank"))
        {
            GUI.color = Color.black;
            GUI.Box(new Rect(10, 10, 200, 80), "");

            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(15, 15, 190, 70));            
            GUILayout.Label($"HP: {_currentHealth}/{_tankData.MaxHealth}");
            GUILayout.Label($"State: {_currentState}");
            GUILayout.Label($"Direction: {_currentDirection}");
            GUILayout.EndArea();
            GUI.color = Color.white;
        }
    }


    private void OnValidate()
    {
        if (_isInitialized)
        {
            UpdateVisual();
        }
    }

}
