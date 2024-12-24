using System;
using System.Collections.Generic;
using System.Text;

namespace HexTacticsOnline.Lib
{
    [Serializable]
    public class EnemyUnit : Unit
    {
        const byte MaxKnownLevel = 5;
        public byte Type;
        public bool IsBornMovable;
        public byte DungeonIndexIn;
        public byte KnownLevel;


        //敵が認知しているプレイヤーユニット情報
        //敵のプレイヤーに対する認知は個別。近くの敵に共有するスキルを大体がもってる。
        [NonSerialized]
        public List<PlayerUnit> PlayerIndexAwareOf = new List<PlayerUnit>();



        public void Init(int Power)
        {

        }
        public override void TryDetect()
        {
            base.TryDetect();
            Database.Instance.QueryDungeon(DungeonIndexIn).TryDetectForEnemy<PlayerUnit>(this);
        }

        public void IncrementKnowLevel()
        {

            KnownLevel++;


        }


        public EnemyUnit CreateUnknownCopy()
        {
            var e = new EnemyUnit() { DungeonId = this.DungeonId, Index = this.Index };


            e.InitializeUnitAction();
            e.AITargetUnit = this.AITargetUnit;
            e.UpdateInfo(this);

            return e;
        }
        //info updates based on actual
        public void UpdateInfo(EnemyUnit actualEnemyUnit)
        {
            actualEnemyUnit.UpdateCount++;
            this.Pos = actualEnemyUnit.Pos;
            this.AITargetUnit = actualEnemyUnit.AITargetUnit;
            this.MyThreatAgainstEnemy = actualEnemyUnit.MyThreatAgainstEnemy;
            if (KnownLevel > 3)
            {
                this.UnitStates = actualEnemyUnit.UnitStates;
            }

        }




    }
}
