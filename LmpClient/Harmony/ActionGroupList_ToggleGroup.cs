﻿using Harmony;
using LmpClient.Events;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to fir an event when you press an action group
    /// </summary>
    [HarmonyPatch(typeof(ActionGroupList))]
    [HarmonyPatch("ToggleGroup")]
    public class ActionGroupList_ToggleGroup
    {
        [HarmonyPostfix]
        private static void PostFixToggleGroup(ActionGroupList __instance, KSPActionGroup group)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            var groupIndex = BaseAction.GetGroupIndex(group);
            if (Planetarium.GetUniversalTime() < __instance.cooldownTimes[groupIndex])
            {
                //This action group has a cooldown time and it wasn't triggered so ignore the event.
                return;
            }

            ActionGroupEvent.onActionGroupFired.Fire(__instance.v, group, __instance.groups[groupIndex]);
        }
    }
}
