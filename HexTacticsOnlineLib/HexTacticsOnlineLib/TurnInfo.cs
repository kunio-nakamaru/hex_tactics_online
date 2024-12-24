using System;
using System.Collections.Generic;
using UnityEngine;
namespace HexTacticsOnline.Lib
{
    [Serializable]
    public class TurnInfo
    {
        public SerializableDictionary<HexVector2, Tile> KnownMap;
        public SerializableDictionary<byte, EnemyUnit> EnemyUnits;
        public SerializableDictionary<byte, PlayerUnit> PlayerUnits;
        public List<News> News;
        public override string ToString()
        {
            return JsonUtility.ToJson(this);

        }
    }
}