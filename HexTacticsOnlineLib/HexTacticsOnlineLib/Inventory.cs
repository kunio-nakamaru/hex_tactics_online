using System;
using System.Collections.Generic;

namespace HexTacticsOnline.Lib
{
    [Serializable]
    public class Inventory
    {

        public List<Piece> PieceDatas = new List<Piece>();

        public void GenerateRandom()
        {
            PieceDatas.Add(new Piece(UnityEngine.Random.Range(3, 7), UnityEngine.Random.Range(1, 5)));
        }
    }
    public enum HexColor { None = -1, Purple = 0, Red = 1, Cian = 2, Green = 3, Metalic = 4, Brown = 5, Gold = 6 }
}