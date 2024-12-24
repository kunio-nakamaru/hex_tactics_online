using System;
using System.Collections.Generic;
using System.Linq;

namespace HexTacticsOnline.Lib
{
    [Serializable]
    public struct HexVector2
    {
        public static readonly HexVector2 Invalid = new HexVector2(int.MinValue, int.MinValue);
        public int X;
        public int Y;

        public static HexVector2[] DirectionVectorEven = { new HexVector2(0, 1), new HexVector2(1, 0), new HexVector2(0, -1), new HexVector2(-1, -1), new HexVector2(-1, 0), new HexVector2(-1, 1) };
        public static HexVector2[] DirectionVectorOdd = { new HexVector2(1, 1), new HexVector2(1, 0), new HexVector2(1, -1), new HexVector2(0, -1), new HexVector2(-1, 0), new HexVector2(0, 1) };
        public HexVector2(int x, int y)
        {
            X = x;
            Y = y;
        }


        public static HexVector2 operator +(HexVector2 a, HexVector2 b)
        {
            return new HexVector2() { X = a.X + b.X, Y = a.Y + b.Y };
        }
        public override bool Equals(object obj)
        {
            return this.X == ((HexVector2)obj).X && this.Y == ((HexVector2)obj).Y;
        }


        public HexDirectionData GetRandomAround(ref List<HexVector2> exception)
        {
            for (int i = 0; i < 10; i++)
            {
                int dir = new System.Random().Next(0, 5);
                HexVector2 pos;
                if (Y % 2 == 0)
                {
                    pos = HexVector2.DirectionVectorEven[dir];
                }
                else
                {
                    pos = HexVector2.DirectionVectorOdd[dir];
                }
                var newV2 = this + pos;
                if (!exception.Any(f => newV2.Equals(f)))
                    return new HexDirectionData() { Vector2Int = newV2, Direction = HexDirection.Unset };
            }
            return new HexDirectionData() { Vector2Int = HexVector2.Invalid, Direction = HexDirection.Unset };
        }


        public void SetUnAllocated()
        {
            X = int.MinValue;
            Y = int.MinValue;
        }



        public static bool operator ==(HexVector2 a, HexVector2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        public static bool operator !=(HexVector2 a, HexVector2 b)
        {
            return !(a.X == b.X && a.Y == b.Y);
        }

        public bool IsAllocated { get { return X != int.MinValue && Y != int.MinValue; } }



        public override string ToString()
        {
            return X + ":" + Y;
        }

        public HexVector2 GetPositionForDirection(int dir)
        {

            HexVector2 pos;
            if (Y % 2 == 0)
            {
                pos = HexVector2.DirectionVectorEven[dir];
            }
            else
            {
                pos = HexVector2.DirectionVectorOdd[dir];
            }

            return this + pos;

        }


        public HexVector2 GetClosestPositionForDestination(HexVector2 target, List<HexVector2> possibility)
        {
            var origin = this;
            int indexOfLowest = -1;
            int lowest = int.MaxValue;
            for (int i = 0; i < possibility.Count; i++)
            {
                int d = (possibility[i] + origin).HexDistance(target);
                if (lowest > d)
                {
                    lowest = d;
                    indexOfLowest = i;
                }
            }
            if (indexOfLowest > -1)
                return possibility[indexOfLowest];
            return HexVector2.Invalid;
        }

        public int HexDistance(HexVector2 b)
        {
            // Convert axial coordinates to cube coordinates
            int ax = this.X - (this.Y - (this.Y & 1)) / 2;
            int az = this.Y;
            int ay = -ax - az;

            int bx = b.X - (b.Y - (b.Y & 1)) / 2;
            int bz = b.Y;
            int by = -bx - bz;

            return Math.Max(Math.Max(Math.Abs(ax - bx), Math.Abs(ay - by)), Math.Abs(az - bz));

        }
    }
}