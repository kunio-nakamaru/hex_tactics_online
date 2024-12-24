using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Linq;
using UnityEngine;
namespace HexTacticsOnline.Lib
{
    public enum UnitState
    {
        MaxHP, HP, Dead, Flame, ThreatShare
    }

    [Serializable]
    public abstract class Unit
    {
        public SerializableDictionary<byte, int> MyThreatAgainstEnemy = new SerializableDictionary<byte, int>();


        public bool IsAlive
        {
            get
            {
                return !this.UnitStates.ContainsKey(UnitState.Dead);
            }
        }
        public void AddThreat(byte threater, int value, bool AddKeyIfNotExist)
        {
            if (AddKeyIfNotExist)
            {
                if (!MyThreatAgainstEnemy.ContainsKey(threater))
                {
                    MyThreatAgainstEnemy.Add(threater, 0);
                }
                MyThreatAgainstEnemy[threater] += value;
            }
            else
            {
                if (MyThreatAgainstEnemy.ContainsKey(threater))
                {
                    MyThreatAgainstEnemy[threater] += value;
                }

            }
        }
        public void ShareThreat(Unit shareTo)
        {
            var l = this.MyThreatAgainstEnemy.Keys;
            for (int i = 0; i < l.Count; i++)
            {
                var key = l.ElementAt(i);
                if (!shareTo.MyThreatAgainstEnemy.ContainsKey(key))
                    shareTo.MyThreatAgainstEnemy.Add(key, this.MyThreatAgainstEnemy[key]);
                else
                    shareTo.MyThreatAgainstEnemy[key] += this.MyThreatAgainstEnemy[key];
            }
        }

        public byte Index;
        public HexVector2 Pos;

        [System.NonSerialized]
        public SerializableDictionary<UnitState, int> ApplyingEffectsThisTurn = new SerializableDictionary<UnitState, int>();


        public bool IsAIOperated = true;

        public UnitPassive UnitPassive;

        public int[] InstalledUnitActionIDs = new int[4] { 0, 1, 2, 3 };

        [System.NonSerialized]
        public List<UnitAction> AvailableUnitAction = new List<UnitAction>(4);

        public UnitAction UnitActionOnQueue;

        public int Sensitivity = 5;
        public int UpdateCount = 0;

        public byte DungeonId;

        public byte AITargetUnit = byte.MaxValue;

        public SerializableDictionary<UnitState, int> UnitStates = new SerializableDictionary<UnitState, int>();

        public bool CanThink { get { return IsAIOperated && !UnitStates.ContainsKey(UnitState.Dead); } }

        public bool CanAction { get { return UnitActionOnQueue != null && !UnitStates.ContainsKey(UnitState.Dead); } }

        Dungeon Dungeon { get { return Database.Instance.QueryDungeon(DungeonId); } }

        public byte MovementWait = 2;

        public Tile GetRandomEmptyTileAround()
        {
            List<Tile> tiles = this.Dungeon.GetPlacableTilesAround(this.Pos);

            if (tiles.Count > 0)
                return tiles[Database.Instance.Random.Next(0, tiles.Count - 1)];
            return null;
        }

        public void CoolDownActions()
        {
            foreach (var item in AvailableUnitAction)
            {
                if (item.Cooldown > 0)
                    item.Cooldown--;
            }

        }

        public void InitializeUnitAction()
        {
            for (int i = 0; i < InstalledUnitActionIDs.Length; i++)
            {
                AvailableUnitAction.Add(Database.Instance.InstantiateUnitAction(this.DungeonId, this.Index, InstalledUnitActionIDs[i]));
            }
        }
        public UnitAction ContinueCasting()
        {
            if (UnitActionOnQueue == null)
                return null;
            UnitActionOnQueue.CastTime--;
            if (UnitActionOnQueue.CastTime <= 0)
            {
                UnitActionOnQueue.CoolAction();
                var r = UnitActionOnQueue;

                UnitActionOnQueue = null;

                return r;
            }
            return null;
        }

        public void ApplyEffectForThisTurn()
        {
            foreach (var item in ApplyingEffectsThisTurn)
            {
                if (UnitStates.ContainsKey(item.Key))
                {
                    UnitStates[item.Key] += item.Value;
                }
                else
                    UnitStates.Add(item.Key, item.Value);
            }
            if (this.UnitStates[UnitState.HP] <= 0)
            {
                UnitStates.Add(UnitState.Dead, 1);
                this.Dungeon.Map[this.Pos].UnitAtID = byte.MaxValue;

                Database.Instance.QueryDungeon(this.DungeonId).IncrementRelativeEnemiesThreat(this, -10000);

            }
            if (this.UnitStates.ContainsKey(UnitState.ThreatShare))
            {
                Debug.Log("sharing threat...");
                var l = this.Dungeon.SearchForUnitAround<EnemyUnit>(this);
                l.ForEach(f => {
                    this.ShareThreat(f);
                });
                Debug.Log("sharing threat end...");
            }
            ApplyingEffectsThisTurn.Clear();
        }

        public void ApplyEffectOnQueue(UnitState state, int value)
        {
            if (ApplyingEffectsThisTurn.ContainsKey(state))
            {
                this.ApplyingEffectsThisTurn[state] += value;
            }
            else
                this.ApplyingEffectsThisTurn.Add(state, value);
            Database.Instance.QueryDungeon(this.DungeonId).LogStateNews(this.Index, state, value);
        }

        public void ApplyEffectOnQueueOverwrite(UnitState state, int value)
        {
            if (ApplyingEffectsThisTurn.ContainsKey(state))
            {
                this.ApplyingEffectsThisTurn[state] = value;
            }
            else
                this.ApplyingEffectsThisTurn.Add(state, value);
            Database.Instance.QueryDungeon(this.DungeonId).LogStateNews(this.Index, state, this.ApplyingEffectsThisTurn[state]);
        }



        public virtual void ThinkTarget()
        {
            AITargetUnit = byte.MaxValue;
            var highestThreat = this.MyThreatAgainstEnemy.OrderByDescending(f => f.Value).FirstOrDefault();

            if (highestThreat.Value > 0)
                AITargetUnit = highestThreat.Key;
            return;
        }
        public void Think()
        {
            ThinkTarget();

            // does not know how to cancel curren command
            if (this.UnitActionOnQueue == null)
            {
                if (AITargetUnit == byte.MaxValue)
                {
                    DoIdeaMove();

                }
                else
                {
                    var target = Dungeon.QueryUnit(AITargetUnit);
                    //自分にかけられる補助を優先する
                    this.UnitActionOnQueue = ThinkBestAction(this, TargetType.FriendOnly);
                    if (this.UnitActionOnQueue != null)
                    {
                        this.UnitActionOnQueue.InitializeTarget(this.Index, this.Pos);
                    }
                    else
                    {
                        this.UnitActionOnQueue = ThinkBestAction(target, TargetType.HostileOnly);

                        if (this.UnitActionOnQueue == null)
                            this.UnitActionOnQueue = ThinkBestAction(target, TargetType.Any);

                        if (this.UnitActionOnQueue != null)
                            this.UnitActionOnQueue.InitializeTarget(AITargetUnit, target.Pos);
                        else
                            DoMoveToTarget();
                    }
                }
            }

        }

        private void DoMoveToTarget()
        {
            Debug.Log("Trying DoMoveToTarget");
            List<HexVector2> posibility = new List<HexVector2>(6);
            HexVector2 targetPosition = HexVector2.Invalid;
            HexVector2 destination = this.Dungeon.QueryUnit(AITargetUnit).Pos;

            if (this.Pos.Y % 2 == 0)
            {
                posibility.AddRange(HexVector2.DirectionVectorEven);
            }
            else
            {
                posibility.AddRange(HexVector2.DirectionVectorOdd);
            }
            //Move To the target by direction if not blocked
            for (int i = 0; i < 6; i++)
            {
                var offset = this.Pos.GetClosestPositionForDestination(destination, posibility);
                var moveTo = this.Pos + offset;
                if (offset.IsAllocated && Dungeon.Map.ContainsKey(moveTo) && Dungeon.Map[moveTo].CanPlaceUnit)
                {
                    this.UnitActionOnQueue = new UnitActionMove(this.DungeonId, this.Index);
                    this.UnitActionOnQueue.InitializeTarget(byte.MaxValue, moveTo);
                    break;

                }
                else
                    posibility.Remove(offset);
            }


        }

        private UnitAction ThinkBestAction(Unit target, TargetType targetType)
        {

            var res = AvailableUnitAction.FindAll(f => f.TargetType == targetType && f.Cooldown == 0 && f.IsTargetInRange(target)).LastOrDefault();
            Debug.Log(res);
            return res;
        }

        private void DoIdeaMove()
        {
            var t = this.GetRandomEmptyTileAround();
            if (!(t is null))
            {
                this.UnitActionOnQueue = new UnitActionMove(this.DungeonId, this.Index);
                this.UnitActionOnQueue.InitializeTarget(byte.MaxValue, t.Pos);
            }
        }





        public void QueueAction(UnitAction unitAction)
        {
            UnitActionOnQueue = unitAction;
        }
        public virtual void TryDetect()
        {

        }

        public void TryMove(Tile targetTile)
        {

            if (targetTile.CanPlaceUnit)
            {

                var current = Database.Instance.QueryDungeon(DungeonId).Map[this.Pos];
                current.UnitAtID = byte.MaxValue;
                targetTile.UnitAtID = this.Index;
                this.Pos = targetTile.Pos;

            }
        }
        public bool EqualsId(Unit b)
        {
            return this.Index == b.Index;

        }

        public bool IsEnemy(Unit u)
        {
            return this.GetType() != u.GetType();
        }
    }
}
