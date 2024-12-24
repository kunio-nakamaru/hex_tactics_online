using System;
using System.Collections.Generic;
using System.Text;

namespace HexTacticsOnline.Lib
{
    public enum TileType { Ground = 0, Wall = 200, Unknown = 255 }

    [Serializable]
    public class Tile
    {

        public static Tile UnKnown = new Tile() { Pos = new HexVector2() { X = byte.MaxValue, Y = byte.MaxValue }, TileType = TileType.Unknown };
        public HexVector2 Pos = new HexVector2();
        //public List<Tile> Connected = new List<Tile>() { null, null, null, null, null, null };
        public byte UnitAtID = byte.MaxValue;
        public byte DungeonID = byte.MaxValue;
        public TileType TileType = TileType.Ground; //unknown

        public bool InPlayerSight = false; //unknown
        public List<byte> TileState = new List<byte>();
        public bool Updated = false;


        public bool CanPlaceUnit { get { return UnitAtID == byte.MaxValue && (byte)TileType < 100; } }

        public Unit UnitOn
        {
            get
            {
                var d = Database.Instance.QueryDungeon(this.DungeonID);
                if (d != null)
                    return d.QueryUnit(UnitAtID);
                return null;
            }
        }
        public static bool operator ==(Tile a, Tile b)
        {
            if (b is null)
                return false;
            return a.Pos == b.Pos;
        }
        public static bool operator !=(Tile a, Tile b)
        {
            if (b is null)
                return true;
            return a.Pos != b.Pos;
        }

        public void ApplyEffectForThisTurn()
        {
            foreach (var item in ApplyingEffectsThisTurn)
            {
                if (TileStates.ContainsKey(item.Key))
                {
                    TileStates[item.Key] += item.Value;
                }
                else
                    TileStates.Add(item.Key, item.Value);
            }

            ApplyingEffectsThisTurn.Clear();
        }

        public void Connect(Tile mapCell, int d)
        {
            //Connected[d] = mapCell;
        }
        SerializableDictionary<UnitState, int> TileStates = new SerializableDictionary<UnitState, int>();
        SerializableDictionary<UnitState, int> ApplyingEffectsThisTurn = new SerializableDictionary<UnitState, int>();
        public void ApplyEffectOnQueue(UnitState key, int value)
        {
            if (ApplyingEffectsThisTurn.ContainsKey(key))
            {
                this.ApplyingEffectsThisTurn[key] += value;
            }
            else
                this.ApplyingEffectsThisTurn.Add(key, value);
            Database.Instance.QueryDungeon(this.DungeonID).LogStateNews(this.DungeonID, key, value);
        }

        public List<Tile> GetTilesAround(int aOERange)
        {
            var list = new List<Tile>();
            list.Add(this);
            Database.Instance.QueryDungeon(this.DungeonID).FillTilesAroundRecursive(ref list, this.Pos, aOERange);
            return list;
        }
    }


    /// <summary>
    /// Flamed will be reduced by Frozen. 
    /// Frozen is converted to Wet by fire
    /// Mudded can be reduced by Frozen
    /// Frozen only work on Liquid Tile. Reduce slight movement.
    /// Mudded reduce movement speed greatly. only work on plain.
    /// Flamed work on non-water tile. 
    /// Poison work on any.
    /// </summary>
    public enum TileStates { Flamed, Frozen, Poisoned, Mudded }

}
