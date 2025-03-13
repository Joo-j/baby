using System;
using BabyNightmare.StaticData;

namespace BabyNightmare.Util
{
    public static class ConditionHelper
    {
        public static int GetValue(EConditionType type)
        {
            switch (type)
            {
                case EConditionType.TotalAttemptCount: return PlayerData.Instance.TotalAttemptCount;
                case EConditionType.Chapter: return PlayerData.Instance.Chapter;
                case EConditionType.ChapterAttemptCount: return PlayerData.Instance.ChapterAttemptCount;
            }

            throw new Exception($"{type}에 대한 조건 타입이 없습니다.");
        }


        public static bool IsCorrect(EComparisonType type, int a, int b)
        {
            switch (type)
            {
                case EComparisonType.Greater: return a > b;
                case EComparisonType.Less: return a < b;
                case EComparisonType.Greater_Equal: return a >= b;
                case EComparisonType.Less_Eqaul: return a <= b;
                case EComparisonType.Equal: return a == b;
            }

            throw new Exception($"{type}이 정의되어있지 않습니다.");
        }

        public static string GetOperation(EComparisonType type)
        {
            switch (type)
            {
                case EComparisonType.Greater: return ">";
                case EComparisonType.Less: return "<";
                case EComparisonType.Greater_Equal: return ">=";
                case EComparisonType.Less_Eqaul: return "<=";
                case EComparisonType.Equal: return "==";
            }

            throw new Exception($"{type}이 정의되어있지 않습니다.");
        }
    }
}