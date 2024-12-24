using System;
using System.Collections.Generic;
using System.Text;

namespace HexTacticsOnline.Lib
{
    //CAN BE SENT TO OTHER PLAYERS AS IT IS
    [Serializable]
    public class PlayerUnit : Unit
    {

        public string Name;




        public override void TryDetect()
        {
            base.TryDetect();
            Database.Instance.QueryDungeon(DungeonId).TryDetectForPlayer<EnemyUnit>(this);
        }

        public override string ToString()
        {
            return DungeonId + " " + Name + " " + " " + this.UpdateCount + " " + Pos.X + " " + Pos.Y;
        }




    }
}
