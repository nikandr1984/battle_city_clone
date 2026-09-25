using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(fileName = "TilesetConfig", menuName = "Battle City/Tileset Config")]
public class TilesetConfig : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public TileType type;

        [Tooltip("Тайл для LevelLoader (RuleTile / AnimatedTile")]
        public TileBase tile;

        [Tooltip("Спрайт для превью в редакторе уровней")]
        public Sprite sprite;

        [Tooltip("Фолбэк-цвет: когда спрайт не назначен или конфига нет")]
        public Color color = Color.gray;
    }


    [SerializeField]
    private Entry[] _entries = new Entry[]
    {
        new Entry { type = TileType.Empty,  color = new (0.22f, 0.22f, 0.24f) },
        new Entry { type = TileType.Brick,  color = new (0.72f, 0.27f, 0.16f) },
        new Entry { type = TileType.Steel,  color = new (0.75f, 0.75f, 0.78f) },
        new Entry { type = TileType.Water,  color = new (0.15f, 0.35f, 0.90f) },
        new Entry { type = TileType.Forest, color = new (0.16f, 0.50f, 0.20f) },
        new Entry { type = TileType.Ice,    color = new (0.60f, 0.85f, 0.95f) },
    };

    
    // МЕТОД: линейный поиск
    public Entry Get(TileType type)
    {
        foreach (Entry e in _entries)
        {
            if (e.type == type)
            {                
                return e;
            }
        }
        return null;
    }

    
    // Геттеры
    
    public TileBase GetTile(TileType type) => Get(type)?.tile;
    public Sprite GetSprite(TileType type) => Get(type)?.sprite;

    public Color GetColor(TileType type)
    {
        Entry e = Get(type);
        return e != null ? e.color : Color.gray;
    }
   

}
