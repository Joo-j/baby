using System;
using UnityEngine;
using Supercent.Util.STM;
using Supercent.Util;
using BabyNightmare.StaticData;
using BabyNightmare.Util;

namespace BabyNightmare.Lobby
{
    public static class LobbyUtil
    {
        public static Sprite GetIcon(ELobbyButtonType type) => Resources.Load<Sprite>($"Lobby/Icon/ICN_{type}");
        public static String GetDesc(ELobbyButtonType type)
        {
            switch (type)
            {

                default: return $"{type}";
            }
        }

        public static Animator GetLobbyIconAni(ELobbyButtonType type, Transform tf)
        {
            var bounceAni = ObjectUtil.LoadAndInstantiate<Animator>($"Lobby/Ani/Ani_{type}", tf);

            switch (type)
            {
                case ELobbyButtonType.Home:
                    bounceAni.transform.localScale = Vector3.one * 0.9f;
                    break;

                case ELobbyButtonType.Talent:
                    bounceAni.transform.localScale = Vector3.one * 0.65f;
                    break;

                default:
                    bounceAni.transform.localScale = Vector3.one;
                    break;
            }

            return bounceAni;
        }
    }
}
