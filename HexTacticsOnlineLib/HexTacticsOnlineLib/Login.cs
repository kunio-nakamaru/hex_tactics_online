using System;
using System.Collections.Generic;
using System.Text;

namespace HexTacticsOnline.Lib
{
    public class Login
    {
        public static PlayerAccount CreateNew()
        {
            var p = Database.Instance.AddPlayer();
            return p;
        }
        public static PlayerAccount WithID(string PlayerID)
        {
            var p = Database.Instance.QueryPlayer(PlayerID);
            return p;
        }
    }
}
