using UnityEngine;

public enum TankState
{
    Spawning,  // Мигает при появлении, неуязвим
    Alive,     // Нормальное состояние
    Dying,     // Проигрывает анимацию взрыва
    Dead       // Уничтожен, ожидает удаления
}
