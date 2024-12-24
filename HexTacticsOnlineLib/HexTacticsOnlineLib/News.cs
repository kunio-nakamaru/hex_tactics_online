using System;
using System.Collections.Generic;

namespace HexTacticsOnline.Lib
{
    [Serializable]
    public class News
    {
        //Newtonなら抽象化できる
        //public UnitActionTargetAttack UnitActionTargetAttack;
        public byte SubjectIndex;
        public SerializableDictionary<UnitState, int> StateNews;
        public CastingInfo CastingInfo;


        public static News CreateStateNews(byte subject, UnitState unitState, int value)
        {
            News news = new News();
            news.StateNews = new SerializableDictionary<UnitState, int>();
            news.StateNews.Add(unitState, value);

            news.SubjectIndex = subject;
            return news;
        }

    }
}