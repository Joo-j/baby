using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Supercent.Util;
using Supercent.Util.STM;
using Supercent.Util.Cheat;

namespace BabyNightmare
{
    public enum ECheatGroup
    {
        GameSetting,
        PlayerData,
        Currency,
        ADSetting,
    }

    public class DevManager : SingletonBase<DevManager>
    {
        public bool EnableADSetting { get; private set; }

        public void Init()
        {
            //CheatManager<ECheatGroup>.Instance.RegisterLabel(ECheatGroup.GameSetting, "IsDev", "is dev version", () => $"{BuildSetting.IS_DEV}");
            //CheatManager<ECheatGroup>.Instance.RegisterLabel(ECheatGroup.GameSetting, "IsDeployAll", "is deployed all", () => $"{BuildSetting.IS_DeployAll}");
            CheatManager<ECheatGroup>.Instance.RegisterLabel(ECheatGroup.GameSetting, "TotalPlayTime", "playtime (total)", () => $"{GetTimeText(PlayerData.TotalPlayTime)}");
            CheatManager<ECheatGroup>.Instance.RegisterLabel(ECheatGroup.GameSetting, "DayPlayTime", "playtime (day)", () => $"{GetTimeText(PlayerData.DayPlayTime)}");
            CheatManager<ECheatGroup>.Instance.RegisterLabel(ECheatGroup.GameSetting, "SessionPlayTime", "playtime (session)", () => $"{GetTimeText(PlayerData.SessionPlayTime)}");
            //CheatManager<ECheatGroup>.Instance.RegisterLabel(ECheatGroup.GameSetting, "Banner", "is banner visible", () => $"{BannerHelper.IsBannerVisible}");
            CheatManager<ECheatGroup>.Instance.RegisterSlider(ECheatGroup.GameSetting, "Time Scale", "manipulate time scale amount", SetTimeScale);

            //CheatManager<ECheatGroup>.Instance.RegisterCheckBox(ECheatGroup.GameSetting, "Skip RV", "skip reward ads", (on) => AdsHelper.RewardedVideo.IsOnSkippedCheat = on, true);
            //CheatManager<ECheatGroup>.Instance.RegisterCheckBox(ECheatGroup.GameSetting, "Skip IS", "skip intersital ads", (on) => AdsHelper.Interstitial.IsOnSkippedCheat = on, true);

            CheatManager<ECheatGroup>.Instance.RegisterLabel(ECheatGroup.PlayerData, "Chapter", "chapter number", () => $"{PlayerData.Instance.Chapter}");
            CheatManager<ECheatGroup>.Instance.RegisterLabel(ECheatGroup.PlayerData, "Attempts", "attempt count (total)", () => $"{PlayerData.Instance.TotalAttemptCount}");
            //            CheatManager<ECheatGroup>.Instance.RegisterLabel(ECheatGroup.PlayerData, "NoAds", "is purchased noads", () => $"{User.NoAds}");
            CheatManager<ECheatGroup>.Instance.RegisterInputField(ECheatGroup.PlayerData, "Set Chapter", "set chapter number", SetChapter);
            CheatManager<ECheatGroup>.Instance.RegisterInputField(ECheatGroup.PlayerData, "Set Attempt", "set attempt count", SetAttempt);
            CheatManager<ECheatGroup>.Instance.RegisterButton(ECheatGroup.PlayerData, "Reset Data", "reset all data", ResetData);

            CheatManager<ECheatGroup>.Instance.RegisterInputField(ECheatGroup.Currency, "Add Coin", "add coin amount", AddCoin);
            CheatManager<ECheatGroup>.Instance.RegisterInputField(ECheatGroup.Currency, "Set Coin", "set coin amount", SetCoin);
            CheatManager<ECheatGroup>.Instance.RegisterInputField(ECheatGroup.Currency, "Add Gem", "add gem amount", AddGem);
            CheatManager<ECheatGroup>.Instance.RegisterInputField(ECheatGroup.Currency, "Set Gem", "set gem amount", SetGem);

            CheatManager<ECheatGroup>.Instance.RegisterCheckBox(ECheatGroup.ADSetting, "Enable AD Setting", "enable AD Setting", (on) => EnableADSetting = on);

        }
        private string GetTimeText(float seconds)
        {
            int hour = (int)seconds / 3600;
            int min = (int)(seconds % 3600) / 60;
            int sec = (int)seconds % 60;

            return $"{hour:D2}:{min:D2}:{sec:D2}";
        }

        public void Show()
        {
            CheatManager<ECheatGroup>.Instance.Show();
        }

        public void Hide()
        {
            CheatManager<ECheatGroup>.Instance.Hide();
        }

        private void SetTimeScale(float amount)
        {
            Time.timeScale = Mathf.Lerp(1, 10f, amount);
        }

        private void SetChapter(string text)
        {
            if (true == text.IsNullOrEmpty())
                return;

            if (false == int.TryParse(text, out var chapter))
                return;

            if (chapter < 1)
            {
                SimpleToastMessage.Show("lesser than 1", null);
                return;
            }

            PlayerData.Instance.Chapter = chapter;
            GameFlowManager.Instance.EnterLobby();
            Hide();
        }

        private void SetAttempt(string text)
        {
            if (true == text.IsNullOrEmpty())
                return;

            if (false == int.TryParse(text, out var count))
                return;

            PlayerData.Instance.TotalAttemptCount = count;
            GameFlowManager.Instance.EnterLobby();
            Hide();
        }

        private void AddCoin(string text)
        {
            if (true == text.IsNullOrEmpty())
                return;

            if (false == int.TryParse(text, out var coin))
                return;

            PlayerData.Instance.Coin = Mathf.Clamp(PlayerData.Instance.Coin + coin, 0, int.MaxValue);
            PlayerData.Instance.Save();
        }

        private void AddGem(string text)
        {
            if (true == text.IsNullOrEmpty())
                return;

            if (false == int.TryParse(text, out var gem))
                return;

            PlayerData.Instance.Gem = Mathf.Clamp(PlayerData.Instance.Gem + gem, 0, int.MaxValue);
            PlayerData.Instance.Save();
        }

        private void SetCoin(string text)
        {
            if (true == text.IsNullOrEmpty())
                return;

            if (false == int.TryParse(text, out var coin))
                return;

            PlayerData.Instance.Coin = Mathf.Clamp(coin, 0, int.MaxValue);
            PlayerData.Instance.Save();
        }

        private void SetGem(string text)
        {
            if (true == text.IsNullOrEmpty())
                return;

            if (false == int.TryParse(text, out var gem))
                return;

            PlayerData.Instance.Gem = Mathf.Clamp(gem, 0, int.MaxValue);
            PlayerData.Instance.Save();
        }


        private void ResetData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            DeleteFiles_Recursive(Application.persistentDataPath);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif // UNITY_EDITOR

            Application.Quit();

            void DeleteFiles_Recursive(string directoryPath)
            {
                if (string.IsNullOrEmpty(directoryPath))
                    return;

                if (!Directory.Exists(directoryPath))
                    return;

                DeleteFiles_FromDirectory(directoryPath);

                var directories = Directory.GetDirectories(directoryPath);
                if (null == directories)
                    return;

                if (directories.Length <= 0)
                    return;

                foreach (var path in directories)
                    DeleteFiles_Recursive(path);
            }
            void DeleteFiles_FromDirectory(string directoryPath)
            {
                var filePaths = Directory.GetFiles(directoryPath);
                if (null == filePaths)
                    return;

                if (filePaths.Length <= 0)
                    return;

                foreach (var path in filePaths)
                {
                    try { File.Delete(path); }
                    catch (Exception e) { Debug.LogWarning($"[DevUI - DeleteFiles_FromDirectory] {e.Message}\n\n{e.StackTrace}"); }
                }
            }
        }
    }
}
