using System;
using System.Collections.Generic;
using System.Text;

namespace HexTacticsOnline.Lib
{
    //DO NOT SEND TO OTHER PLAYERS
    [Serializable]
    public class PlayerAccount
    {
        public string ID;
        public string EnrolledDungeonID;
        public string Name;
        public Inventory Inventory = new Inventory();
    }
}
