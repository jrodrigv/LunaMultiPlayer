﻿using LmpClient.Base;
using LmpClient.Localization;
using LmpClient.Network;
using LmpCommon;
using LmpCommon.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LmpClient.Windows.ServerList
{
    public partial class ServerListWindow : Window<ServerListWindow>
    {
        #region Fields

        private static readonly Dictionary<string, PropertyInfo> OrderByPropertyDictionary = new Dictionary<string, PropertyInfo>();

        protected float WindowHeight = Screen.height * 0.95f;
        protected float WindowWidth = Screen.width * 0.95f;
        protected float ServerDetailWindowHeight = 50;
        protected float ServerDetailWindowWidth = 350;

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display && MainSystem.ToolbarShowGui && MainSystem.NetworkState == ClientState.Disconnected && HighLogic.LoadedScene == GameScenes.MAINMENU;
            set
            {
                if (!_display && value)
                    NetworkServerList.RequestServers();
                base.Display = _display = value;
            }
        }

        private static readonly List<ServerInfo> DisplayedServers = new List<ServerInfo>();
        private static Vector2 _verticalScrollPosition;
        private static Vector2 _horizontalScrollPosition;
        private static Rect _serverDetailWindowRect;
        private static GUILayoutOption[] _serverDetailLayoutOptions;

        private static long _selectedServerId;
        private static string _orderBy = "PlayerCount";
        private static bool _ascending;

        private static GUIStyle _headerServerLine;
        private static GUIStyle _evenServerLine;
        private static GUIStyle _oddServerLine;
        private static GUIStyle _labelStyle;
        private static GUIStyle _kspLabelStyle;

        protected override bool Resizable => true;

        #endregion

        #region Constructor

        public ServerListWindow()
        {
            foreach (var property in typeof(ServerInfo).GetProperties())
            {
                OrderByPropertyDictionary.Add(property.Name, property);
            }
        }

        #endregion

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width * 0.025f, Screen.height * 0.025f, WindowWidth, WindowHeight);
            _serverDetailWindowRect = new Rect(Screen.width * 0.025f, Screen.height * 0.025f, WindowWidth, WindowHeight);
            MoveRect = new Rect(0, 0, int.MaxValue, TitleHeight);

            _headerServerLine = new GUIStyle
            {
                normal =
                {
                    background = new Texture2D(1, 1)
                }
            };
            _headerServerLine.normal.background.SetPixel(0, 0, new Color(0.04f, 0.04f, 0.04f, 0.9f));
            _headerServerLine.normal.background.Apply();
            _headerServerLine.onNormal.background = new Texture2D(1, 1);
            _headerServerLine.onNormal.background.SetPixel(0, 0, new Color(0.04f, 0.04f, 0.04f, 0.9f));
            _headerServerLine.onNormal.background.Apply();

            _evenServerLine = new GUIStyle
            {
                normal =
                {
                    background = new Texture2D(1, 1)
                }
            };
            _evenServerLine.normal.background.SetPixel(0, 0, new Color(0.120f, 0.120f, 0.150f, 0.9f));
            _evenServerLine.normal.background.Apply();
            _evenServerLine.onNormal.background = new Texture2D(1, 1);
            _evenServerLine.onNormal.background.SetPixel(0, 0, new Color(0.120f, 0.120f, 0.150f, 0.9f));
            _evenServerLine.onNormal.background.Apply();

            _oddServerLine = new GUIStyle
            {
                normal =
                {
                    background = new Texture2D(1, 1)
                }
            };
            _oddServerLine.normal.background.SetPixel(0, 0, new Color(0.180f, 0.180f, 0.220f, 0.9f));
            _oddServerLine.normal.background.Apply();
            _oddServerLine.onNormal.background = new Texture2D(1, 1);
            _oddServerLine.onNormal.background.SetPixel(0, 0, new Color(0.180f, 0.180f, 0.220f, 0.9f));
            _oddServerLine.onNormal.background.Apply();

            _kspLabelStyle = new GUIStyle(Skin.label) { alignment = TextAnchor.MiddleCenter };
            _labelStyle = new GUIStyle(Skin.label) { alignment = TextAnchor.MiddleCenter, normal = GUI.skin.label.normal };
            
            _serverDetailLayoutOptions = new GUILayoutOption[4];
            _serverDetailLayoutOptions[0] = GUILayout.MinWidth(ServerDetailWindowWidth);
            _serverDetailLayoutOptions[1] = GUILayout.MaxWidth(ServerDetailWindowWidth);
            _serverDetailLayoutOptions[2] = GUILayout.MinHeight(ServerDetailWindowHeight);
            _serverDetailLayoutOptions[3] = GUILayout.MaxHeight(ServerDetailWindowHeight);

            LabelOptions = new GUILayoutOption[1];
            LabelOptions[0] = GUILayout.Width(100);
        }

        protected override void DrawGui()
        {
            WindowRect = FixWindowPos(GUILayout.Window(6714 + MainSystem.WindowOffset, WindowRect, DrawContent, LocalizationContainer.ServerListWindowText.Title));
            if (_selectedServerId != 0)
            {
                _serverDetailWindowRect = FixWindowPos(GUILayout.Window(6715 + MainSystem.WindowOffset,
                    _serverDetailWindowRect, DrawServerDetailsContent, LocalizationContainer.ServerListWindowText.ServerDetailTitle, _serverDetailLayoutOptions));
            }
        }

        public override void Update()
        {
            base.Update();
            if (Display)
            {
                DisplayedServers.Clear();
                DisplayedServers.AddRange(_ascending ? NetworkServerList.Servers.Values.OrderBy(s => OrderByPropertyDictionary[_orderBy].GetValue(s, null)) :
                    NetworkServerList.Servers.Values.OrderByDescending(s => OrderByPropertyDictionary[_orderBy].GetValue(s, null)).Where(ServerFilter.MatchesFilters));
            }
        }

        private static GUIStyle GetCorrectLabelStyle(ServerInfo server)
        {
            return server.DedicatedServer ? _labelStyle : _kspLabelStyle;
        }

        private static GUIStyle GetCorrectHyperlinkLabelStyle(ServerInfo server)
        {
            return server.DedicatedServer ? _labelStyle : HyperlinkLabelStyle;
        }
    }
}
