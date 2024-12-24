using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using UnityEngine;
namespace HexTacticsOnline.Lib
{
    [Serializable]
    public class UnitAction
    {
        [IgnoreDataMember]
        public Unit Caster { get { return Database.Instance.QueryDungeon(DungeonId).QueryUnit(CasterIndex); } }
        [IgnoreDataMember]
        public Unit Target { get { return Database.Instance.QueryDungeon(DungeonId).QueryUnit(TargetIndex); } }
        [IgnoreDataMember]
        public Tile TargetTile { get { return Database.Instance.QueryDungeon(DungeonId).Map[TargetPos]; } }

        public TargetType TargetType = TargetType.HostileOnly;

        public virtual bool IsTargetInRange(Unit target)
        {
            return Caster.Pos.HexDistance(target.Pos) <= Range;
        }

        public int Range = 1;
        public byte Cooldown = 0;
        public byte CasterIndex;
        public byte TargetIndex;
        public HexVector2 TargetPos;
        public byte CastTime;
        public byte BaseCastTime = 3;
        public byte BaseCooldown = 0;
        public int BaseThreat = 3;
        public byte DungeonId;
        public SerializableDictionary<UnitState, int> StateEffectOnCaster = new SerializableDictionary<UnitState, int>();
        public SerializableDictionary<UnitState, int> StateEffectOnTarget = new SerializableDictionary<UnitState, int>();

        public UnitAction(byte dungeonId, byte caster)
        {
            DungeonId = dungeonId;
            CasterIndex = caster;
        }




        public virtual bool InitializeTarget(byte target, HexVector2 tilePos)
        {
            if (this.Cooldown > 0)
                return false;


            CastTime = BaseCastTime;
            TargetIndex = target;
            this.TargetPos = tilePos;
            return true;

        }

        //FinalizeEffectされた数値を用いて、実際にUnit等に影響を与える
        public virtual void DoAction()
        {
            Debug.Log(this.GetType().Name + " is being triggered");
            foreach (var item in StateEffectOnTarget)
            {
                Target.ApplyEffectOnQueue(item.Key, item.Value);
            }
            foreach (var item in StateEffectOnCaster)
            {
                Target.ApplyEffectOnQueue(item.Key, item.Value);
            }
            Database.Instance.QueryDungeon(this.DungeonId).IncrementRelativeEnemiesThreat(Caster, BaseThreat);

        }
        public virtual void DoLateAction()
        {
            Debug.Log(this.GetType() + " is DoAction");
        }
        //クライアントから送られてきたコマンドがチートやバグで作られたものではないかを確認する
        public void VerifyAtServer()
        {

        }

        public void CoolAction()
        {
            Cooldown = BaseCooldown;
        }
    }
    public enum Direction { NE = 0, E = 1, SE = 2, SW = 3, W = 4, NW = 5 }
    [Serializable]
    public class UnitActionMove : UnitAction
    {

        public new int BaseCooldown = 0;

        public override bool InitializeTarget(byte target, HexVector2 tilePos)
        {

            if (!base.InitializeTarget(target, tilePos))
                return false;

            if (!TargetTile.CanPlaceUnit)
            {
                return false;
            }
            this.CastTime = Caster.MovementWait;
            return true;
        }
        public UnitActionMove(byte dungeonId, byte caster) : base(dungeonId, caster) { }

        public override void DoLateAction()
        {
            base.DoAction();
            Caster.TryMove(this.TargetTile);
        }
    }


    [Serializable]
    public class UnitActionTargetAttackMeleeSingle_Q : UnitAction
    {


        public UnitActionTargetAttackMeleeSingle_Q(byte dungeonId, byte caster) : base(dungeonId, caster)
        {
            Range = 1;
            BaseCastTime = 3;
            BaseCooldown = 1;
            StateEffectOnTarget.Add(UnitState.HP, -4);



        }
    }
    [Serializable]
    public class UnitActionTargetAttackRangeSingle_W : UnitAction
    {


        public UnitActionTargetAttackRangeSingle_W(byte dungeonId, byte caster) : base(dungeonId, caster)
        {
            Range = 3;
            BaseCastTime = 3;
            BaseCooldown = 8;

            StateEffectOnTarget.Add(UnitState.HP, -3);

        }
    }

    public enum TargetType { Any, FriendOnly, HostileOnly }

    [Serializable]
    public class UnitActionTileAOE : UnitAction
    {

        public int AOERange = 1;


        public SerializableDictionary<UnitState, int> StateEffectOnTiles = new SerializableDictionary<UnitState, int>();
        public SerializableDictionary<UnitState, int> StateEffectOnAOETarget = new SerializableDictionary<UnitState, int>();

        public UnitActionTileAOE(byte dungeonId, byte caster) : base(dungeonId, caster) { }

        public override bool IsTargetInRange(Unit target)
        {
            return Caster.Pos.HexDistance(target.Pos) <= Range + AOERange;
        }

        public override void DoAction()
        {
            base.DoAction();
            List<Tile> tiles = TargetTile.GetTilesAround(this.AOERange);
            Debug.Log("Target tiles count" + tiles.Count);
            var d = Database.Instance.QueryDungeon(this.DungeonId);
            tiles.ForEach(f =>
            {

                foreach (var item in StateEffectOnTiles)
                {
                    f.ApplyEffectOnQueue(item.Key, item.Value);
                }
                Debug.Log(f);
                Unit u = f.UnitOn;
                if (u != null)
                {
                    int counter = 0;
                    foreach (var item in StateEffectOnAOETarget)
                    {
                        if (this.TargetType == TargetType.Any)
                            u.ApplyEffectOnQueue(item.Key, item.Value);
                        else if (this.TargetType == TargetType.HostileOnly)
                        {
                            if (Caster.IsEnemy(u))
                            {
                                u.ApplyEffectOnQueue(item.Key, item.Value);
                            }
                        }
                        else if (this.TargetType == TargetType.FriendOnly)
                        {
                            if (!Caster.IsEnemy(u))
                            {
                                u.ApplyEffectOnQueue(item.Key, item.Value);
                            }
                        }

                        counter++;
                    }
                    foreach (var item in StateEffectOnCaster)
                    {
                        while (counter > 0)
                        {
                            counter--;
                            Caster.ApplyEffectOnQueue(item.Key, item.Value);
                        }
                    }

                }


            });
        }
    }

    [Serializable]
    public class UnitActionTargetAOE : UnitActionTileAOE
    {
        public UnitActionTargetAOE(byte dungeonId, byte caster) : base(dungeonId, caster)
        {

        }

        public override void DoAction()
        {
            TargetPos = Target.Pos;
            base.DoAction();

        }
    }

    public class UnitActionTileAOE_E : UnitActionTileAOE
    {
        public UnitActionTileAOE_E(byte dungeonId, byte caster) : base(dungeonId, caster)
        {
            Range = 7;
            AOERange = 2;
            BaseCastTime = 1;
            BaseCooldown = 1;
            TargetType = TargetType.Any;
            StateEffectOnTiles.Add(UnitState.Flame, 3);
            StateEffectOnAOETarget.Add(UnitState.HP, -10);
        }
    }

    public class UnitActionThreatShare_E : UnitAction
    {
        public UnitActionThreatShare_E(byte dungeonId, byte caster) : base(dungeonId, caster)
        {
            Range = 0;
            BaseCastTime = 1;
            BaseCooldown = 20;
            TargetType = TargetType.FriendOnly;
            this.StateEffectOnCaster.Add(UnitState.ThreatShare, 4);
        }
    }

    public class UnitActionTargetAOE_R : UnitActionTargetAOE
    {
        public UnitActionTargetAOE_R(byte dungeonId, byte caster) : base(dungeonId, caster)
        {
            Range = 2;
            AOERange = 1;
            BaseCastTime = 3;
            BaseCooldown = 60;
            TargetType = TargetType.HostileOnly;
            //StateEffectOnTiles.Add(UnitState.Flame, 1);
            StateEffectOnTarget.Add(UnitState.HP, -2);
            StateEffectOnAOETarget.Add(UnitState.HP, -2);

        }
    }
}
