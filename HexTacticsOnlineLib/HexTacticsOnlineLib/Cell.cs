using System;


namespace HexTacticsOnline.Lib
{
    [Serializable]
    public class HexCell
    {
        public const int MAX_TIER = 5;
        public HexVector2 Position;
        public HexColor HexColor;
        public int Tier;
    }
}