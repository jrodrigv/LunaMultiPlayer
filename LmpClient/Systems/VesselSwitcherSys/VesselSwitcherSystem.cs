﻿using System.Collections;
using LmpClient.Base;
using UnityEngine;

namespace LmpClient.Systems.VesselSwitcherSys
{
    /// <summary>
    /// This system handles the vessel loading into the game and sending our vessel structure to other players.
    /// We only load vesels that are in our subspace
    /// </summary>
    public class VesselSwitcherSystem : System<VesselSwitcherSystem>
    {
        #region Fields & properties

        private static Vessel VesselToSwitchTo { get; set; }

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselSwitcherSystem);

        protected override void OnDisabled()
        {
            base.OnDisabled();
            VesselToSwitchTo = null;
        }

        #endregion

        #region Public

        /// <summary>
        /// Specifies the vessel that we should switch to
        /// </summary>
        public void SwitchToVessel(Vessel vessel)
        {
            if (vessel != null)
            {
                VesselToSwitchTo = vessel;
                LunaLog.Log($"[LMP]: Switching to vessel {vessel.vesselName}");
                MainSystem.Singleton.StartCoroutine(SwitchToVessel());
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Switch to the vessel
        /// </summary>
        private static IEnumerator SwitchToVessel()
        {
            if (VesselToSwitchTo != null)
            {
                var tries = 0;
                var zoom = FlightCamera.fetch.Distance;

                OrbitPhysicsManager.HoldVesselUnpack();
                while (!VesselToSwitchTo.loaded && tries < 100)
                {
                    tries++;
                    yield return new WaitForFixedUpdate();
                }

                LunaLog.Log($"Tries: {tries} Loaded: {VesselToSwitchTo.loaded}");

                if (!VesselToSwitchTo.loaded)
                {
                    tries = 0;
                    //Vessels still didn't loaded after 100 fixued update frames, let's wait for 1 more second...
                    while (!VesselToSwitchTo.loaded && tries < 10)
                    {
                        tries++;
                        yield return new WaitForSeconds(0.1f);
                    }
                }

                if(!VesselToSwitchTo.loaded)
                    VesselToSwitchTo.Load();

                FlightGlobals.ForceSetActiveVessel(VesselToSwitchTo);
                FlightCamera.fetch.SetDistance(zoom);
                VesselToSwitchTo = null;
            }
        }

        #endregion
    }
}
