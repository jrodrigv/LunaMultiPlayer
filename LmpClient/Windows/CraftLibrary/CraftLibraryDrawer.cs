﻿using LmpClient.Systems.CraftLibrary;
using LmpClient.Systems.SettingsSys;
using System;
using System.Linq;
using UnityEngine;

namespace LmpClient.Windows.CraftLibrary
{
    public partial class CraftLibraryWindow
    {
        #region Folders

        protected override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            DrawRefreshAndUploadButton(() => System.MessageSender.SendRequestFoldersMsg(), ()=> _drawUploadScreen = true);
            GUILayout.Space(15);

            GUILayout.BeginVertical();
            _foldersScrollPos = GUILayout.BeginScrollView(_foldersScrollPos);
            if (!System.CraftInfo.Keys.Any()) _selectedFolder = null;
            else
            {
                foreach (var folderName in System.CraftInfo.Keys)
                {
                    DrawFolderButton(folderName);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }

        private void DrawFolderButton(string folderName)
        {
            if (GUILayout.Toggle(_selectedFolder == folderName, folderName, GetFolderStyle(folderName)))
            {
                if (_selectedFolder != folderName)
                {
                    _selectedFolder = folderName;
                    System.RequestCraftListIfNeeded(_selectedFolder);
                }
            }
            else
            {
                if (_selectedFolder == folderName) _selectedFolder = null;
            }
        }

        private GUIStyle GetFolderStyle(string folderName)
        {
            return System.FoldersWithNewContent.Contains(folderName) ? RedFontButtonStyle : Skin.button;
        }

        #endregion

        #region Craft list

        public void DrawLibraryContent(int windowId)
        {
            //Always draw close button first
            DrawCloseButton(() => _selectedFolder = null, _libraryWindowRect);

            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            DrawRefreshButton(() => System.MessageSender.SendRequestCraftListMsg(_selectedFolder));
            GUILayout.Space(15);

            if (string.IsNullOrEmpty(_selectedFolder)) return;

            GUILayout.BeginVertical();
            _libraryScrollPos = GUILayout.BeginScrollView(_libraryScrollPos);
            if (SphCrafts.Any())
            {
                GUILayout.Label("SPH", BoldRedLabelStyle);
                for (var i = 0; i < SphCrafts.Count; i++)
                {
                    DrawCraftEntry(SphCrafts[i]);
                }
                GUILayout.Space(5);
            }
            if (VabCrafts.Any())
            {
                GUILayout.Label("VAB", BoldRedLabelStyle);
                for (var i = 0; i < VabCrafts.Count; i++)
                {
                    DrawCraftEntry(VabCrafts[i]);
                }
                GUILayout.Space(5);
            }
            if (SubAssemblyCrafts.Any())
            {
                GUILayout.Label("Subassembly", BoldRedLabelStyle);
                for (var i = 0; i < SubAssemblyCrafts.Count; i++)
                {
                    DrawCraftEntry(SubAssemblyCrafts[i]);
                }
                GUILayout.Space(5);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }

        private void DrawCraftEntry(CraftBasicEntry craftBasicEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(craftBasicEntry.CraftName);

            if (craftBasicEntry.FolderName == SettingsSystem.CurrentSettings.PlayerName)
            {
                if (GUILayout.Button(DeleteIcon, GUILayout.Width(35)))
                {
                    System.MessageSender.SendDeleteCraftMsg(craftBasicEntry);
                    _selectedFolder = null;
                }
            }
            else
            {
                if (GUILayout.Button(SaveIcon, GUILayout.Width(35)))
                {
                    if (System.CraftDownloaded.TryGetValue(_selectedFolder, out var downloadedCraft) && !downloadedCraft.ContainsKey(craftBasicEntry.CraftName))
                        System.RequestCraft(craftBasicEntry);
                    _selectedFolder = null;
                }
            }

            GUILayout.EndHorizontal();
        }

        #endregion

        #region Upload screen

        public void DrawUploadScreenContent(int windowId)
        {
            //Always draw close button first
            DrawCloseButton(() => _drawUploadScreen = false, _uploadWindowRect);

            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            DrawRefreshButton(() => System.RefreshOwnCrafts());
            GUILayout.Space(15);

            GUILayout.BeginVertical();
            _uploadScrollPos = GUILayout.BeginScrollView(_uploadScrollPos);
            for (var i = 0; i < System.OwnCrafts.Count; i++)
            {
                DrawUploadCraftEntry(System.OwnCrafts[i]);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }

        private void DrawUploadCraftEntry(CraftEntry craftEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{craftEntry.CraftName} ({craftEntry.CraftType})");
            if (GUILayout.Button(UploadIcon, GUILayout.Width(35)))
            {
                System.SendCraft(craftEntry);
                _drawUploadScreen = false;
            }
            GUILayout.EndHorizontal();
        }

        #endregion

        private static void DrawRefreshAndUploadButton(Action refreshAction, Action uploadAction)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(RefreshIcon)) refreshAction.Invoke();
            if (GUILayout.Button(UploadIcon)) uploadAction.Invoke();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
