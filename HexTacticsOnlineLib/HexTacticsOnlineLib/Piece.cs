using System;
using System.Collections.Generic;
using UnityEngine;

namespace HexTacticsOnline.Lib
{
    [Serializable]
    public class Piece
    {
        public List<HexCell> Cells;
        public List<HexDirection> GeneratedDirection = new List<HexDirection>();
        public Piece() { }
        public Piece(float size, float rarity)
        {
            float rarityPower = rarity;// Mathf.Pow(rarity, Mathf.Sqrt(size));
            Cells = new List<HexCell>();
            List<HexVector2> positionsUsed = new List<HexVector2>();
            HexVector2 currentPosition = new HexVector2(0, 0);
            positionsUsed.Add(currentPosition);
            Cells.Add(new HexCell() { Position = currentPosition, HexColor = (HexColor)UnityEngine.Random.Range(0, 3), Tier = (int)Mathf.Min(4, UnityEngine.Random.Range(0f, rarityPower)) });
            for (int i = 0; i < size; i++)
            {
                var seed = currentPosition.GetRandomAround(ref positionsUsed);
                if (seed.Direction == HexDirection.Unset)
                    return;

                currentPosition = seed.Vector2Int;
                positionsUsed.Add(currentPosition);

                GeneratedDirection.Add(seed.Direction);
                Cells.Add(new HexCell() { Position = currentPosition, HexColor = (HexColor)UnityEngine.Random.Range(0, 3), Tier = (int)Mathf.Min(4, UnityEngine.Random.Range(0f, rarityPower)) });


            }
            Debug.Log(positionsUsed.Count);
        }
    }
}