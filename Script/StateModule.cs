using System.Collections.Generic;
using UnityEngine;

namespace LumenCat92.SimpleFSM
{
    public abstract class StateModule
    {
        public bool IsModuleRunning { private set; get; } = false;
        protected StateModuleData ModuleData { set; get; }
        public bool CanEnter<T>(T t = null) where T : StateModuleData
        {
            ModuleData = t;
            if (IsReady())
            {
                return true;
            }

            ModuleData = null;
            return false;
        }
        public abstract bool IsReady();
        public abstract void EnterModuleToException();
        public void Enter() { IsModuleRunning = true; OnEnterModule(); }
        public void Exit() { IsModuleRunning = false; OnExitModule(); }
        protected abstract void OnEnterModule();
        protected abstract void OnExitModule();
        public abstract class StateModuleData { }
    }
}