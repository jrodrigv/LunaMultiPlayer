﻿using LmpClient.Events;
using LmpClient.Extensions;
using LmpClient.VesselUtilities;
using LmpCommon.Enums;
using System;
using UnityEngine;

namespace LmpClient.Systems.VesselPartSyncFieldSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselPartSyncField
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;

        public uint PartFlightId;
        public string ModuleName;
        public string FieldName;

        public PartSyncFieldType FieldType;

        public string StrValue;
        public bool BoolValue;
        public int IntValue;
        public float FloatValue;
        public double DoubleValue;
        public Vector3 VectorValue;
        public Quaternion QuaternionValue;

        #endregion

        public void ProcessPartFieldSync()
        {
            var vessel = FlightGlobals.FindVessel(VesselId);
            if (vessel == null) return;

            if (!VesselCommon.DoVesselChecks(VesselId))
                return;

            var part = vessel.protoVessel.GetProtoPart(PartFlightId);
            if (part != null)
            {
                var module = part.FindProtoPartModuleInProtoPart(ModuleName);
                if (module != null)
                {
                    switch (FieldType)
                    {
                        case PartSyncFieldType.Boolean:
                            module.moduleValues.SetValue(FieldName, BoolValue);
                            PartModuleEvent.onPartModuleBoolFieldProcessed.Fire(module, FieldName, BoolValue);
                            break;
                        case PartSyncFieldType.Integer:
                            module.moduleValues.SetValue(FieldName, IntValue);
                            PartModuleEvent.onPartModuleIntFieldProcessed.Fire(module, FieldName, IntValue);
                            break;
                        case PartSyncFieldType.Float:
                            module.moduleValues.SetValue(FieldName, FloatValue);
                            PartModuleEvent.onPartModuleFloatFieldProcessed.Fire(module, FieldName, FloatValue);
                            break;
                        case PartSyncFieldType.Double:
                            module.moduleValues.SetValue(FieldName, DoubleValue);
                            PartModuleEvent.onPartModuleDoubleFieldProcessed.Fire(module, FieldName, DoubleValue);
                            break;
                        case PartSyncFieldType.Vector3:
                            module.moduleValues.SetValue(FieldName, VectorValue);
                            PartModuleEvent.onPartModuleVectorFieldProcessed.Fire(module, FieldName, VectorValue);
                            break;
                        case PartSyncFieldType.Quaternion:
                            module.moduleValues.SetValue(FieldName, QuaternionValue);
                            PartModuleEvent.onPartModuleQuaternionFieldProcessed.Fire(module, FieldName, QuaternionValue);
                            break;
                        case PartSyncFieldType.String:
                            module.moduleValues.SetValue(FieldName, StrValue);
                            PartModuleEvent.onPartModuleStringFieldProcessed.Fire(module, FieldName, StrValue);
                            break;
                        case PartSyncFieldType.Enum:
                            module.moduleValues.SetValue(FieldName, StrValue);
                            PartModuleEvent.onPartModuleEnumFieldProcessed.Fire(module, FieldName, IntValue, StrValue);
                            break;
                        case PartSyncFieldType.Object:
                            module.moduleValues.SetValue(FieldName, StrValue);
                            PartModuleEvent.onPartModuleObjectFieldProcessed.Fire(module, FieldName, StrValue);
                            //We do not set the value of objects in the module as we cannot be sure if they can be transformed from a string back to the object
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }
}
