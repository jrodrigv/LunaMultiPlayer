﻿using System.Collections.Generic;
using KSP.UI.Screens.Flight;

namespace LmpClient.Extensions
{
    public static class VesselPartCreator
    {
        public static void CreateMissingPartsInCurrentProtoVessel(Vessel vessel, ProtoVessel protoVessel)
        {
            //TODO: This is old code where we created parts dinamically but it's quite buggy. It create parts in the CURRENT vessel so it wont work for other vessels

            //We've run trough all the vessel parts and removed the ones that don't exist in the definition.
            //Now run trough the parts in the definition and add the parts that don't exist in the vessel.
            var partsToInit = new List<ProtoPartSnapshot>();
            foreach (var partSnapshot in protoVessel.protoPartSnapshots)
            {
                if (partSnapshot.FindModule("ModuleDockingNode") != null)
                {
                    //We are in a docking port part so remove it from our own vessel if we have it
                    if (FlightGlobals.FindLoadedPart(partSnapshot.persistentId, out var vesselPart))
                    {
                        vesselPart.Die();
                    }
                }

                //Skip parts that already exists
                if (FlightGlobals.FindLoadedPart(partSnapshot.persistentId, out _))
                    continue;

                var newPart = partSnapshot.Load(vessel, false);
                vessel.parts.Add(newPart);
                partsToInit.Add(partSnapshot);
            }

            //Init new parts. This must be done in another loop as otherwise new parts won't have their correct attachment parts.
            foreach (var partSnapshot in partsToInit)
                partSnapshot.Init(vessel);

            vessel.RebuildCrewList();
            MainSystem.Singleton.StartCoroutine(CallbackUtil.DelayedCallback(0.25f, () => { if (FlightGlobals.ActiveVessel) FlightGlobals.ActiveVessel.SpawnCrew(); }));
            MainSystem.Singleton.StartCoroutine(CallbackUtil.DelayedCallback(0.5f, () => { if (KerbalPortraitGallery.Instance) KerbalPortraitGallery.Instance.SetActivePortraitsForVessel(FlightGlobals.ActiveVessel); }));
        }
    }
}
