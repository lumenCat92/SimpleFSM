# LumenCat
<div align="center">

![LumenCat92.jpg](https://github.com/lumenCat92/SimpleFSM/blob/main/Image/LumenCat92.jpg)
</div>

# SimpleFSM
![SimpleFSM.jpg](https://github.com/lumenCat92/SimpleFSM/blob/main/Image/SimpleFSM.jpg)

# Language
<details>
<summary>English</summary>

# How Can Install This?

Download this to Assets Folder in your unity project.

# What is This?

This code provide u simple way to using FSM system.

# Where Can Use This?

If you want to do something different depending on the status and also want very base code of FSM. this will be perfect.

# Why Should Use This?

This FSM (Finite State Machine) system provides the most basic code required for an FSM. You don't need to know a lot about FSMs to use it. It has a simple structure consisting of three steps for each state: Prepare -> Start -> End.  Moreover, it is designed to be easily customizable to your liking.

# How to Use This?

1. when u look at the state module handler
```csharp
public class StateModuleHandler
{
    protected List<StateModule> Modules { set; get; } = new List<StateModule>();
    protected int lastPlayingModuleIndex = -1;
    protected ModuleHandlerLockData HandlerLockData { set; get; }
    public bool IsSameModule(int num) => num == lastPlayingModuleIndex;

    public virtual void EnterModule(int targetModuleIndex, StateModule.StateModuleData moduleData = null, int releaseStateNum = -1)
    {
        var targetModule = GetModule(targetModuleIndex);
        if (targetModule == null)
        {
            UnityEngine.Debug.LogError("target Module is empty. index : " + targetModuleIndex);
            return;
        }

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
}
```
as u can see, u have to add each mouduler to StateMouduleHandler first.
of Cource u can use StateModuler with independently, but my recommand is using with handler.

and also u can lock the state change until a particular state is approached by using ModuleHandlerLockData.
```csharp
public class StateModuleHandler
{
    protected ModuleHandlerLockData HandlerLockData { set; get; }
    public void EnterModule(int targetModuleIndex, StateModule.StateModuleData moduleData = null)
    {
        //
        //skip
        //
        if (targetModule.CanEnter(moduleData))
        {
            playingModuleIndex = targetModuleIndex;
            targetModule.Enter();

            if (releaseStateNum >= 0)
            {
                HandlerLockData = new ModuleHandlerLockData(targetModuleIndex, releaseStateNum);
            }
        }
        //
        //skip
        //
    }

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
```

2. when u look at the StateModule
```csharp
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
```
it exist as abstract. so u have to redefine first.
as we have already seen in Step 1, the sequence flow is as follows.
CanEnter => isReady == true ? Enter() : EnterModuleToException(); => When State Change => Exit();

StateModuleData is usually using for preparing module state before enter state.


* Example
```csharp
public class HumanAniController : AniController
{
    protected override StateModuleHandler GetStateModuleHandler()
    {
        return new HumanAniStateModuleHandler(animator, ragDollHandler);
    }
}

public class HumanAniStateModuleHandler : StateModuleHandler
{
    public RagDollHandler RagDollHandler { private set; get; }
    public Animator Animator { private set; get; }
    public HumanAniStateModuleHandler(Animator animator, RagDollHandler ragDollHandler)
    {
        Animator = animator;
        RagDollHandler = ragDollHandler;
        Modules = HumanAniState.GetStatesList(this);
    }

    public void EnterModule(HumanAniState.StateKind state, StateModule.StateModuleData prepareData = null)
    {
        base.EnterModule((int)state, prepareData);
    }

    public HumanAniState GetModule(HumanAniState.StateKind state) => GetModule((int)state) as HumanAniState;
}

public abstract class HumanAniState : StateModule
{
    //*if you need change state, you have to controll here frist*/
    public enum StateKind
    {
        Reset,
        Walk,
        //Run,
        Standing,
        LookAround,
        Sitting,
        Surprize,
        KeepingWeapon,
        HoldingWeapon,
        UsingWeapon,
        Attack,
        Reload,
        // Avoid,
        TurnAround,
        Dead,
        Non,
    }

    public static List<StateModule> GetStatesList(HumanAniStateModuleHandler moduleHandler)
    {
        if (moduleHandler != null)
        {
            var stateList = new List<StateModule>();

            StateModule state = null;
            for (StateKind stateKind = StateKind.Reset; stateKind <= StateKind.Non; stateKind++)
            {
                switch (stateKind)
                {
                    case StateKind.Non: state = new Non_HumanAniState(moduleHandler); break;
                    case StateKind.Reset: state = new Reset_HumanAniState(moduleHandler); break;
                    case StateKind.Walk: state = new Walk_HumanAniState(moduleHandler); break;
                    case StateKind.Standing: state = new Standing_HumanAniState(moduleHandler); break;
                    case StateKind.LookAround: state = new LookAround_HumanAniState(moduleHandler); break;
                    case StateKind.Sitting: state = new Sitting_HumanAniState(moduleHandler); break;
                    case StateKind.Surprize: state = new Surprize_HumanAniState(moduleHandler); break;
                    case StateKind.KeepingWeapon: state = new KeepingWeapon_HumanAniState(moduleHandler); break;
                    case StateKind.HoldingWeapon: state = new HoldingWeapon_HumanAniState(moduleHandler); break;
                    case StateKind.UsingWeapon: state = new UsingWeapon_HumanAniState(moduleHandler); break;
                    case StateKind.Attack: state = new Attack_HumanAniState(moduleHandler); break;
                    case StateKind.Reload: state = new Reload_HumanAniState(moduleHandler); break;
                    case StateKind.TurnAround: state = new TurnAround_HumanAniState(moduleHandler); break;
                    case StateKind.Dead: state = new Dead_HumanAniState(moduleHandler); break;
                }

                stateList.Add(state);
            }

            return stateList;
        }

        return null;
    }
}
```
</details>

<details>
<summary>한국어</summary>

# 어떻게 설치하죠?

직접 다운로드해서 프로젝트의 Assets에 설치합니다.

# 이게 뭐죠?

심플하게 구현된 FSM 시스템입니다.

# 어디에 쓰나요?

상태에 따라 다른 일을 하고 싶다거나, FSM의 기초적인 코드도 원한다면 쓰시면 됩니다.

# 왜 써야할까요?

이 FSM(Finite State Machine)은 FSM에 필요한 가장 기본적인 코드를 제공합니다. 

FSM을 사용하기 위해 너무 많은 것을 알 필요는 없습니다. 

각 상태별로 준비 -> 시작 -> 종료의 세 단계로 구성된 간단한 구조를 가지고 있음으로 시작하기 편리합니다. 

또한, 원하는 대로 쉽게 사용자 커스텀이 가능하도록 설계되었습니다.

# 어떻게 사용하나요?

1. StateModuleHandler를 보시게 되면
```csharp
public class StateModuleHandler
{
    protected List<StateModule> Modules { set; get; } = new List<StateModule>();
    protected int lastPlayingModuleIndex = -1;
    protected ModuleHandlerLockData HandlerLockData { set; get; }
    public bool IsSameModule(int num) => num == lastPlayingModuleIndex;

    public virtual void EnterModule(int targetModuleIndex, StateModule.StateModuleData moduleData = null, int releaseStateNum = -1)
    {
        var targetModule = GetModule(targetModuleIndex);
        if (targetModule == null)
        {
            UnityEngine.Debug.LogError("target Module is empty. index : " + targetModuleIndex);
            return;
        }

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
}
```
보시는 것처럼 StateModuleHandler에서 사용할 각 StateModule들을 먼저 정의하여 넣어야 합니다.

물론 StateModule를 개별적으로 사용할 수도 있으나, StateModuleHandler과 함께 사용을 권장드립니다.


또한 ModuleHandlerLockData를 이용하여 특정 StateModule이 들어오기까지 State변화를 막을 수 있습니다.
```csharp
public class StateModuleHandler
{
    protected ModuleHandlerLockData HandlerLockData { set; get; }
    public void EnterModule(int targetModuleIndex, StateModule.StateModuleData moduleData = null)
    {
        //
        //skip
        //
        if (targetModule.CanEnter(moduleData))
        {
            playingModuleIndex = targetModuleIndex;
            targetModule.Enter();

            if (releaseStateNum >= 0)
            {
                HandlerLockData = new ModuleHandlerLockData(targetModuleIndex, releaseStateNum);
            }
        }
        //
        //skip
        //
    }

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
```

2. StateModule를 보면
```csharp
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
```
abstract 상태로 되어있어 먼저 재정의가 필요합니다.

이미 Step1에서 본 것과 같이 시퀀스 플로는 아래와 같이 정의됩니다.

CanEnter => isReady == true ? Enter() : EnterModuleToException(); => When State Change => Exit();


StateModuleData는 일반적으로 StateModule에 들어가기 전의 준비하는 데이터라 보시면 됩니다.


* 실제 개인 프로젝트에 들어가 있는 사용 예시
```csharp
public class HumanAniController : AniController
{
    protected override StateModuleHandler GetStateModuleHandler()
    {
        return new HumanAniStateModuleHandler(animator, ragDollHandler);
    }
}

public class HumanAniStateModuleHandler : StateModuleHandler
{
    public RagDollHandler RagDollHandler { private set; get; }
    public Animator Animator { private set; get; }
    public HumanAniStateModuleHandler(Animator animator, RagDollHandler ragDollHandler)
    {
        Animator = animator;
        RagDollHandler = ragDollHandler;
        Modules = HumanAniState.GetStatesList(this);
    }

    public void EnterModule(HumanAniState.StateKind state, StateModule.StateModuleData moduleData = null)
    {
        base.EnterModule((int)state, moduleData);
    }

    public HumanAniState GetModule(HumanAniState.StateKind state) => GetModule((int)state) as HumanAniState;
}

public abstract class HumanAniState : StateModule
{
    //*if you need change state, you have to control here frist*/
    public enum StateKind
    {
        Reset,
        Walk,
        //Run,
        Standing,
        LookAround,
        Sitting,
        Surprize,
        KeepingWeapon,
        HoldingWeapon,
        UsingWeapon,
        Attack,
        Reload,
        // Avoid,
        TurnAround,
        Dead,
        Non,
    }

    public static List<StateModule> GetStatesList(HumanAniStateModuleHandler moduleHandler)
    {
        if (moduleHandler != null)
        {
            var stateList = new List<StateModule>();

            StateModule state = null;
            for (StateKind stateKind = StateKind.Reset; stateKind <= StateKind.Non; stateKind++)
            {
                switch (stateKind)
                {
                    case StateKind.Non: state = new Non_HumanAniState(moduleHandler); break;
                    case StateKind.Reset: state = new Reset_HumanAniState(moduleHandler); break;
                    case StateKind.Walk: state = new Walk_HumanAniState(moduleHandler); break;
                    case StateKind.Standing: state = new Standing_HumanAniState(moduleHandler); break;
                    case StateKind.LookAround: state = new LookAround_HumanAniState(moduleHandler); break;
                    case StateKind.Sitting: state = new Sitting_HumanAniState(moduleHandler); break;
                    case StateKind.Surprize: state = new Surprize_HumanAniState(moduleHandler); break;
                    case StateKind.KeepingWeapon: state = new KeepingWeapon_HumanAniState(moduleHandler); break;
                    case StateKind.HoldingWeapon: state = new HoldingWeapon_HumanAniState(moduleHandler); break;
                    case StateKind.UsingWeapon: state = new UsingWeapon_HumanAniState(moduleHandler); break;
                    case StateKind.Attack: state = new Attack_HumanAniState(moduleHandler); break;
                    case StateKind.Reload: state = new Reload_HumanAniState(moduleHandler); break;
                    case StateKind.TurnAround: state = new TurnAround_HumanAniState(moduleHandler); break;
                    case StateKind.Dead: state = new Dead_HumanAniState(moduleHandler); break;
                }

                stateList.Add(state);
            }

            return stateList;
        }

        return null;
    }
}
```