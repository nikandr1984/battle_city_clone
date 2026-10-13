using UnityEngine;

[CreateAssetMenu(fileName = "UILayoutConfig", menuName = "Battle City/UI Layout Config")]
public class UILayoutConfig : ScriptableObject
{
    [Header("Screen Dimension")]
    public const int ScreenWidth = 32;
    public const int ScreenHeight = 28;



    [Header("Field Offsets")]
    public int fieldOffsetX = 2;
    public int fieldOffsetY = 1;



    [Header("Field Sizes")]
    public int fieldWidth = 26;
    public int fieldHeight = 26;



    [Header("Right Panel (HUD)")]
    [Tooltip("X-координата начала правой панели (после поля битвы)")]
    public int hudPanelX = 28; 

    [Tooltip("X-координата первой колонки HUD (внутри панели)")]
    public int hudColumn1X = 29;  

    [Tooltip("X-координата второй колонки HUD (внутри панели)")]
    public int hudColumn2X = 30; 



    [Header("Flag (2x2)")]
    [Tooltip("Координаты левого нижнего угла флага (BL)")]
    public Vector2Int flagOrigin = new Vector2Int(29, 4);

    

    [Header("Stage Number")]
    [Tooltip("Y-координата счетчика уровней")]
    public int stageNumberY = 3;
     


    [Header("Enemy Counter")]
    [Tooltip("Y-координата верхней иконки врага")]
    public int enemyCounterTopY = 25;

    [Tooltip("Y-координата нижней иконки врага")]
    public int enemyCounterBottomY = 17;



    [Header("Player 1 (1P)")]
    [Tooltip("Y-координата метки 1Р")]
    public int player1LabelY = 11;

    [Tooltip("Y-координата счетчика жизней 1Р")]
    public int player1LivesY = 10;
    

    [Header("Player 2 (2P)")]
    [Tooltip("Y-координата метки 2Р")]
    public int player2LabelY = 8;

    [Tooltip("Y-координата счетчика жизней 2Р")]
    public int player2LivesY = 7;

    

    [Header("Helper Properties")]

    // Получить Х-координату левого края поля в мировых координатах экрана
    public int FieldLeftX => fieldOffsetX;


    // Получить Х-координату правого края поля в мировых координатах экрана
    public int FieldRightX => fieldOffsetX + fieldWidth - 1;


    // Получить Y-координату нижнего края поля в мировых координатах экрана
    public int FieldBottomY => fieldOffsetY;


    // Получить Y-координату верхнего края поля в мировых координатах экрана
    public int FieldTopY => fieldOffsetY + fieldHeight - 1;


    // Конвертировать координаты поля (0..25) в координаты экрана
    public Vector2Int FieldToScreen (int fieldX, int fieldY)
    {
        return new Vector2Int(fieldX + fieldOffsetX, fieldY + fieldOffsetY);
    }


    // Конвертировать координаты экрана в координаты поля
    public Vector2Int ScreenToField(int screenX, int screenY)
    {
        return new Vector2Int( screenX -  fieldOffsetX, screenY - fieldOffsetY );
    }


    // Проверить находятся ли координаты в пределах поля
    public bool IsInField(int screenX, int screenY)
    {
        return screenX >= fieldOffsetX &&
               screenX < fieldOffsetX + fieldWidth &&
               screenY >= fieldOffsetY &&
               screenY < fieldOffsetY + fieldHeight;
    }


}
