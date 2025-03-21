using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Supercent.Util;
using BabyNightmare.Util;
using UnityEngine.Events;
using BabyNightmare.StaticData;

namespace BabyNightmare.GlobalEvent
{
    public struct GlobalEventInst
    {
        private UnityEvent<int> _event;

        public bool IsTotal { get; }

        public GlobalEventInst(bool isAccum, UnityAction<int> listner)
        {
            _event = new UnityEvent<int>();
            _event.AddListener(listner);

            this.IsTotal = isAccum;
        }

        public void Invoke(int value)
        {
            _event.Invoke(value);
        }
    }

    public static class GlobalEventManager
    {
        private const string PATH_GLOBAL_EVENT_SAVE_DATA = "gl_ev_sa_da";
        private const float AUTO_SAVE_INTERVAL = 30f;
        private static Dictionary<EGlobalEventType, int> _totalValueDict = new Dictionary<EGlobalEventType, int>();
        private static Dictionary<EGlobalEventType, Dictionary<string, GlobalEventInst>> _instTypeDict = new Dictionary<EGlobalEventType, Dictionary<string, GlobalEventInst>>();
        private static LogClassPrinter _printer = new LogClassPrinter("GlobalEventManager", "#120231");
        private static bool _dirty = false;

        private static bool USE_ENCODE
        {
            get
            {
#if UNITY_EDITOR
                return false;
#else
                return true;
#endif
            }
        }

        public static bool EnableSave { private get; set; } = true;

        static GlobalEventManager()
        {
            Load();

            // App.Coroutine(Co_Save());
        }

        private static IEnumerator Co_Save()
        {
            while (true)
            {
                yield return CoroutineUtil.WaitForSeconds(AUTO_SAVE_INTERVAL);
                Save();
            }
        }


        //하나의 이벤트 타입에 여러개의 리스너가 들어올 수 있음
        //그 리스너들은 각자의 키로 관리할 수 있도록 한다
        //이벤트 인스턴스가 누적 값을 사용하는지 아닌지에 대해 분류한다
        public static void AddListner(EGlobalEventType type, string key, bool isTotal, UnityAction<int> listner)
        {
            if (false == _instTypeDict.ContainsKey(type))
                _instTypeDict.Add(type, new Dictionary<string, GlobalEventInst>());

            var instKeyDict = _instTypeDict[type];

            if (true == instKeyDict.ContainsKey(key))
            {
                _printer.Log("AddListner", $"{type}타입의 {key} 키값을 가진 이벤트 인스턴스가 이미 있습니다.");
                return;
            }

            instKeyDict.Add(key, new GlobalEventInst(isTotal, listner));

            _printer.Log("AddListner", $"{type}");
        }

        public static void RemoveListner(EGlobalEventType type, string key)
        {
            if (false == _instTypeDict.TryGetValue(type, out var instKeyDict))
                return;

            instKeyDict.Remove(key);

            _printer.Log("RemoveListner", $"{type}");
        }

        private static void InvokeListner(EGlobalEventType type, int value)
        {
            if (false == _instTypeDict.TryGetValue(type, out var instKeyDict))
                return;

            var totalValue = _totalValueDict[type];

            foreach (var pair in instKeyDict)
            {
                var inst = pair.Value;
                if (true == inst.IsTotal)
                    inst.Invoke(totalValue);
                else
                    inst.Invoke(value);
            }
        }

        public static void AddValue(EGlobalEventType type, int value)
        {
            _totalValueDict[type] += value; //이벤트가 등록되지 않아도 누적 값은 갱신

            InvokeListner(type, value);

            _dirty = true;
        }

        public static int GetValue(EGlobalEventType type)
        {
            return _totalValueDict[type];
        }

        public static void Save(bool printLog = false)
        {
            if (false == EnableSave)
                return;

            if (false == _dirty)
                return;

            var binaryData = JsonConvert.SerializeObject(_totalValueDict);
            FileSaveUtil.Save(PATH_GLOBAL_EVENT_SAVE_DATA, binaryData, USE_ENCODE, USE_ENCODE);

            _dirty = false;

            if (true == printLog)
                _printer.Log("Save", "저장 완료");
        }

        private static void Load()
        {
            var binaryData = FileSaveUtil.Load(PATH_GLOBAL_EVENT_SAVE_DATA, USE_ENCODE, USE_ENCODE);
            var types = Enum.GetValues(typeof(EGlobalEventType));
            if (true == binaryData.IsNullOrEmpty())
            {
                InitValueDict(types);
                _printer.Log("Load", "저장된 누적 이벤트 발생 값이 없어 타입별 초기화합니다.");
                return;
            }

            var tempDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(binaryData);
            foreach (var pair in tempDict)
            {
                var type_string = pair.Key;
                if (false == Enum.TryParse<EGlobalEventType>(type_string, out var type))
                    continue;

                var value = pair.Value;

                _totalValueDict.Add(type, value);
            }

            InitValueDict(types); //저장된 데이터를 파싱한 이후에 초기화 하여 추가된 타입 대응
            _printer.Log("Load", "불러오기 완료");

            void InitValueDict(Array types)
            {
                foreach (EGlobalEventType type in types)
                {
                    if (type == EGlobalEventType.Unknown)
                        continue;

                    if (true == _totalValueDict.ContainsKey(type))
                        continue;

                    _totalValueDict.Add(type, 0);
                }
            }
        }
    }
}