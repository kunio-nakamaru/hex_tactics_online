using System;
using System.Collections.Generic;
using System.Text;

namespace HexTacticsOnline.Lib
{
    public enum HexDirection
    {
        Unset = -1, NE = 0, E = 1, SE = 2, SW = 3, W = 4, NW = 5
    }
    public class HexDirectionData
    {
        public HexVector2 Vector2Int;
        public HexDirection Direction;
    }
}
