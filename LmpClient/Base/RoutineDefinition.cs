﻿using System;
using System.Diagnostics;
using UnityEngine.Profiling;

namespace LmpClient.Base
{
    public enum RoutineExecution
    {
        FixedUpdate,
        Update,
        LateUpdate
    }

    /// <summary>
    /// This class defines a routine that must be executed during an update or a fixed/late update
    /// </summary>
    public class RoutineDefinition
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// Routine name is the name of the method it runs
        /// </summary>
        public string Name => Method.Method.Name;

        /// <summary>
        /// Interval in ms at witch this routine will be executed. Set it to 0 if you want to run it on every update/fixed update
        /// </summary>
        public int IntervalInMs { get; set; }
        
        /// <summary>
        /// Method that this routine will execute
        /// </summary>
        public Action Method { private get; set; }

        /// <summary>
        /// The method name
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Select here if the routine must run in update or in fixed update
        /// </summary>
        public RoutineExecution Execution { get; set; }

        #region Constructors

        private RoutineDefinition() => _stopwatch.Start();

        /// <summary>
        /// Create a routine definition. Set the interval to 0 if you want to execute it on every update/fixed update
        /// Set the runOnce to true if you want to run the routine only 1 time
        /// </summary>
        public RoutineDefinition(int intervalInMs, RoutineExecution execution, Action method) : this()
        {
            IntervalInMs = intervalInMs;
            Execution = execution;
            Method = method;
            MethodName = method.Method.Name;
        }

        #endregion

        /// <summary>
        /// Call this method to try to run the routine if the interval is ok
        /// </summary>
        public void RunRoutine()
        {
            if (IntervalInMs <= 0 || _stopwatch.ElapsedMilliseconds > IntervalInMs)
            {
                Profiler.BeginSample(MethodName);

                Method.Invoke();

                _stopwatch.Reset();
                _stopwatch.Start();

                Profiler.EndSample();
            }
        }
    }
}
