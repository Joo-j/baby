using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabyNightmare.HUD
{
    public interface IHUD
    {
        EHUDType Type { get; }
        public GameObject GO { get; }

        public void EnableShortcut(bool enable);

        public static string GetCurrencyUnit(int currencyCount, bool useNumberSymbol = true)
        {
            if (useNumberSymbol)
            {
                if (currencyCount == 0) return "0";

                if (currencyCount < 1_000) return $"{currencyCount:0}";

                if (currencyCount < 10_000) return $"{currencyCount / 1_000.0f:0.00}K";
                if (currencyCount < 100_000) return $"{currencyCount / 1_000.0f:0.0}K";
                if (currencyCount < 1_000_000) return $"{currencyCount / 1_000:0}K";

                if (currencyCount < 10_000_000) return $"{currencyCount / 1_000_000.0f:0.00}M";
                if (currencyCount < 100_000_000) return $"{currencyCount / 1_000_000.0f:0.0}M";

                return $"{currencyCount / 1_000_000:0}M";
            }
            else
            {
                if (currencyCount == 0) return "0";

                return $"{currencyCount:0}";
            }
        }

        public static string GetCurrencyUnit_SecondDigit(int currencyCount)
        {
            if (currencyCount < 1_000) return $"{currencyCount:0}";

            if (currencyCount < 10_000) return $"{currencyCount / 1_000.0f:0.0}K";
            if (currencyCount < 100_000) return $"{currencyCount / 1_000.0f:0.0}K";
            if (currencyCount < 1_000_000) return $"{currencyCount / 1_000:0}K";

            if (currencyCount < 10_000_000) return $"{currencyCount / 1_000_000.0f:0.0}M";
            if (currencyCount < 100_000_000) return $"{currencyCount / 1_000_000.0f:0.0}M";

            return $"{currencyCount / 1_000_000:0}M";
        }
    }
}