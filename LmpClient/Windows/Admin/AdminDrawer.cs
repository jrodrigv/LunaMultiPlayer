﻿using LmpClient.Localization;
using LmpClient.Systems.Admin;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.Status;
using UnityEngine;

namespace LmpClient.Windows.Admin
{
    public partial class AdminWindow
    {
        protected override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.AdminWindowText.Password);
            AdminSystem.Singleton.AdminPassword = GUILayout.PasswordField(AdminSystem.Singleton.AdminPassword, '*', 30, GUILayout.Width(200)); // Max 32 characters
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUI.enabled = !string.IsNullOrEmpty(AdminSystem.Singleton.AdminPassword);

            ScrollPos = GUILayout.BeginScrollView(ScrollPos);
            foreach (var player in StatusSystem.Singleton.PlayerStatusList.Keys)
            {
                if (player == SettingsSystem.CurrentSettings.PlayerName) continue;
                DrawPlayerLine(player);
            }
            GUILayout.EndScrollView();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(DekesslerBigIcon))
            {
                AdminSystem.Singleton.MessageSender.SendDekesslerMsg();
            }
            if (GUILayout.Button(NukeBigIcon))
            {
                AdminSystem.Singleton.MessageSender.SendNukeMsg();
            }
            if (GUILayout.Button(RestartServerIcon))
            {
                AdminSystem.Singleton.MessageSender.SendServerRestartMsg();
                Display = false;
            }

            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private static void DrawPlayerLine(string playerName)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(playerName);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(BanIcon))
            {
                _selectedPlayer = playerName;
                _banMode = true;
            }
            if (GUILayout.Button(KickIcon))
            {
                _selectedPlayer = playerName;
                _banMode = false;
            }
            GUILayout.EndHorizontal();
        }

        #region Confirmation Dialog

        public void DrawConfirmationDialog(int windowId)
        {
            //Always draw close button first
            DrawCloseButton(() => { _selectedPlayer = null; _reason = string.Empty; }, _confirmationWindowRect);

            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.Label(_banMode ? LocalizationContainer.AdminWindowText.BanText : LocalizationContainer.AdminWindowText.KickText, LabelOptions);
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.AdminWindowText.Reason, LabelOptions);
            _reason = GUILayout.TextField(_reason, 255, GUILayout.Width(255));
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (_banMode)
            {
                if (GUILayout.Button(BanBigIcon, GUILayout.Width(255)))
                {
                    AdminSystem.Singleton.MessageSender.SendBanPlayerMsg(_selectedPlayer, _reason);
                    _selectedPlayer = null;
                    _reason = string.Empty;
                }
            }
            else
            {
                if (GUILayout.Button(KickBigIcon, GUILayout.Width(255)))
                {
                    AdminSystem.Singleton.MessageSender.SendKickPlayerMsg(_selectedPlayer, _reason);
                    _selectedPlayer = null;
                    _reason = string.Empty;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        #endregion
    }
}
