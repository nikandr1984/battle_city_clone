using UnityEngine;
using UnityEditor;
using Unity.Hierarchy.Editor;
using UnityEngine.Rendering.VirtualTexturing;
using Unity.VisualScripting;

public class LevelEditorWindow : EditorWindow
{    
    private const int FieldWidth = LevelData.FieldWidth;   // Ширина игрового поля
    private const int FieldHeight = LevelData.FieldHeight;  // Высота игрового поля

    private const float ToolbarHeight = 24f;  // Высота тулбара в пикселях
    private const float StatusHeight = 20f;   // Высота статус бара в пикселях
    private const float PaletteWidth = 110f;  // Ширина панели кистей
    private const float MinCellSize = 8f;     // Мин.размер клетки в пикселях
    private const float MaxCellSize = 32f;    // Макс.размер клетки в пикселях

    private enum ToolType { Brush, Base, P1, P2, E1, E2, E3 }  // Типы инструментов редактора
    

    [SerializeField] private LevelData _level;                      // Ссылка на ассет с данными уровня
    [SerializeField] private float _cellSize = 16f;                 // Размер клетки в пикселях
    [SerializeField] private bool _showGrid = true;                 // Статус отрисовки сетки
    [SerializeField] private TileType _brush = TileType.Brick;      // Текущая кисть
    [SerializeField] private bool _blockMode;                       // Режим штампа 2х2
    [SerializeField] private ToolType _activeTool = ToolType.Brush; // Активный инструмент




    private Rect _canvasRect;                        // Область холста
    private Rect _fieldRect;                         // Область поля рисования
    private Vector2Int _cursorCell = new(-1, -1);    // Ячейка под курсором

    private static readonly TileType[] AllTiles =
        (TileType[])System.Enum.GetValues(typeof(TileType)); // Кэш всех типов тайлов


    // Поля состояния штриха
    private bool _isPainting;            // Мышь зажата и штрих идет
    private bool _strokeUndoRegistered;  // undo-снимок сделан (лениво)
    private TileType _strokeBrush;       // Кисть штриха: ЛКМ = _brush, ПКМ = Empty
    private Vector2Int _lastPaintedCell; // Для интерполяции линии

    // Стили
    private static GUIStyle _s_brushBtn;          // Стиль обычных кнопок кистей
    private static GUIStyle _s_brushBtnSel;       // Стиль выделенной кнопки кисти
    private static GUIStyle _s_markerLabel;       // Стиль для текста на маркерах
    private static GUIStyle _s_blockToggle;       // Стиль для тогла блока
   



    // Палитра тайлов
    private static readonly Color BgColor =     new(0.16f, 0.16f, 0.16f);        // Цвет окна
    private static readonly Color CellColorA =  new(0.22f, 0.22f, 0.24f);        // Цвет ячейки А
    private static readonly Color CellColorB =  new(0.25f, 0.25f, 0.28f);        // Цвет ячейки В
    private static readonly Color GridColor =   new(0.40f, 0.40f, 0.49f, 0.35f); // Цвет сетки
    private static readonly Color BorderColor = new(0.90f, 0.60f, 0.10f);        // Цвет рамки    

    // Палитра маркеров
    private static readonly Color MarkerBase =   new(0.95f, 0.75f, 0.20f); // Маркер базы
    private static readonly Color MarkerPlayer = new(0.30f, 0.80f, 0.35f); // Маркер игроков    
    private static readonly Color MarkerEnemy =  new(0.90f, 0.30f, 0.30f); // Маркер врагов
   


    // Создает окно, если его нет
    [MenuItem("Tools/Battle_City_Clone/Level Editor")]
    private static void ShowWindow()
    {
        var window = GetWindow<LevelEditorWindow>("BC Level Editor");
        window.minSize = new Vector2(680, 560);
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
        DrawPalette();
        DrawCanvas();
        HandleInput();
        DrawStatusBar();
    }


    // --- КОМПОНОВКА ЭЛЕМЕНТОВ ---
    private void ComputeLayout()
    {
        // 1. Прямоугольник описывающий всю центральную область
        _canvasRect = new Rect(PaletteWidth, ToolbarHeight,
                               position.width - PaletteWidth,
                               position.height - ToolbarHeight - StatusHeight);

        // 2. Вычисление размера поля в пикселях
        Vector2 fieldSize = new(FieldWidth * _cellSize, FieldWidth * _cellSize);

        // 3. Центрирование поля
        float ox = _canvasRect.x + Mathf.Max(0f, (_canvasRect.width - fieldSize.x) / 2f);
        float oy = _canvasRect.y + Mathf.Max(0f, (_canvasRect.height - fieldSize.y) / 2f);
        
        // 4. Создание прямоугольника игрового поля
        _fieldRect = new Rect(ox, oy, fieldSize.x, fieldSize.y);
    }


    // --- ТУЛБАР ---
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
                               GUILayout.Width(200))   // Размер поля
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
        
        // 8. Закрываем горизонтальную зону
        GUILayout.EndHorizontal();

        // 9. Закрываем зону тулбара
        GUILayout.EndArea();
    }


    // --- ПАЛИТРА ---
    private void DrawPalette()
    {
        // 1. Задаем координаты и размер области палитры
        Rect paletteArea = new Rect(0, ToolbarHeight, 
                               PaletteWidth, position.height - ToolbarHeight - StatusHeight);
        
        // 2. Начинаем зону палитры
        GUILayout.BeginArea(paletteArea);
        {
            // 3. Делаем метку Кисти
            GUILayout.Label("Кисти", EditorStyles.boldLabel);

            // 4. Создаем безопасные копии стилей кистей
            EnsureBrushStyles();

            // 5. Отрисовываем кнопки тайлов
            foreach (TileType t in AllTiles)
            {
                bool isSelected = _activeTool == ToolType.Brush && _brush == t;

                GUI.backgroundColor = ColorForTile(t);

                bool clicked = GUILayout.Button(t.ToString(), isSelected ? _s_brushBtnSel : _s_brushBtn,
                    GUILayout.Height(20));

                GUI.backgroundColor = Color.white;

                if (clicked)
                {
                    _brush = t;
                    _activeTool = ToolType.Brush;
                }                
            }

            

            // 6. Переключатель режима штампа
            GUILayout.Space(10);

            _blockMode = GUILayout.Toggle(_blockMode, "Block 2x2", _s_blockToggle, GUILayout.Height(20));            
            
            
            // 7. Отрисовываем кнопки маркеров
            GUILayout.Space(10);
            GUILayout.Label("Маркеры", EditorStyles.boldLabel);

            DrawToolButton("База (В)", ToolType.Base, MarkerBase);
            DrawToolButton("Игрок 1",  ToolType.P1, MarkerPlayer);
            DrawToolButton("Игрок 2",  ToolType.P2, MarkerPlayer);
            DrawToolButton("Враг 1",   ToolType.E1, MarkerEnemy);
            DrawToolButton("Враг 2",   ToolType.E2, MarkerEnemy);
            DrawToolButton("Враг 3",   ToolType.E3, MarkerEnemy);
        }

        GUILayout.EndArea();
    }

    // МЕТОД: создание универсальной кнопки инструмента
    private void DrawToolButton(string label, ToolType tool, Color bgColor)
    {
        bool isSelected = _activeTool == tool;
        GUI.backgroundColor = isSelected ? bgColor : bgColor * 0.5f;
        if (GUILayout.Button(label, isSelected ? _s_brushBtnSel : _s_brushBtn, GUILayout.Height(20)))
        {
            _activeTool = tool;
        }
        GUI.backgroundColor = Color.white;
    }


    // МЕТОД: кэширует стили текстов кнопок
    private static void EnsureBrushStyles()
    {
        // Предохранитель
        if (_s_brushBtn != null) return;

        // Стили текста кнопок
        _s_brushBtn = new GUIStyle(EditorStyles.miniButton);
        _s_brushBtnSel = new GUIStyle(EditorStyles.miniButton);        
        _s_brushBtnSel.fontStyle = FontStyle.Bold;
        _s_brushBtnSel.normal.textColor = new Color(1f, 0.8f, 0.3f);

        // Стили текста тогла
        _s_blockToggle = new GUIStyle(EditorStyles.miniButton);
        _s_blockToggle.normal.textColor = EditorStyles.miniButton.normal.textColor;
        _s_blockToggle.onNormal.textColor = new Color(1f, 0.8f, 0.3f);
        _s_blockToggle.fontStyle = FontStyle.Bold;



    }

    // МЕТОД: стиль для текста на маркерах
    private static void EnsureMarkerLabelStyle()
    {
        if (_s_markerLabel != null) return;

        _s_markerLabel = new GUIStyle(EditorStyles.boldLabel);
        _s_markerLabel.alignment = TextAnchor.MiddleCenter;
        _s_markerLabel.normal.textColor = Color.black;
        _s_markerLabel.fontSize = 10;
        _s_markerLabel.fontStyle = FontStyle.Bold;
    }




    // --- СТАТУСБАР ---
    
    // ОСНОВНОЙ МЕТОД: рисует статусбар
    private void DrawStatusBar()
    {
        // 1. Начало зоны статусбара
        GUILayout.BeginArea(new Rect(10, position.height - StatusHeight,
                                                              position.width, StatusHeight));

        // 2. Определяем координаты ячейки и сохраняем в переменную        
        string statText;        

        if (IsInBounds(_cursorCell))
        {
            // Определяем тип маркера
            string markerType = MarkerAt(_cursorCell); 

            // Определяем тип тайла
            string tileType = _level != null
                ? _level.GetTile(_cursorCell.x, _cursorCell.y).ToString()
                : "-";
                        
            statText = $"Cell: ({_cursorCell.x}, {_cursorCell.y}) Tile: {tileType}";

            if (!string.IsNullOrEmpty(markerType))
            {
                statText += $" | Marker: {markerType}";
            }

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


    // ДОП.МЕТОД: определяет какой маркер покрывает данную клетку
    private string MarkerAt(Vector2Int cell)
    {
        // Если нет данных уровня
        if (_level == null) return "";
        
        // Если база
        if (Inside(_level.BasePosition, cell)) return "Base";

        // Если спавн врагов
        var enemies = _level.EnemySpawns;
        for (int i = 0; i < enemies.Count; i++)
        {
            if (Inside(enemies[i], cell)) return "Enemy" + (i + 1);
        }

        // Если спавн игроков
        var players = _level.PlayerSpawns;
        for (int i = 0; i < players.Count; i++)
        {
            if (Inside(players[i], cell)) return "Player" + (i + 1);
        }

        // Если не маркер
        return "";

    }


    // ДОП.МЕТОД: находится ли клетка cell внутри квадрата 2х2 (верхний левый угол в точке origin)
    private static bool Inside(Vector2Int origin, Vector2Int cell) =>
                                                                     cell.x >= origin.x &&
                                                                     cell.x < origin.x + 2 &&
                                                                     cell.y >= origin.y &&
                                                                     cell.y < origin.y + 2;
    




    // --- КАНВАС ---
    
    // ОСНОВНОЙ МЕТОД: рисует канвас
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


            // 5. Рисуем маркеры
            DrawMarkers();

            // 6. Рисуем ховер и гост-превью
            DrawHoverAndGhost();                       
            
            // 7. Напоминание назначить LevelData
            if(_level == null)
            {
                GUI.Label(new Rect(_canvasRect.x, _canvasRect.y + 2, _canvasRect.width, 18),
                    "Назначь LevelData в тулбаре", EditorStyles.centeredGreyMiniLabel);
            }
        }
    }


    // ДОП.МЕТОД: какого цвета должна быть клетка с координатами (х,у)
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

    
    // ДОП.МЕТОД: определяет цвет тайлов
    private static Color ColorForTile(TileType t) => t switch
    {
        TileType.Brick  => new Color(0.72f, 0.27f, 0.16f),
        TileType.Steel  => new Color(0.75f, 0.75f, 0.78f),
        TileType.Water  => new Color(0.15f, 0.35f, 0.90f),
        TileType.Forest => new Color(0.16f, 0.50f, 0.20f),
        TileType.Ice    => new Color(0.60f, 0.85f, 0.95f),
        _               => CellColorA,
    };


    // ДОП.МЕТОД: все маркеры рисуем здесь, чтобы не засерать метод отрисовки канваса
    private void DrawMarkers()
    {
        // 1. Проверяем существуют ли данные уровня
        if (_level == null) return;
        
        // 2. Задаем стиль текста маркеров
        EnsureMarkerLabelStyle();

        // 3. Рисуем маркер базы
        DrawMarker(_level.BasePosition, "B", MarkerBase);

        // 4. Рисуем маркеры спавна врагов
        var enemies = _level.EnemySpawns;
        for (int i = 0; i < enemies.Count; i++)
        {
            DrawMarker(enemies[i], "E" + (i + 1), MarkerEnemy);
        }

        // 4. Рисуем маркеры спавна игроков
        var player = _level.PlayerSpawns;
        for (int i = 0; i < player.Count; i++)
        {
            DrawMarker(player[i], "P" + (i + 1), MarkerPlayer);
        }
    }


    // ДОП.МЕТОД: рисует один маркер 2х2 с буквой и рамкой
    private void DrawMarker(Vector2Int origin, string label, Color bgColor)
    {
        // 1. Проверяем находится ли клетка в пределах игрового поля
        if (!IsInBounds(origin)) return;
        
        // 2. Задаем характеристики маркера в виде прямоугольника 2х2
        Rect rect = new Rect(_fieldRect.x + origin.x * _cellSize,
                             _fieldRect.y + origin.y * _cellSize,
                             _cellSize * 2,
                             _cellSize * 2);

        // 3. Рисуем маркер (прямоугольник, цвет)
        EditorGUI.DrawRect(rect, bgColor);             

        // 4. Делаем подпись маркера (прямоугольник, подпись, стиль подписи)
        GUI.Label(rect, label, _s_markerLabel);
    }


    // ДОП.МЕТОД: ховер + гост-превью активного инструмента
    private void DrawHoverAndGhost()
    {
        if (!IsInBounds(_cursorCell)) return;

        Vector2Int origin = StampOrigin(_cursorCell);
                
        
        if (_activeTool == ToolType.Brush) // Ховер для кистей
        {
            int sizeRatio = _blockMode ? 2 : 1;
           
            Rect brushRect = new (_fieldRect.x + origin.x * _cellSize, 
                                  _fieldRect.y + origin.y * _cellSize,
                                  _cellSize *  sizeRatio, _cellSize * sizeRatio);

            Color brushColor = ColorForTile(_brush);
            brushColor.a = 0.35f;   
                        
            EditorGUI.DrawRect(brushRect, brushColor);
        }
        else // Ховер для маркеров
        {            
            // Задаем прямоугольник маркера
            Rect markerRect = new(_fieldRect.x + origin.x * _cellSize,
                                  _fieldRect.y + origin.y * _cellSize,
                                  _cellSize * 2, _cellSize * 2);

            // Выбор цвета маркера
            Color markerColor = _activeTool switch
            {
                ToolType.Base              => MarkerBase,
                ToolType.P1 or ToolType.P2 => MarkerPlayer,
                _                          => MarkerEnemy,
            };

            // Прозрачность маркера
            markerColor.a = 0.5f;

            // Отрисовываем маркер
            EditorGUI.DrawRect(markerRect, markerColor);

            // Задаем стиль текста на маркере
            EnsureMarkerLabelStyle();
            

            // Задаем надпись
            string markerLabel = _activeTool switch
            {
                ToolType.Base => "B",
                ToolType.P1 => "P1",
                ToolType.P2 => "P2",
                ToolType.E1 => "E1",
                ToolType.E2 => "E2",
                ToolType.E3 => "E3",
                _ => ""

            };
            
            // Добавляем полупрозрачность надписи
            Color previous = _s_markerLabel.normal.textColor;
            _s_markerLabel.normal.textColor = new Color(previous.r, previous.g, previous.b, 0.6f);
            
            // Отрисовываем текст
            GUI.Label(markerRect, markerLabel, _s_markerLabel);
            _s_markerLabel.normal.textColor = previous;
        }
    }


    // --- ОБРАБОТКА ВВОДА ---
    private void HandleInput()
    {
        // 1. Получение текущего события
        Event e = Event.current;

        // 2. Обработка события
        switch (e.type)
        {
            case EventType.MouseMove:
            {
                UpdateCursor(CellAt(e.mousePosition));
                break;
            }                

            case EventType.MouseDown:
            {
                    if (_level == null) break;

                    Vector2Int cell = CellAt(e.mousePosition);

                    if (!IsInBounds(cell)) break;

                    if(_activeTool == ToolType.Brush)
                    {
                        if (e.button != 0 && e.button != 1) break;
                        _isPainting = true;
                        _strokeUndoRegistered = false;
                        _strokeBrush = e.button == 0 ? _brush : TileType.Empty;
                        _lastPaintedCell = cell;
                        PaintCell(cell, _strokeBrush);
                    }
                    else if (e.button == 0)
                    {
                        ApplyMarker(cell, _activeTool);
                    }

                    e.Use();
                    Repaint();
                    break;                   
            }

            case EventType.MouseDrag:
            {
                UpdateCursor(CellAt(e.mousePosition));  
                if (!_isPainting || _activeTool != ToolType.Brush) break;

                Vector2Int cell = CellAt(e.mousePosition);

                if (IsInBounds(cell) && cell != _lastPaintedCell)
                {
                    PaintLine(_lastPaintedCell, cell, _strokeBrush);
                    _lastPaintedCell = cell;
                    e.Use();
                    Repaint();
                }
                break;
            }

            case EventType.MouseUp:
            {
               _isPainting = false;
               break;
            }

            case EventType.MouseLeaveWindow:
            {
               _isPainting = false;
               UpdateCursor(new Vector2Int(-1, -1));
               break;

            }
        }
    }


    // ДОП.МЕТОД: ховер курсора
    private void UpdateCursor(Vector2Int cell)
    {
        if (cell == _cursorCell) return;
        _cursorCell = cell;
        Repaint();
    }

    
    // ДОП.МЕТОД: раскрашивает клетки
    private void PaintCell(Vector2Int cell, TileType brush)
    {
        Vector2Int origin = StampOrigin(cell);
        int sizeRatio = _blockMode ? 2 : 1;

        for (int dy = 0; dy < sizeRatio; dy++)
        for (int dx = 0; dx < sizeRatio; dx++)
        {
            Vector2Int c = new (origin.x + dx, origin.y + dy);
            
            if (!IsInBounds(c)) continue;
            if (_level.GetTile(c.x, c.y) == brush) continue;

            // Снимок undo только перед первым реальным изменением штриха
            if (!_strokeUndoRegistered)
            {
               Undo.RegisterCompleteObjectUndo(_level, "Paint level");
               _strokeUndoRegistered = true;
            }

            // Меняем тип клетки в массиве LevelData
            _level.SetTile(c.x, c.y, brush);

            // помечаем ассет как «измененный», чтобы Unity предложила его сохранить
            EditorUtility.SetDirty(_level);
        }          
    }


    // ДОП.МЕТОД: рисует линии по алгоритму Брезенхэма
    private void PaintLine(Vector2Int from, Vector2Int to, TileType brush)
    {
        int x0 = from.x, y0 = from.y;                         // Начальная точка
        int x1 = to.x,   y1 = to.y;                           // Конечная точка
        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0); // Длины проекций отрезков
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;     // Напрвление шага
        int err = dx - dy;                                    // Начальная ошибка

        while (true)
        {
            PaintCell(new Vector2Int(x0, y0), brush);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 <  dx) { err += dx; y0 += sy; }
        }
    }


    // ДОП.МЕТОД: выравнивает клетки к началу блока 2х2
    private Vector2Int StampOrigin(Vector2Int cell) =>
        _blockMode ? new Vector2Int(cell.x & ~1, cell.y & ~1) : cell;
       

    // ДОП.МЕТОД: установка маркера с undo
    private void ApplyMarker(Vector2Int cell, ToolType tool)
    {
        Vector2Int origin = StampOrigin(cell);

        Undo.RegisterCompleteObjectUndo(_level, "Move marker");

        switch (tool)
        {
            case ToolType.Base:
                _level.SetBasePosition(origin);
                    break;
            case ToolType.P1:
                _level.SetPlayerSpawn(0, origin);
                break;
            case ToolType.P2:
                _level.SetPlayerSpawn(1, origin);
                break;
            case ToolType.E1:
                _level.SetEnemySpawn(0, origin);
                break;
            case ToolType.E2:
                _level.SetEnemySpawn(1, origin);
                break;
            case ToolType.E3:
                _level.SetEnemySpawn(2, origin);
                break;
        }

        EditorUtility.SetDirty(_level);
    }





    

    // --- ХЕЛПЕРЫ ---

    // I. Конвертор координат тайлов (логические в пиксели)
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
