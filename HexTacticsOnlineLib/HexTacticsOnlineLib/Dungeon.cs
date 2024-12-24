using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HexTacticsOnline.Lib
{
    public enum DungeonState { NotReady = 0, InDungeon = 1, PlayerLost = 2, EnemyLost = 3 }

    [Serializable]
    public class Dungeon
    {
        //インプット系 ユーザーから受け取る
        const byte INPUT_PLAYER_TRY_MOVE = 0;
        const byte INPUT_PLAYER_TRY_SKILLCAST = 1;
        const byte INPUT_PLAYER_CANCEL_MOVE = 2;
        const byte INPUT_PLAYER_CANCEL_SKILLCAST = 3;

        //情報更新系　プレイヤーがサーバから受け取る
        const byte INFORM_CELL = 100;
        //エネミー情報を更新　見つけた場合や視認できてる状態で、ステートが変更された場合などにも使用する
        const byte INFORM_PLAYER = 101;
        //エネミー情報を更新　見つけた場合や視認できてる状態で、ステートが変更された場合などにも使用する
        const byte INFORM_ENEMY = 102;

        public int TurnCount;
        public byte Index;
        public static byte LastIndex = 10;

        //実際のMap。敵は完全に把握してる。プレイヤーに送信する事はない。
        public SerializableDictionary<HexVector2, Tile> Map;

        //プレイヤーパーティが認知しているMap。部分的に送信していく。
        public SerializableDictionary<HexVector2, Tile> KnownMap;

        public DungeonState DungeonState = DungeonState.NotReady;
        public SerializableDictionary<byte, PlayerUnit> PlayerUnits;
        //AI行動はこちらで。プレイヤーに送信する事はない。
        public SerializableDictionary<byte, EnemyUnit> EnemyUnits;
        //プレイヤーが認知している敵。基本索敵等でこのデータを獲得する。サーバにはバックアップとして、サーバ落ちの為に保持。
        public SerializableDictionary<byte, EnemyUnit> KnownEnemyUnits = new SerializableDictionary<byte, EnemyUnit>();

        public int DungeonPower;
        public int DungeonPowerAvg;
        public byte Size;




        [System.NonSerialized]
        public List<PlayerAccount> PlayerList = new List<PlayerAccount>();
        public Inventory CreateReward(PlayerAccount p, float SizePower, float RarityPower)
        {
            Inventory list = new Inventory();

            for (int i = 0; i < this.DungeonPowerAvg; i++)
            {
                Debug.Log("creating new piece for reward " + i);
                Piece piece = new Piece(SizePower, RarityPower);
                p.Inventory.PieceDatas.Add(piece);
                list.PieceDatas.Add(piece);
            }
            return list;
        }

        public void IncrementRelativeEnemiesThreat(Unit threatOrigin, int value)
        {
            if (threatOrigin is PlayerUnit)
            {

                foreach (var e in EnemyUnits)
                {
                    e.Value.AddThreat(threatOrigin.Index, value, false);
                }

            }
            else
            {
                foreach (var e in PlayerUnits)
                {
                    e.Value.AddThreat(threatOrigin.Index, value, true);
                }
            }
        }

        static SerializableDictionary<int, HexDistanceCalculationData[]> _rangePositionEvenCenter;
        static SerializableDictionary<int, HexDistanceCalculationData[]> _rangePositionOddCenter;

        public SerializableDictionary<int, List<News>> NewsHistory = new SerializableDictionary<int, List<News>>();
        public static SerializableDictionary<int, HexDistanceCalculationData[]> RangePositionEvenCenter
        {
            get
            {
                if (_rangePositionEvenCenter == null)
                    //_rangePositionEvenCenter = new SerializableDictionary<int, MyVector2IntAndDistance[]>();
                    _rangePositionEvenCenter = JsonUtility.FromJson<SerializableDictionary<int, HexDistanceCalculationData[]>>(System.IO.File.ReadAllText("RangePositionEvenCenter.txt"));
                return _rangePositionEvenCenter;
            }
        }
        public static SerializableDictionary<int, HexDistanceCalculationData[]> RangePositionOddCenter
        {
            get
            {
                if (_rangePositionOddCenter == null)
                    //_rangePositionOddCenter = new SerializableDictionary<int, MyVector2IntAndDistance[]>();
                    _rangePositionOddCenter = JsonUtility.FromJson<SerializableDictionary<int, HexDistanceCalculationData[]>>(System.IO.File.ReadAllText("RangePositionOddCenter.txt"));
                return _rangePositionOddCenter;
            }
        }


        public void UpdateCacheForRange()
        {
            _rangePositionEvenCenter = new SerializableDictionary<int, HexDistanceCalculationData[]>();
            _rangePositionOddCenter = new SerializableDictionary<int, HexDistanceCalculationData[]>();
            //RangePositionOddCenter = new SerializableDictionary<int, MyVector2Int[]>();

            _rangePositionEvenCenter.Add(0, new HexDistanceCalculationData[1] { new HexDistanceCalculationData() { } });
            _rangePositionOddCenter.Add(0, new HexDistanceCalculationData[1] { new HexDistanceCalculationData() });
            for (int range = 1; range < 6; range++)
            {
                List<HexDistanceCalculationData> result = new List<HexDistanceCalculationData>();
                HexDistanceCalculationData current = new HexDistanceCalculationData() { Power = range };
                RecursiveBuildCacheForRange(current, result, range, true);
                _rangePositionEvenCenter[range] = result.ToArray();
            }
            for (int range = 1; range < 6; range++)
            {
                List<HexDistanceCalculationData> result = new List<HexDistanceCalculationData>();
                HexDistanceCalculationData current = new HexDistanceCalculationData() { Power = range };
                RecursiveBuildCacheForRange(current, result, range, false);
                _rangePositionOddCenter[range] = result.ToArray();
            }
            Debug.Log(JsonUtility.ToJson(_rangePositionEvenCenter));
            Debug.Log(JsonUtility.ToJson(_rangePositionOddCenter));
        }

        static void RecursiveBuildCacheForRange(HexDistanceCalculationData center, List<HexDistanceCalculationData> list, int remainLoopCounter, bool simulateEven)
        {
            HexDistanceCalculationData[] around = center.GetPositionsAround(simulateEven);

            foreach (var item in around)
            {
                if (!list.Contains(item))
                {
                    list.Add(item);
                }
                if (remainLoopCounter > 1)
                {
                    //TODO performance. include redundent search
                    RecursiveBuildCacheForRange(item, list, remainLoopCounter - 1, simulateEven);
                }


            }




        }

        public void CreateUnitFor(PlayerAccount p, byte index)
        {
            PlayerUnits[index] = new PlayerUnit() { Name = p.Name, Index = index, DungeonId = this.Index };
            PlayerUnits[index].UnitStates.Add(UnitState.HP, 50);
            PlayerUnits[index].UnitStates.Add(UnitState.MaxHP, 50);


            PlayerUnits[index].InitializeUnitAction();
            PlayerList.Add(p);
        }

        public static void Initialize(byte dungeonIndex)
        {

        }

        public void Init()
        {
            DungeonState = DungeonState.InDungeon;
            Debug.Log(Index + " dungeon is init");
            Size = (byte)Mathf.Sqrt((DungeonPower + 400));
            DungeonPowerAvg = this.DungeonPower / PlayerUnits.Count;
            // Initialize the actual Map
            Map = new SerializableDictionary<HexVector2, Tile>();// [Size, Size];
            KnownMap = new SerializableDictionary<HexVector2, Tile>();
            for (byte x = 0; x < Size; x++)
                for (byte y = 0; y < Size; y++)
                {
                    Tile mc = new Tile();
                    mc.Pos.X = x;
                    mc.Pos.Y = y;
                    mc.DungeonID = this.Index;
                    Map.Add(mc.Pos, mc);
                    mc.UnitAtID = byte.MaxValue;
                    int percentage = 0;
                    if (!(x <= 2 && y <= 2))
                    {
                        percentage = Database.Instance.Random.Next(100);
                    }
                    if (percentage > 80)
                        mc.TileType = TileType.Wall;
                    else
                        mc.TileType = TileType.Ground;
                    Tile unknown = new Tile() { DungeonID = this.Index, TileType = TileType.Unknown };

                    KnownMap.Add(mc.Pos, unknown);
                }

            //Debug.Log((KnownMap));
            for (byte x = 0; x < Size; x++)
                for (byte y = 0; y < Size; y++)
                    for (byte d = 0; d < 6; d++)
                    {
                        HexVector2 p = new HexVector2();
                        var bo = new HexVector2() { X = y, Y = y };
                        if (y % 2 == 0)
                        {
                            p.X = x + HexVector2.DirectionVectorEven[d].X;
                            p.Y = y + HexVector2.DirectionVectorEven[d].Y;
                        }
                        else
                        {
                            p.X = x + HexVector2.DirectionVectorOdd[d].X;
                            p.Y = y + HexVector2.DirectionVectorOdd[d].Y;
                        }

                        if (p.X >= 0 && p.X < Size && p.Y >= 0 && p.Y < Size)
                        {


                            Map[bo].Connect(Map[bo], d);
                        }

                    }


            Debug.Log(Index + " dungeon's tile has been created by " + Size);
            // Initialize the actual Enemies
            byte EnemyCount = (byte)(Size);
            this.EnemyUnits = new SerializableDictionary<byte, EnemyUnit>();
            for (byte i = 0; i < EnemyCount; i++)
            {
                var c = this.GetRandomEmptyCell(4);
                //ヌルポ面倒なので失敗してても初期化
                if (c != Tile.UnKnown)
                {
                    var e = new EnemyUnit();
                    e.DungeonId = this.Index;
                    e.Index = LastIndex++;
                    e.UnitStates.Add(UnitState.HP, 10);
                    e.UnitStates.Add(UnitState.MaxHP, 10);
                    //スカウト型
                    e.InstalledUnitActionIDs[2] = 4;

                    e.InitializeUnitAction();
                    EnemyUnits.Add(e.Index, e);
                    e.DungeonIndexIn = this.Index;
                    UnitDirectMoveTo(c.Pos, e);
                    e.Init(Size);

                }
            }
            for (byte i = 0; i < PlayerUnits.Count; i++)
            {
                UnitDirectMoveTo(PlayerPositions[i], PlayerUnits[i]);
            }

            Debug.Log(Index + " dungeon's enemies has been spawn by " + EnemyUnits.Count);
            Debug.Log(Index + " dungeon's players tries first Detection" + PlayerUnits.Count);

            ProcessTurn();

        }



        public void ProcessTurn()
        {
            TurnCount++;
            NewsHistory.Add(TurnCount, new List<News>());




            //敵をAIからUnitActionにキュー
            foreach (var item in EnemyUnits)
            {
                if (item.Value.CanThink)
                    item.Value.Think();
            }


            for (byte i = 0; i < this.PlayerUnits.Count; i++)
            {

                if (PlayerUnits[i].CanThink == true)
                    PlayerUnits[i].Think();
            }

            Queue<UnitAction> LateActionQueue = new Queue<UnitAction>();

            //味方のUnitActionをキャストタイムを回し、０なら取得
            for (byte i = 0; i < this.PlayerUnits.Count; i++)
            {
                if (PlayerUnits[i].CanAction)
                {
                    UnitAction casted = PlayerUnits[i].ContinueCasting();
                    if (casted != null)
                    {
                        casted.DoAction();
                        LateActionQueue.Enqueue(casted);

                    }
                }
            }
            //敵のUnitActionをキャストタイムを回し、０なら取得し、実行
            foreach (var item in EnemyUnits)
            {
                if (item.Value.CanAction)
                {
                    UnitAction casted = item.Value.ContinueCasting();
                    if (casted != null)
                    {
                        casted.DoAction();
                        LateActionQueue.Enqueue(casted);

                    }

                }
            }
            // DoLateActionを実行する 移動する効果等をここに
            while (LateActionQueue.Count > 0)
            {
                LateActionQueue.Dequeue().DoLateAction();
            }
            //行動フェーズで起こった変化を実際に反映する
            for (byte i = 0; i < this.PlayerUnits.Count; i++)
            {
                if (PlayerUnits[i].IsAlive)
                {
                    PlayerUnits[i].ApplyEffectForThisTurn();
                    PlayerUnits[i].CoolDownActions();
                }

            }
            //行動フェーズで起こった変化を実際に反映する
            foreach (var item in EnemyUnits)
            {
                if (item.Value.IsAlive)
                {
                    item.Value.ApplyEffectForThisTurn();
                    item.Value.CoolDownActions();
                }

            }
            //行動フェーズで起こったタイルへの変化を実際に反映する
            foreach (var item in Map)
            {
                item.Value.ApplyEffectForThisTurn();
            }
            //実際に発動。同フレーム発動時は、お互いにキャストされる為、順番の優位を受けないようにする必要がある
            for (byte i = 0; i < this.PlayerUnits.Count; i++)
            {

                PlayerUnits[i].TryDetect();
            }

            foreach (var item in EnemyUnits)
            {

                item.Value.TryDetect();
            }


        }

        private void UnitDirectMoveTo(HexVector2 pos, Unit unit)
        {
            if (!Map[pos].CanPlaceUnit)
                return;


            if (Map[unit.Pos].UnitAtID != byte.MaxValue)
                Map[unit.Pos].UnitAtID = byte.MaxValue;

            unit.Pos = pos;
            Map[unit.Pos].UnitAtID = unit.Index;

        }

        static HexVector2[] PlayerPositions = { new HexVector2(0, 0), new HexVector2(1, 0), new HexVector2(0, 1), new HexVector2(1, 1), new HexVector2(2, 0), new HexVector2(0, 2) };

        private Tile GetRandomEmptyCell(byte v)
        {
            for (int i = 0; i < 100; i++)
            {
                var x = Database.Instance.Random.Next(v, Size - 1);
                var y = Database.Instance.Random.Next(v, Size - 1);
                var p = new HexVector2() { X = (byte)x, Y = (byte)y };
                if (this.Map[p].CanPlaceUnit)
                {
                    return this.Map[p];
                }
            }
            return Tile.UnKnown;

        }
        public static int[] ThreatByDistance = new int[] { 20, 15, 12, 10, 8, 7, 6, 5, 4, 3, 3, 3, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

        public void TryDetectForPlayer<T>(Unit playerUnit)
        {

            List<Unit> list = SearchForUnitAround<EnemyUnit>(playerUnit);

            list.ForEach(f =>
            {

                if (!playerUnit.MyThreatAgainstEnemy.ContainsKey(f.Index))
                    Database.Instance.QueryDungeon(playerUnit.DungeonId).IncrementRelativeEnemiesThreat(f, ThreatByDistance[f.Pos.HexDistance(playerUnit.Pos)]);
                else
                    playerUnit.MyThreatAgainstEnemy[f.Index] += ThreatByDistance[f.Pos.HexDistance(playerUnit.Pos)];


                this.EnemyUnits[f.Index].IncrementKnowLevel();

                if (!this.KnownEnemyUnits.ContainsKey(f.Index))
                {
                    this.KnownEnemyUnits.Add(f.Index, EnemyUnits[f.Index].CreateUnknownCopy());

                }
                else
                {

                    this.KnownEnemyUnits[f.Index].IncrementKnowLevel();
                    this.KnownEnemyUnits[f.Index].UpdateInfo(EnemyUnits[f.Index]);

                }
                Database.Instance.QueryDungeon(playerUnit.DungeonId).QueueLatestEnemyUnitInfo(this.KnownEnemyUnits[f.Index]);

            });

            RefreshKnownMapForAround(playerUnit);
        }

        void RefreshKnownMapForAround(Unit unit)
        {
            //Sensitiviesごとにキャッシュしておくべき
            //仮実装

            HexDistanceCalculationData[] targets;

            if (unit.Pos.Y % 2 == 0)
            {
                targets = Dungeon.RangePositionEvenCenter[unit.Sensitivity];
            }
            else
            {
                targets = Dungeon.RangePositionOddCenter[unit.Sensitivity];
            }

            for (int i = 0; i < targets.Length; i++)
            {
                var p = unit.Pos + targets[i].Pos;

                //TODO performance
                if (KnownMap.ContainsKey(p))
                {

                    KnownMap[p] = Map[p];
                    KnownMap[p].Updated = true;
                }
            }


        }
        public Unit QueryUnit(byte index)
        {
            if (index == byte.MaxValue) return null;

            if (PlayerUnits.ContainsKey(index))
            {
                return PlayerUnits[index];
            }
            else if (EnemyUnits.ContainsKey(index))
            {
                return EnemyUnits[index];
            }
            return null;
        }
        public List<Unit> SearchForUnitAround<T>(Unit unit)
        {
            //Sensitiviesごとにキャッシュしておくべき
            //仮実装
            List<Unit> result = new List<Unit>();
            HexDistanceCalculationData[] targets;

            if (unit.Pos.Y % 2 == 0)
            {
                targets = Dungeon.RangePositionEvenCenter[unit.Sensitivity];
            }
            else
            {
                targets = Dungeon.RangePositionOddCenter[unit.Sensitivity];
            }

            for (int i = 0; i < targets.Length; i++)
            {
                var p = unit.Pos + targets[i].Pos;
                if (Map.ContainsKey(p) && Map[p].UnitAtID != byte.MaxValue)
                {
                    var foundUnit = QueryUnit(Map[p].UnitAtID);
                    if (foundUnit is T)
                    {
                        result.Add(foundUnit);
                    }

                }
            }
            return result;
        }

        public void TryDetectForEnemy<T>(Unit enemyUnit)
        {

            //既にすべて見つけている場合は処理しない
            //敵は Threatを共有しない

            var l = SearchForUnitAround<PlayerUnit>(enemyUnit);
            l.ForEach(f =>
            {
                if (!enemyUnit.MyThreatAgainstEnemy.ContainsKey(f.Index))
                    enemyUnit.MyThreatAgainstEnemy.Add(f.Index, ThreatByDistance[f.Pos.HexDistance(enemyUnit.Pos)]);
                else
                    enemyUnit.MyThreatAgainstEnemy[f.Index] += ThreatByDistance[f.Pos.HexDistance(enemyUnit.Pos)];




            });
        }
        public SerializableDictionary<byte, EnemyUnit> QueuedEnemyInfo = new SerializableDictionary<byte, EnemyUnit>();
        public void QueueLatestEnemyUnitInfo(EnemyUnit newInfo)
        {
            var found = QueuedEnemyInfo.ContainsKey(newInfo.Index);
            if (!found)
            {
                QueuedEnemyInfo.Add(newInfo.Index, newInfo);
            }
            else
                QueuedEnemyInfo[newInfo.Index] = newInfo;
        }

        public List<Tile> GetTilesAround(HexVector2 pos)
        {
            var tiles = new List<Tile>(6);
            for (int i = 0; i < 6; i++)
            {
                HexVector2 v = pos.GetPositionForDirection(i);
                if (this.Map.ContainsKey(v))
                {
                    tiles.Add(this.Map[v]);

                }
            }
            return tiles;
        }
        public List<Tile> GetTilesAround(HexVector2 pos, int range)
        {
            var tiles = new List<Tile>();
            for (int r = 0; r < range; r++)
            {
                for (int i = 0; i < 6; i++)
                {
                    HexVector2 v = pos.GetPositionForDirection(i);
                    if (this.Map.ContainsKey(v))
                    {
                        tiles.Add(this.Map[v]);

                    }
                }
            }
            return tiles;
        }
        public void FillTilesAroundRecursive(ref List<Tile> list, HexVector2 pos, int range)
        {
            if (range <= 0)
                return;

            for (int i = 0; i < 6; i++)
            {
                HexVector2 v = pos.GetPositionForDirection(i);
                if (this.Map.ContainsKey(v) && !list.Contains(this.Map[v]))
                {
                    list.Add(this.Map[v]);
                    range--;
                    FillTilesAroundRecursive(ref list, v, range);
                }
            }



        }

        public List<Tile> GetPlacableTilesAround(HexVector2 pos)
        {
            return GetTilesAround(pos).FindAll(f => f.CanPlaceUnit);

        }

        public void LogStateNews(byte index, UnitState state, int amount)
        {
            NewsHistory[TurnCount].Add(News.CreateStateNews(index, state, amount));
        }

        public bool MayEndGame()
        {
            bool playerLost = PlayerUnits.Values.All(f => !f.IsAlive);

            if (!playerLost)
            {
                bool playerWin = EnemyUnits.Values.All(f => !f.IsAlive);
                if (playerWin)
                {
                    DungeonResult = DungeonResult.PlayerWin;
                    return true;
                }
            }
            else
            {
                DungeonResult = DungeonResult.EnemyWin;
                return true;
            }
            return false;

        }
        public DungeonResult DungeonResult = DungeonResult.Ongoing;
    }
}