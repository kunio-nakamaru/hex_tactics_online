

using System;
using System.Collections.Generic;
using System.Linq;

namespace HexTacticsOnline.Lib
{


    public interface ILobbySender
    {
        void SendReadies(List<EnrolState> enlist, Dungeon d);
    }
    public class Database
    {
        SerializableDictionary<string, Lobby> Lobbies = new SerializableDictionary<string, Lobby>();
        SerializableDictionary<string, PlayerAccount> Players = new SerializableDictionary<string, PlayerAccount>();
        SerializableDictionary<byte, Dungeon> Dungeons = new SerializableDictionary<byte, Dungeon>();
    
        public static Database Instance;// { get { if (instance == null) instance = new Database(); return instance; } }
        public System.Random Random = new System.Random();
        //public float ServerFrameRate = 2;
        public float GameSpeed = 0.5f;
        public float AnimationSpeed = 0.5f;

        public ILobbySender ILobbySender;
        public Database()
        {
        
        }
        public Database(ILobbySender sender) {
            ILobbySender = sender;
        }


        public LobbyInfoList LobbyInfoList = new LobbyInfoList();
        public SerializableDictionary<byte, Dungeon> GetDungeons()
        {
            return Dungeons;
        }
        public PlayerAccount AddPlayer()
        {
            var p = new PlayerAccount() { Name = Guid.NewGuid().ToString().Substring(0, 4), ID = Guid.NewGuid().ToString() };
            Players.Add(p.ID, p);
            return p;
        }
        public void AddLobby(Lobby d)
        {
            Lobbies.Add(d.ID, d);
        }
        public PlayerAccount QueryPlayer(string ID)
        {
            return Players[ID];
        }
        public Lobby QueryLobby(string ID)
        {
            return Lobbies[ID];
        }

        public Dungeon QueryDungeon(byte index)
        {
            if (Dungeons.ContainsKey(index))
                return Dungeons[index];
            return null;
        }

        public Dungeon CreateNewDungeon(List<EnrolState> en)
        {
            Dungeon d = new Dungeon();
            //d.UpdateCacheForRange();
            d.DungeonPower = en.Sum(f => f.PlayerPower);

            d.PlayerUnits = new SerializableDictionary<byte, PlayerUnit>();
            byte index = 0;

            en.ForEach(x =>
            {
                d.CreateUnitFor(Database.Instance.QueryPlayer(x.PlayerID), index++);
            });
            Dungeons.Add(d.Index, d);
            return d;
        }
        static List<string> ClassNames = new List<string>() { "UnitActionTargetAttackMeleeSingle_Q", "UnitActionTargetAttackRangeSingle_W", "UnitActionTileAOE_E", "UnitActionTargetAOE_R", "UnitActionThreatShare_E" };
        public UnitAction InstantiateUnitAction(byte dungeon, byte caster, int v)
        {
            var con = Type.GetType("HexAndSlash.Data." + ClassNames[v]).GetConstructor(new Type[] { typeof(byte), typeof(byte) });

            var obj = con.Invoke(new object[] { dungeon, caster });

            return obj as UnitAction;
        }

        public LobbyInfoList GetAllLobbyInfo()
        {
            return this.LobbyInfoList;

        }
    }








    

    

}