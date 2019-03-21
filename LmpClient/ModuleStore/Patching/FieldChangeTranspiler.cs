﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using LmpClient.Events;
using LmpClient.ModuleStore.Structures;
using UnityEngine;

namespace LmpClient.ModuleStore.Patching
{
    public class FieldChangeTranspiler
    {
        private static ModuleDefinition _definition;
        private static ILGenerator _generator;
        private static MethodBase _originalMethod;
        private static List<CodeInstruction> _codes;

        /// <summary>
        /// We create local vars to store the values of the tracked fields. Those local vars will have an index to identify them.
        /// Here we store the KEY-FIED so we can access them easely
        /// </summary>
        private static readonly Dictionary<int, int> FieldIndexToLocalVarDictionary = new Dictionary<int, int>();

        private static int LastIndex => _codes.Count - 1;

        /// <summary>
        /// Creates the static references and init the transpiler
        /// </summary>
        public static void InitTranspiler(ModuleDefinition definition, ILGenerator generator,
            MethodBase originalMethod, List<CodeInstruction> codes)
        {
            _definition = definition;
            _generator = generator;
            _originalMethod = originalMethod;
            _codes = codes;

            FieldIndexToLocalVarDictionary.Clear();
        }

        /// <summary>
        /// This is black magic. By using opcodes we store all the fields that we are tracking into local variables at the beginning of the method.
        /// Then, after the method has run, we compare the current values against the stored ones and we trigger the onPartModuleFieldChanged event
        /// </summary>
        public static IEnumerable<CodeInstruction> Transpile()
        {
            TranspileBackupFields();
            TranspileEvaluations();

            return _codes.AsEnumerable();
        }

        /// <summary>
        /// Here we make a backup of all the tracked fields at the BEGGINING of the function.
        /// Ex: 
        /// public void MyFunction()
        /// {
        ///     String backupField1 = TrackedField1;
        ///     Int backupField2 = TrackedField2;
        ///     Bool backupField3 = TrackedField3;
        /// 
        ///     ----- Function Code------
        /// } 
        /// </summary>
        private static void TranspileBackupFields()
        {
            var fields = _definition.Fields.ToList();
            for (var i = 0; i < fields.Count; i++)
            {
                var field = AccessTools.Field(_originalMethod.DeclaringType, fields[i].FieldName);
                if (field == null)
                {
                    LunaLog.LogError($"Field {fields[i].FieldName} not found in module {_originalMethod.DeclaringType}");
                    continue;
                }

                //Here we declare a local var to store the OLD value of the field that we are tracking
                var localVar = _generator.DeclareLocal(field.FieldType);
                FieldIndexToLocalVarDictionary.Add(i, localVar.LocalIndex);

                _codes.Insert(0, new CodeInstruction(OpCodes.Ldarg_0));
                _codes.Insert(1, new CodeInstruction(OpCodes.Ldfld, field));

                switch (FieldIndexToLocalVarDictionary[i])
                {
                    case 0:
                        _codes.Insert(2, new CodeInstruction(OpCodes.Stloc_0));
                        break;
                    case 1:
                        _codes.Insert(2, new CodeInstruction(OpCodes.Stloc_1));
                        break;
                    case 2:
                        _codes.Insert(2, new CodeInstruction(OpCodes.Stloc_2));
                        break;
                    case 3:
                        _codes.Insert(2, new CodeInstruction(OpCodes.Stloc_3));
                        break;
                    default:
                        _codes.Insert(2, new CodeInstruction(OpCodes.Stloc_S, FieldIndexToLocalVarDictionary[i]));
                        break;
                }
            }
        }

        /// Here we make a comparison and we trigger a field when the new vals are different.
        /// Example: 
        /// public void MyFunction()
        /// {
        ///     ----- Fields declared before ----
        /// 
        ///     ----- Function Code------
        /// 
        ///     if (TrackedField1 != backupField1)
        ///         PartModuleEvent.onPartModuleStringFieldChanged.Fire();
        ///     if (TrackedField2 != backupField2)
        ///         PartModuleEvent.onPartModuleIntFieldChanged.Fire();
        ///     if (TrackedField3 != backupField3)
        ///         PartModuleEvent.onPartModuleBoolFieldChanged.Fire();
        /// } 
        private static void TranspileEvaluations()
        {
            var startComparisonInstructions = new List<CodeInstruction>();
            var jmpInstructions = new List<CodeInstruction>();

            var fields = _definition.Fields.ToList();
            for (var i = 0; i < fields.Count; i++)
            {
                var field = AccessTools.Field(_originalMethod.DeclaringType, fields[i].FieldName);
                if (field == null) continue;

                var evaluationVar = _generator.DeclareLocal(typeof(bool));

                //We write the comparision and the triggering of the event at the bottom
                switch (FieldIndexToLocalVarDictionary[i])
                {
                    case 0:
                        _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldloc_0));
                        break;
                    case 1:
                        _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldloc_1));
                        break;
                    case 2:
                        _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldloc_2));
                        break;
                    case 3:
                        _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldloc_3));
                        break;
                    default:
                        _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldloc_S, FieldIndexToLocalVarDictionary[i]));
                        break;
                }

                //If we are inserting the first field, redirect all the "returns" towards this instruction so we always do the comparison checks
                if (i == 0)
                {
                    RedirectExistingReturns(_codes);
                }
                else
                {
                    startComparisonInstructions.Add(_codes[_codes.Count - 2]);
                }

                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldarg_0));
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldfld, field));
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ceq));
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldc_I4_0));
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ceq));

                switch (evaluationVar.LocalIndex)
                {
                    case 0:
                        _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Stloc_0));
                        _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldloc_0));
                        break;
                    case 1:
                        _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Stloc_1));
                        _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldloc_1));
                        break;
                    case 2:
                        _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Stloc_2));
                        _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldloc_2));
                        break;
                    case 3:
                        _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Stloc_3));
                        _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldloc_3));
                        break;
                    default:
                        _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Stloc_S, evaluationVar.LocalIndex));
                        _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldloc_S, evaluationVar.LocalIndex));
                        break;
                }
                
                //If we are in the last field then return to the last "ret" of the function
                if (i == fields.Count - 1)
                {
                    if (!_codes[LastIndex].labels.Any())
                    {
                        _codes[LastIndex].labels.Add(_generator.DefineLabel());
                    }
                    _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Brfalse_S, _codes[LastIndex].labels[0]));
                }
                else
                {
                    var jmpInstruction = new CodeInstruction(OpCodes.Brfalse_S);
                    _codes.Insert(LastIndex, jmpInstruction);
                    jmpInstructions.Add(jmpInstruction);
                }

                LoadFunctionByFieldType(field.FieldType);

                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldarg_0));
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldstr, field.Name));
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldarg_0));
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldfld, field));

                if (field.FieldType.IsEnum)
                {
                    //For enums we must do the call of the "ToString()"
                    _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldarg_0));
                    _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldflda, field));
                    _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Constrained, field.FieldType));
                    _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(object), "ToString")));
                }

                CallFunctionByFieldType(field.FieldType);
            }

            FixFallbackInstructions(startComparisonInstructions, jmpInstructions);
        }

        /// <summary>
        /// Now we must fix all the instructions when the if's fail
        /// Example:
        /// public void MyFunction()
        /// {
        ///     ----- Fields declared before ----
        /// 
        ///     ----- Function Code------
        /// 
        ///     if (TrackedField1 != backupField1) -----> If it fails it must get a GOTO "if (TrackedField2 != backupField2)"
        ///         PartModuleEvent.onPartModuleStringFieldChanged.Fire();
        /// 
        ///     if (TrackedField2 != backupField2) -----> If it fails it must get a GOTO "if (TrackedField3 != backupField3)"
        ///         PartModuleEvent.onPartModuleIntFieldChanged.Fire();
        /// 
        ///     if (TrackedField3 != backupField3) -----> This one already jumps to the last ret of the function
        ///         PartModuleEvent.onPartModuleBoolFieldChanged.Fire();
        /// } 
        /// </summary>
        private static void FixFallbackInstructions(List<CodeInstruction> startComparisonInstructions, List<CodeInstruction> jmpInstructions)
        {
            for (var i = 0; i < startComparisonInstructions.Count; i++)
            {
                var lbl = _generator.DefineLabel();
                startComparisonInstructions[i].labels.Add(lbl);
                jmpInstructions[i].operand = lbl;
            }
        }

        /// <summary>
        /// Here we redirect all the "Returns" that the function might have so they point to the first comparison
        /// Example: 
        /// public void MyFunction()
        /// {
        ///     String backupField1 = TrackedField1;
        ///     Int backupField2 = TrackedField2;
        ///     Bool backupField3 = TrackedField3;
        /// 
        ///     ----- Function Code------
        ///     return; ---------------------> This becomes a kind of "GOTO: if (TrackedField1 != backupField1)"
        ///     ----- Function Code------
        /// 
        ///     if (TrackedField1 != backupField1)
        ///         PartModuleEvent.onPartModuleStringFieldChanged.Fire();
        ///     if (TrackedField2 != backupField2)
        ///         PartModuleEvent.onPartModuleIntFieldChanged.Fire();
        ///     if (TrackedField3 != backupField3)
        ///         PartModuleEvent.onPartModuleBoolFieldChanged.Fire();
        /// } 
        /// </summary>
        private static void RedirectExistingReturns(List<CodeInstruction> codes)
        {
            //Store the instruction we just added as we must redirect all the returns that the method already had towards this line
            var firstCheck = _generator.DefineLabel();
            codes[codes.Count - 2].labels.Add(firstCheck);

            //This is the last "ret" that every function has
            var lastReturnInstructionLabels = codes.Last().labels;

            //Skip the last "ret" of the function as otherwise we will get an endless loop
            for (var i = 0; i < codes.Count - 1; i++)
            {
                //IF the code is a "return" then change it for a break and go to the first check
                if (codes[i].opcode == OpCodes.Ret)
                {
                    codes[i].opcode = OpCodes.Br;
                    codes[i].operand = firstCheck;
                }

                //Whatever the opcode is, if it ends up in last instruction, redirect it to the first check
                if (codes[i].operand is Label lbl && lastReturnInstructionLabels.Contains(lbl))
                {
                    codes[i].operand = firstCheck;
                }
            }
        }

        /// <summary>
        /// Calls the correct onPartModuleXXXFieldChanged based on type
        /// </summary>
        private static void LoadFunctionByFieldType(Type fieldType)
        {
            if (fieldType == typeof(bool))
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleBoolFieldChanged")));
            }
            else if (fieldType == typeof(int))
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleIntFieldChanged")));
            }
            else if (fieldType == typeof(float))
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleFloatFieldChanged")));
            }
            else if (fieldType == typeof(double))
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleDoubleFieldChanged")));
            }
            else if (fieldType == typeof(string))
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleStringFieldChanged")));
            }
            else if (fieldType == typeof(Quaternion))
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleQuaternionFieldChanged")));
            }
            else if (fieldType == typeof(Vector3))
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleVectorFieldChanged")));
            }
            else if (fieldType.IsEnum)
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleEnumFieldChanged")));
            }
            else
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PartModuleEvent), "onPartModuleObjectFieldChanged")));
            }
        }

        /// <summary>
        /// Calls the correct Fire() based on type
        /// </summary>
        private static void CallFunctionByFieldType(Type fieldType)
        {
            if (fieldType == typeof(bool))
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, bool>), "Fire")));
            }
            else if (fieldType == typeof(int))
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, int>), "Fire")));
            }
            else if (fieldType == typeof(float))
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, float>), "Fire")));
            }
            else if (fieldType == typeof(double))
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, double>), "Fire")));
            }
            else if (fieldType == typeof(string))
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, string>), "Fire")));
            }
            else if (fieldType == typeof(Quaternion))
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, Quaternion>), "Fire")));
            }
            else if (fieldType == typeof(Vector3))
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, Vector3>), "Fire")));
            }
            else if(fieldType.IsEnum)
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, int, string>), "Fire")));
            }
            else
            {
                _codes.Insert(LastIndex, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(EventData<PartModule, string, object>), "Fire")));
            }
        }
    }
}
