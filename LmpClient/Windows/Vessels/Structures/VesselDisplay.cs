﻿using LmpClient.VesselUtilities;
using System;
using UnityEngine;

namespace LmpClient.Windows.Vessels.Structures
{
    internal class VesselDisplay : VesselBaseDisplay
    {
        public override bool Display { get; set; }
        public Guid VesselId { get; set; }
        public string VesselName { get; set; }
        public VesselDataDisplay Data { get; set; }
        public VesselLockDisplay Locks { get; set; }
        public VesselOrbitDisplay Orbit { get; set; }
        public VesselInterpolationDisplay Interpolation { get; set; }
        public VesselPositionDisplay Position { get; set; }
        public VesselVectorsDisplay Vectors { get; set; }

        public VesselDisplay(Guid vesselId)
        {
            VesselId = vesselId;
            Data = new VesselDataDisplay(VesselId);
            Locks = new VesselLockDisplay(VesselId);
            Orbit = new VesselOrbitDisplay(VesselId);
            Interpolation = new VesselInterpolationDisplay(VesselId);
            Position = new VesselPositionDisplay(VesselId);
            Vectors = new VesselVectorsDisplay(VesselId);
        }

        protected override void UpdateDisplay(Vessel vessel)
        {
            VesselId = vessel.id;
            VesselName = vessel.vesselName;
            Data.Update(vessel);
            Locks.Update(vessel);
            Orbit.Update(vessel);
            Interpolation.Update(vessel);
            Position.Update(vessel);
            Vectors.Update(vessel);
        }

        protected override void PrintDisplay()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label(VesselName);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reload"))
            {
                var vessel = FlightGlobals.FindVessel(VesselId);
                
                //Do not call BackupVessel() as that would overwrite the proto with the actual vessel values and mess up part syncs
                VesselLoader.LoadVessel(vessel.protoVessel, true);
            }
            GUILayout.EndHorizontal();

            Data.Display = GUILayout.Toggle(Data.Display, nameof(Data), ButtonStyle);
            Data.Print();
            Locks.Display = GUILayout.Toggle(Locks.Display, nameof(Locks), ButtonStyle);
            Locks.Print();
            Orbit.Display = GUILayout.Toggle(Orbit.Display, nameof(Orbit), ButtonStyle);
            Orbit.Print();
            Interpolation.Display = GUILayout.Toggle(Interpolation.Display, nameof(Interpolation), ButtonStyle);
            Interpolation.Print();
            Position.Display = GUILayout.Toggle(Position.Display, nameof(Position), ButtonStyle);
            Position.Print();
            Vectors.Display = GUILayout.Toggle(Vectors.Display, nameof(Vectors), ButtonStyle);
            Vectors.Print();
            GUILayout.EndVertical();
        }
    }
}
