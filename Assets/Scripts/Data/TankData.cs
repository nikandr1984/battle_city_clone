using UnityEngine;

[CreateAssetMenu(fileName = "NewTankData", menuName = "Battle City/Tank Data")]
public class TankData : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("Скорость движения танка в юнитах/сек")]
    [SerializeField] private float _moveSpeed = 2f;


    [Header("Shooting")]
    [Tooltip("Скорость полета снаряда в юнитах/сек")]
    [SerializeField] private float _bulletSpeed = 4f;

    [Tooltip("Время перезарядки между выстрелами в секундах")]
    [SerializeField] private float _fireRate = 0.5f;

    [Tooltip("Максимальное количество снарядов одновременно на экране")]
    [SerializeField] private int _maxBullets = 1;


    [Header("Health")]
    [Tooltip("Количество попаданий для уничтожения")]
    [SerializeField] private int _maxHealth = 1;


    [Header("Visual")]
    [Tooltip("Тип брони для визуального отображения (0-3)")]
    [SerializeField] private int _armorType = 0;


    [Header("Scoring")]
    [Tooltip("Очки за уничтожение этого танка")]
    [SerializeField] private int _scoreValue = 100;



    // Публичные свойства только для чтения
    public float MoveSpeed => _moveSpeed;
    public float BulletSpeed => _bulletSpeed;
    public float FireRate => _fireRate;
    public int MaxBullets => _maxBullets;
    public int MaxHealth => _maxHealth;
    public int MaxArmorType => _armorType;
    public int ScoreValue => _scoreValue;


}
