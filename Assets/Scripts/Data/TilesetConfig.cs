using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(fileName = "TilesetConfig", menuName = "Battle City/Tileset Config")]
public class TilesetConfig : ScriptableObject
{
    [System.Serializable]
    public class EntryTile
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
    private EntryTile[] _entriesTiles = new EntryTile[]
    {
        new EntryTile { type = TileType.Empty,  color = new (0.22f, 0.22f, 0.24f) },
        new EntryTile { type = TileType.Brick,  color = new (0.72f, 0.27f, 0.16f) },
        new EntryTile { type = TileType.Steel,  color = new (0.75f, 0.75f, 0.78f) },
        new EntryTile { type = TileType.Water,  color = new (0.15f, 0.35f, 0.90f) },
        new EntryTile { type = TileType.Trees,  color = new (0.16f, 0.50f, 0.20f) },
        new EntryTile { type = TileType.Ice,    color = new (0.60f, 0.85f, 0.95f) },
    };


    [Tooltip("Спрайты орла для маркера базы (TL, TR, BL, BR)")]
    public Sprite[] eagleSprites = new Sprite[4];


    // МЕТОД: линейный поиск
    public EntryTile Get(TileType type)
    {
        foreach (EntryTile e in _entriesTiles)
        {
            if (e.type == type)
            {                
                return e;
            }
        }
        return null;
    }

    
    // ГЕТТЕРЫ    
    public TileBase GetTile(TileType type) => Get(type)?.tile;
    public Sprite GetSprite(TileType type) => Get(type)?.sprite;

    public Color GetColor(TileType type)
    {
        EntryTile e = Get(type);
        return e != null ? e.color : Color.gray;
    }
   

}
