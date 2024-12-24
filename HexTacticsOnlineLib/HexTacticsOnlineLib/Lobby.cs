using System;
using System.Collections.Generic;
using UnityEngine;
namespace HexTacticsOnline.Lib
{
    public enum LobbyState { Open, Full }

    [Serializable]
    public class Lobby
    {
        const int MAX_PLAYER_LOBBY = 6;
        public LobbyState State = LobbyState.Open;
        public List<EnrolState> Enrols = new List<EnrolState>();
        public string ID;
        public LobbyInfo LobbyInfo = new LobbyInfo() { Count = 0 };

        public void UpdateLobbyInfo()
        {
            LobbyInfo.LobbyID = ID;
            LobbyInfo.Count = Enrols.Count;
            LobbyInfo.State = State;
        }
        public static EnrolState CreateNew(string PlayerID)
        {
            Lobby d = new Lobby() { ID = Guid.NewGuid().ToString() };
            Database.Instance.AddLobby(d);
            Database.Instance.LobbyInfoList.LobbyInfo.Add(d.LobbyInfo);
            return Enrol(PlayerID, d.ID);
        }

        public static LobbyInfoList GetAllLobbyInfo()
        {
            return Database.Instance.GetAllLobbyInfo();
        }

        public static EnrolState Enrol(string PlayerID, string dungeonId)
        {
            Lobby lobby = Database.Instance.QueryLobby(dungeonId);
            if (lobby.State == LobbyState.Open)
            {
                Database.Instance.QueryPlayer(PlayerID).EnrolledDungeonID = dungeonId;
                var e = new EnrolState() { PlayerID = PlayerID, IsReady = false, DungeonID = dungeonId };
                lobby.Enrols.Add(e);
                if (lobby.Enrols.Count >= MAX_PLAYER_LOBBY)
                    lobby.State = LobbyState.Full;
                lobby.UpdateLobbyInfo();
                return e;
            }
            else
                return EnrolState.Null;
        }
        public static EnrolState UnEnrol(string PlayerID, string dungeonId)
        {
            Database.Instance.QueryPlayer(PlayerID).EnrolledDungeonID = null;
            var lobby = Database.Instance.QueryLobby(dungeonId);
            int c = lobby.Enrols.RemoveAll(f => f.PlayerID == PlayerID && f.DungeonID == dungeonId);
            lobby.UpdateLobbyInfo();
            return EnrolState.Null;

        }

        public static EnrolState LobbyReady(string PlayerID, string dungeonId)
        {
            var lobby = Database.Instance.QueryLobby(dungeonId);
            var es = lobby.Enrols.Find(f => f.PlayerID == PlayerID);
            es.IsReady = true;

            return es;
        }
        public const byte NotReady = 0;
        public const byte Ready = 1;
        public static bool IsLobbyAllReady(string PlayerID, string dungeonId)
        {
            Debug.Log("IsLobbyAllReady");
            var lobby = Database.Instance.QueryLobby(dungeonId);
            var en = lobby.Enrols;
            var es = en.TrueForAll(f => f.DungeonID == dungeonId && f.IsReady);
            if (es)
            {
                lobby.State = LobbyState.Full;
                lobby.UpdateLobbyInfo();
                Debug.Log("CreateNewDungeon begins");
                Dungeon d = Database.Instance.CreateNewDungeon(en);
                Debug.Log("everyone for " + dungeonId + " is ready");
                Database.Instance.ILobbySender.SendReadies(en, d);
                return true;
                //MyWebSocketServer.Instance.SendReadies(en, d);

            }
            return false;
        }
    }

    [Serializable]
    public class LobbyInfoList
    {
        public List<LobbyInfo> LobbyInfo = new List<LobbyInfo>();
    }
    [Serializable]
    public class LobbyInfo
    {
        public int Count;
        public string LobbyID;
        public LobbyState State = LobbyState.Open;
    }



    //DO NOT SEND TO OTHER PLAYERS
    [Serializable]
    public class EnrolState
    {
        //CONFIDENTIAL
        public string PlayerID;
        public string DungeonID;
        public bool IsReady;
        public int PlayerPower = 50;
        public static EnrolState Null = new EnrolState() { };
        public byte UnitIndex = byte.MaxValue;

        public bool IsNull { get { return string.IsNullOrEmpty(this.DungeonID); } }
    }

}
