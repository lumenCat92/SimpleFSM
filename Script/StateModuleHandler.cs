using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
namespace LumenCat92.SimpleFSM
{
    public class StateModuleHandler
    {
        protected List<StateModule> Modules { set; get; } = new List<StateModule>();
        protected int lastPlayingModuleIndex = -1;
        protected ModuleHandlerLockData HandlerLockData { set; get; }
        public bool IsSameModule(int num) => num == lastPlayingModuleIndex;

        public virtual void EnterModule(int targetModuleIndex, StateModule.StateModuleData moduleData = null, int releaseStateNum = -1)
        {
            var targetModule = GetModule(targetModuleIndex);
            if (targetModule == null) return;
            if (!CanTransitionToModule(targetModuleIndex)) return;

            if (targetModule.CanEnter(moduleData))
            {
                lastPlayingModuleIndex = targetModuleIndex;
                targetModule.Enter();

                if (releaseStateNum >= 0)
                {
                    HandlerLockData = new ModuleHandlerLockData(targetModuleIndex, releaseStateNum);
                }
            }
            else
            {
                targetModule.EnterModuleToException();
            }
        }

        private bool CanTransitionToModule(int targetModuleIndex)
        {
            if (HandlerLockData != null && targetModuleIndex != HandlerLockData.ReleaseStateNum)
            {
                return false;
            }

            HandlerLockData = null;

            if (IsPlayingModuleRunning(out StateModule playingModule))
            {
                playingModule.Exit();
            }

            return true;
        }

        public StateModule GetModule(int num)
        {
            if (num < 0) return null;

            if (num < Modules.Count)
                return Modules[num];
            else
            {
                UnityEngine.Debug.Log("Module Couldnt found");
                return null;
            }
        }
        public int GetPlayingModuleIndex() => lastPlayingModuleIndex;
        public StateModule GetPlayingModule() => GetModule(lastPlayingModuleIndex);
        public bool IsPlayingModuleRunning() => IsPlayingModuleRunning(out StateModule stateModule);
        public bool IsPlayingModuleRunning(out StateModule stateModule)
        {
            stateModule = GetPlayingModule();
            if (stateModule == null)
            {
                return false;
            }
            return stateModule.IsModuleRunning;
        }
        public T GetPlayingModule<T>() where T : StateModule => GetPlayingModule() as T;
        protected class ModuleHandlerLockData
        {
            public int RequestStateNum { set; get; } = -1;
            public int ReleaseStateNum { set; get; } = -1;
            public ModuleHandlerLockData(int requestStateNum, int releaseStateNum)
            {
                RequestStateNum = requestStateNum;
                ReleaseStateNum = releaseStateNum;
            }
        }
    }
}