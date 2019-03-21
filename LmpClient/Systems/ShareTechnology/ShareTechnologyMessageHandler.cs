﻿using System.Collections.Concurrent;
using System.Linq;
using KSP.UI.Screens;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;

namespace LmpClient.Systems.ShareTechnology
{
    public class ShareTechnologyMessageHandler : SubSystem<ShareTechnologySystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.TechnologyUpdate) return;

            if (msgData is ShareProgressTechnologyMsgData data)
            {
                var tech = new TechNodeInfo(data.TechNode); //create a copy of the tech value so it will not change in the future.
                LunaLog.Log($"Queue TechnologyResearch with: {tech.Id}");
                System.QueueAction(() =>
                {
                    TechnologyResearch(tech);
                });
            }
        }

        private static void TechnologyResearch(TechNodeInfo tech)
        {
            System.StartIgnoringEvents();
            var node = AssetBase.RnDTechTree.GetTreeTechs().ToList().Find(n => n.techID == tech.Id);

            //Unlock the technology
            ResearchAndDevelopment.Instance.UnlockProtoTechNode(node);

            //Refresh RD nodes in case we are in the RD screen
            if (RDController.Instance && RDController.Instance.partList)
            {
                RDController.Instance.partList.Refresh();
                RDController.Instance.UpdatePanel();
            }

            //Refresh the tech tree
            ResearchAndDevelopment.RefreshTechTreeUI();

            //Refresh the part list in case we are in the VAB/SPH
            if (EditorPartList.Instance) EditorPartList.Instance.Refresh();

            System.StopIgnoringEvents();
            LunaLog.Log($"TechnologyResearch received - technology researched: {tech.Id}");
        }
    }
}
