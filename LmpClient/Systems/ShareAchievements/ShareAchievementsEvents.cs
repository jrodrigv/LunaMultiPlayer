﻿using LmpClient.Base;

namespace LmpClient.Systems.ShareAchievements
{
    public class ShareAchievementsEvents : SubSystem<ShareAchievementsSystem>
    {
        #region EventHandlers

        public void AchievementReached(ProgressNode progressNode)
        {
            if (System.IgnoreEvents) return;

            System.MessageSender.SendAchievementsMessage(progressNode);
            LunaLog.Log($"Achievement reached: {progressNode.Id}");
        }

        public void AchievementCompleted(ProgressNode progressNode)
        {
            if (System.IgnoreEvents) return;
            
            System.MessageSender.SendAchievementsMessage(progressNode);
            LunaLog.Log($"Achievement completed: {progressNode.Id}");
        }

        public void AchievementAchieved(ProgressNode progressNode)
        {
            //This event is triggered to often (always if some speed or distance record changes).
            //LunaLog.Log("Achievement achieved:" + progressNode.Id);
        }

        public void RevertingDetected()
        {
            System.Reverting = true;
            System.StartIgnoringEvents();
        }

        public void RevertingToEditorDetected(EditorFacility data)
        {
            System.Reverting = true;
            System.StartIgnoringEvents();
        }

        public void LevelLoaded(GameScenes data)
        {
            if (System.Reverting)
            {
                System.Reverting = false;
                System.StopIgnoringEvents(true);
            }
        }

        #endregion
    }
}
