using System;
using System.Collections.Generic;

namespace HexTacticsOnline.Lib
{
    [Serializable]
    public struct HexDistanceCalculationData
    {
        public HexVector2 Pos;
        public int Power;

        public HexDistanceCalculationData[] GetPositionsAround(bool simulateEven)
        {

            bool isReallyEven = Pos.Y % 2 == 0;
            bool isVirtuallyEven = !(isReallyEven ^ simulateEven);
            HexDistanceCalculationData[] result = new HexDistanceCalculationData[6];
            for (int i = 0; i < 6; i++)
            {
                HexVector2 pos;
                if (isVirtuallyEven)
                {
                    pos = HexVector2.DirectionVectorEven[i];
                }
                else
                    pos = HexVector2.DirectionVectorOdd[i];
                result[i] = new HexDistanceCalculationData() { Pos = this.Pos + pos, Power = this.Power - 1 };
            }
            return result;
        }
    }

    
}