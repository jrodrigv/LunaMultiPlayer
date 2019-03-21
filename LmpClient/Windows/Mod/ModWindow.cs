﻿using LmpClient.Base;
using LmpClient.Localization;
using UnityEngine;

namespace LmpClient.Windows.Mod
{
    public partial class ModWindow : Window<ModWindow>
    {
        private const float WindowHeight = 600;
        private const float WindowWidth = 600;

        private static Vector2 _missingExpansionsScrollPos;
        private static Vector2 _mandatoryFilesNotFoundScrollPos;
        private static Vector2 _mandatoryFilesDifferentShaScrollPos;
        private static Vector2 _forbiddenFilesScrollPos;
        private static Vector2 _nonListedFilesScrollPos;
        private static Vector2 _mandatoryPartsScrollPos;
        private static Vector2 _forbiddenPartsScrollPos;

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display && HighLogic.LoadedScene == GameScenes.MAINMENU;
            set => base.Display = _display = value;
        }

        protected override void DrawGui()
        {
            WindowRect = FixWindowPos(GUILayout.Window(6706 + MainSystem.WindowOffset, WindowRect, DrawContent,
                LocalizationContainer.ModWindowText.Title, LayoutOptions));
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width/2f - WindowWidth/2f, Screen.height/2f - WindowHeight/2f, WindowWidth,
                WindowHeight);
            MoveRect = new Rect(0, 0, int.MaxValue, TitleHeight);
            
            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);
        }
    }
}
