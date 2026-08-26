using UnityEngine;
using UnityEditor;
using Unity.Hierarchy.Editor;

public class LevelEditorWindow : EditorWindow
{    
    private const int FieldWidth = LevelData.FieldWidth;   // Ширина игрового поля
    private const int FieldHeight = LevelData.FildHeight;  // Высота игрового поля

    private const float ToolbarHeight = 24f;  // Высота тулбара в пикселях
    private const float StatusHeight = 20f;   // Высота статус бара в пикселях
    private const float MinCellSize = 8f;     // Мин.размер клетки в пикселях
    private const float MaxCellSize = 32f;    // Макс.размер клетки в пикселях

    [SerializeField] private LevelData _level;
    [SerializeField] private float _cellSize = 16f;
    [SerializeField] private bool _showGrid = true;

    private Rect _canvasRect;   // Область холста
    private Rect _fieldRect;    // Область поля рисования
    private Vector2Int _cursorCell = new(-1, -1);

    private static readonly Color BgColor = new(0.16f, 0.16f, 0.16f);          // Цвет окна
    private static readonly Color CellColorA = new(0.22f, 0.22f, 0.24f);       // Цвет ячейки А
    private static readonly Color CellColorB = new(0.25f, 0.25f, 0.28f);       // Цвет ячейки В
    private static readonly Color GridColor = new(0.40f, 0.40f, 0.49f, 0.35f); // Цвет сетки
    private static readonly Color BorderColor = new(0.90f, 0.60f, 0.10f);      // Цвет рамки
    private static readonly Color HoverColor = new(1f, 1f, 1f, 0.15f);         // Цвет подсветки


    
    // Создает окно, если его нет
    [MenuItem("Tools/Battle_City_Clone/Level Editor")]
    private static void ShowWindow()
    {
        var window = GetWindow<LevelEditorWindow>("BC Level Editor");
        window.minSize = new Vector2(560, 560);
        window.Show();
    }


    private void OnEnable()
    {
        wantsMouseMove = true;
    }


    private void OnGUI()
    {
        ComputeLayout();
        DrawToolbar();
        DrawCanvas();
        HandleInput();
        DrawStatusBar();
    }


    // --- РАСЧЕТЫ РАЗМЕРОВ И ЦЕНТРИРОВАНИЕ ПОЛЯ РИСОВАНИЯ ОТНОСИТЕЛЬНО ХОЛСТА ---
    private void ComputeLayout()
    {
        // 1. Прямоугольник описывающий всю центральную область
        _canvasRect = new Rect(0, ToolbarHeight, position.width,
                               position.height - ToolbarHeight - StatusHeight);

        // 2. Вычисление размера поля в пикселях
        Vector2 fieldSize = new(FieldWidth * _cellSize, FieldWidth * _cellSize);

        // 3. Центрирование поля
        float ox = _canvasRect.x + Mathf.Max(0f, (_canvasRect.width - fieldSize.x) / 2f);
        float oy = _canvasRect.y + Mathf.Max(0f, (_canvasRect.height - fieldSize.y) / 2f);
        
        // 4. Создание прямоугольника игрового поля
        _fieldRect = new Rect(ox, oy, fieldSize.x, fieldSize.y);
    }


    // --- РИСУЕМ ТУЛБАР С АВТОМАТИЧЕСКОЙ КОМПОНОВКОЙ ---
    private void DrawToolbar()
    {
        // 1. Команда начала зоны тулбара
        GUILayout.BeginArea(new Rect(0, 0, position.width, ToolbarHeight), EditorStyles.toolbar);
        
        // 2. Делаем зону горизонтальной
        GUILayout.BeginHorizontal();

        // 3. Создаем поле для перетаскивания ассета
        LevelData picked = EditorGUILayout.ObjectField(
                               _level,                 // Текущее значение
                               typeof(LevelData),      // Допустимый тип
                               false,                  // Разрешить ли объекты сцены
                               GUILayout.Width(160))   // Размер поля
                               as LevelData;           // Каст чтобы возвращал только этот тип
       
        if (picked != _level)
        {
            _level = picked;
            Repaint();
        }

        
        // 4. Делаем метку Zoom
        GUILayout.Label("Zoom", GUILayout.Width(40));

        // 5. Делаем слайдер зума
        _cellSize = GUILayout.HorizontalSlider(_cellSize, MinCellSize, MaxCellSize,
                                                                            GUILayout.Width(160));
            
        // 6. Делаем метку с текущим значением зума
        GUILayout.Label($"{_cellSize:0}px", GUILayout.Width(40));

        // 7. Делаем переключатель сетки
        _showGrid = GUILayout.Toggle(_showGrid, "Grid", EditorStyles.toolbarButton, 
                                                                             GUILayout.Width(60));
       
        // 8. 
        if (GUILayout.Button("Fill test", EditorStyles.toolbarButton, GUILayout.Width(70)))
        {
            FillTestPattern();
        }
        
        // 7. Закрываем горизонтальную зону
        GUILayout.EndHorizontal();

        // 8. Закрываем зону тулбара
        GUILayout.EndArea();
    }


    // --- РИСУЕМ СТАТУСБАР ---
    private void DrawStatusBar()
    {
        // 1. Начало зоны статусбара
        GUILayout.BeginArea(new Rect(10, position.height - StatusHeight,
                                                              position.width, StatusHeight));

        // 2. Определяем координаты ячейки и сохраняем в переменную
        
        string statText;        

        if (IsInBounds(_cursorCell))
        {
            string tileType = _level != null
                ? _level.GetTile(_cursorCell.x, _cursorCell.y).ToString()
                : "-";
                        
            statText = $"Cell: ({_cursorCell.x}, {_cursorCell.y}) Tile: {tileType}";
        }
        else
        {
            statText = "Cell: -";
        }

        // 3. Создаем метку с координатами ячейки
        GUILayout.Label(statText);

        // 4. Закрываем зону статусбара
        GUILayout.EndArea();            
    }



    // --- РИСУЕМ ХОЛСТ ---
    private void DrawCanvas()
    {
        // 1. Заливка всей доступной области канваса серым цветом
        EditorGUI.DrawRect(_canvasRect, BgColor);

        // 2. Отрисовка клеток шахматкой
        for (int y = 0; y < FieldHeight; y++)
        for (int x = 0; x < FieldWidth; x++)
        {
            EditorGUI.DrawRect(CellRect(x, y), CellColor(x, y));           
        }

        // 3. Отрисовка сетки (линий)
        if (_showGrid)
        {
            for (int x = 1; x < FieldWidth; x++)
            {
                EditorGUI.DrawRect(new Rect(_fieldRect.x + x * _cellSize, _fieldRect.y, 1,
                                             _fieldRect.height), GridColor);
            }

            for (int y = 1; y < FieldHeight; y++)
            {
                EditorGUI.DrawRect(new Rect(_fieldRect.x, _fieldRect.y + y * _cellSize,
                                            _fieldRect.width, 1), GridColor);
            }


            // 4. Отрисовка рамки поля
            EditorGUI.DrawRect(new Rect(_fieldRect.x, _fieldRect.yMin, _fieldRect.width, 1), 
                               BorderColor); // Верх
            EditorGUI.DrawRect(new Rect(_fieldRect.x, _fieldRect.yMax - 1, _fieldRect.width, 1),
                               BorderColor); // Низ
            EditorGUI.DrawRect(new Rect(_fieldRect.xMin, _fieldRect.y, 1, _fieldRect.height),
                               BorderColor); // Лево
            EditorGUI.DrawRect(new Rect(_fieldRect.xMax - 1, _fieldRect.y, 1, _fieldRect.height),
                               BorderColor); // Право

            
            // 5. Подсветка клетки под курсором
            if (IsInBounds(_cursorCell))
            {
                EditorGUI.DrawRect(CellRect(_cursorCell.x, _cursorCell.y), HoverColor);
            }
            
            // 6. Напоминание назначить LevelData
            if(_level == null)
            {
                GUI.Label(new Rect(_canvasRect.x, _canvasRect.y + 2, _canvasRect.width, 18),
                    "Назначь LevelData в тулбаре", EditorStyles.centeredGreyMiniLabel);
            }
        }
    }


    // МЕТОД: какого цвета должна быть клетка с координатами (х,у)
    private Color CellColor(int x, int y)
    {
        // 1. Если есть загруженный уровень и таил не пустой - рисуем цветом
        if (_level != null)
        {
            TileType t = _level.GetTile(x, y);

            if (t != TileType.Empty)
            {
                return ColorForTile(t);
            }               
        }

        // 2. Если уровень не загружен или таил пустой - рисуем шахматкой
        return (x + y) % 2 == 0 ? CellColorA : CellColorB;
    }

    private static Color ColorForTile(TileType t) => t switch
    {
        TileType.Brick  => new Color(0.72f, 0.27f, 0.16f),
        TileType.Steel  => new Color(0.75f, 0.75f, 0.78f),
        TileType.Water  => new Color(0.15f, 0.35f, 0.90f),
        TileType.Forest => new Color(0.16f, 0.50f, 0.20f),
        TileType.Ice    => new Color(0.60f, 0.85f, 0.95f),
        _               => CellColorA,
    };



    // --- МЕТОД ОБРАБОТКИ ВВОДА ---
    private void HandleInput()
    {
        // 1. Получение текущего события
        Event e = Event.current;

        // 2. Обработка движения и перетаскивания
        if (e.type == EventType.MouseMove || e.type== EventType.MouseDrag)
        {
            Vector2Int cell = CellAt(e.mousePosition);

            if (cell != _cursorCell)
            {
                _cursorCell = cell;
                Repaint();
            }
        }
        else if (e.type == EventType.MouseLeaveWindow)
        {
            _cursorCell = new(-1, -1);
            Repaint();
        }
    }
    

    // --- ЗАГЛУШКА ---

    private void FillTestPattern()
    {
        if (_level == null) return;

        Undo.RegisterCompleteObjectUndo(_level, "Test fill");

        for (int y = 0; y < FieldHeight; y++)
        for (int x = 0; x < FieldWidth; x++)
        {
            TileType t = TileType.Empty;
            if      (x >= 4  && x <= 7  && y >= 4  && y <= 7)  t = TileType.Brick;
            else if (x >= 10 && x <= 13 && y >= 4  && y <= 7)  t = TileType.Steel;
            else if (x >= 16 && x <= 19 && y >= 4  && y <= 7)  t = TileType.Water;
            else if (x >= 4  && x <= 7  && y >= 10 && y <= 13) t = TileType.Forest;
            else if (x >= 10 && x <= 13 && y >= 10 && y <= 13) t = TileType.Ice;
            _level.SetTile(x, y, t);
        }

        EditorUtility.SetDirty(_level);

        Repaint();
    }






    // --- ХЕЛПЕРЫ ---

    // I. Конвертор координат
    private Rect CellRect(int x, int y)
    {
        return new(_fieldRect.x + x * _cellSize, // Левый край (Х)
                   _fieldRect.y + y * _cellSize, // Верхний край (Y)
                   _cellSize,                    // Ширина
                   _cellSize);                   // Высота

    }


    // II. Переводит экранные координаты мыши в координаты сетки
    private Vector2Int CellAt(Vector2 mousePos)
    {
        // 1. Проверяем находится ли курсор мыши внутри поля рисования
        if (!_fieldRect.Contains(mousePos))
        {
            return new(-1, -1);
        }

        // 2. Переход от абсолютных координат к относительным
        int x = Mathf.FloorToInt((mousePos.x - _fieldRect.x) / _cellSize);
        int y = Mathf.FloorToInt((mousePos.y - _fieldRect.y) / _cellSize);

        return (x >= 0 && x < FieldWidth && y >= 0 && y < FieldHeight)
            ? new Vector2Int(x, y)
            : new Vector2Int(-1, -1);
    }


    // III. Проверка - находится ли клетка внутри игрового поля
    private static bool IsInBounds(Vector2Int cell)
    {
        return cell.x >= 0 && 
               cell.y >= 0 && 
               cell.x < FieldWidth && 
               cell.y < FieldHeight;
    }
}
