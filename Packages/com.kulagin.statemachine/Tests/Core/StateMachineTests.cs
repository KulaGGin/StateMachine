using System;
using NUnit.Framework;

namespace Kulagin.StateMachine.Core.Tests {
    class TestStateMachine : StateMachine<TestStateMachine, TestState> {
        public TestStateMachine() {
        }


        public TestStateMachine(params TestState[] StartStates) : base(StartStates) {
        }
    }

    class TestState : State<TestStateMachine, TestState> {
        public bool Entered;
        public bool Exited;
        public int EnterCount;
        public int ExitCount;
        public object ReceivedArgs;

        protected TestState(TestStateMachine TestStateMachine) : base(TestStateMachine) {
        }

        public override object EnterState(object StateEventArgs = null) {
            Entered = true;
            EnterCount++;
            ReceivedArgs = StateEventArgs;
            return StateEventArgs;
            
        }

        public override void ExitState() {
            Exited = true;
            ExitCount++;
        }
    }
    
    
    readonly struct AttackEvent {
        public readonly string Tag;
        public AttackEvent(string Tag) { this.Tag = Tag; }
    }

    class IdleState : TestState {
        public IdleState(TestStateMachine TestStateMachine) : base(TestStateMachine) {
        }
    }

    class WalkingState : TestState {
        public WalkingState(TestStateMachine TestStateMachine) : base(TestStateMachine) {
        }
    }
    
    public readonly struct HitEvent {
        public readonly string Tag;
        public HitEvent(string Tag) { this.Tag = Tag; }
    }
    
    public class TestStateMachineTests {
        private TestStateMachine TestStateMachine;
        private IdleState IdleState;
        private WalkingState WalkingState;

        [SetUp]
        public void SetUp() {
            TestStateMachine = new TestStateMachine();
            IdleState = new IdleState(TestStateMachine);
            WalkingState = new WalkingState(TestStateMachine);
        }

        [Test]
        public void Constructor_WithEnumerable_RegistersStates() {
            TestStateMachine = new TestStateMachine(new TestState[] {
                IdleState,
                WalkingState
            });

            TestStateMachine.StartStateMachine<WalkingState>();

            Assert.AreEqual(WalkingState, TestStateMachine.CurrentState);
        }

        [Test]
        public void Constructor_WithParams_RegistersStates() {
            TestStateMachine = new TestStateMachine(IdleState, WalkingState);

            TestStateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(IdleState, TestStateMachine.CurrentState);
        }

        [Test]
        public void SetStates_WithEnumerable_RegistersStates() {
            TestStateMachine.SetStates(new TestState[] {
                IdleState,
                WalkingState
            });

            TestStateMachine.StartStateMachine<WalkingState>();

            Assert.AreEqual(WalkingState, TestStateMachine.CurrentState);
        }

        [Test]
        public void SetStates_WithParams_RegistersStates() {
            TestStateMachine.SetStates(IdleState, WalkingState);

            TestStateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(IdleState, TestStateMachine.CurrentState);
        }

        [Test]
        public void SetStates_ClearsPreviousStates() {
            TestStateMachine.SetStates(IdleState);
            TestStateMachine.SetStates(WalkingState);

            Assert.Throws<ArgumentException>(TestStateMachine.StartStateMachine<IdleState>);
        }

        [Test]
        public void StartStateMachine_Generic_StartsCorrectState() {
            TestStateMachine.SetStates(IdleState);

            TestStateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(IdleState, TestStateMachine.CurrentState);
        }

        [Test]
        public void StartStateMachine_Type_StartsCorrectState() {
            TestStateMachine.SetStates(IdleState);

            TestStateMachine.StartStateMachine(typeof(IdleState));

            Assert.AreEqual(IdleState, TestStateMachine.CurrentState);
        }

        [Test]
        public void StartStateMachine_Generic_CallsEnterState() {
            TestStateMachine.SetStates(IdleState);

            TestStateMachine.StartStateMachine<IdleState>();

            Assert.IsTrue(IdleState.Entered);
        }

        [Test]
        public void StartStateMachine_Type_CallsEnterState() {
            TestStateMachine.SetStates(IdleState);

            TestStateMachine.StartStateMachine(typeof(IdleState));

            Assert.IsTrue(IdleState.Entered);
        }

        [Test]
        public void StartStateMachine_CallsEnterStateExactlyOnce() {
            TestStateMachine.SetStates(IdleState);

            TestStateMachine.StartStateMachine<IdleState>();

            Assert.AreEqual(1, IdleState.EnterCount);
        }

        [Test]
        public void IsInState_Generic_ReturnsTrueForCurrentState() {
            TestStateMachine.SetStates(IdleState);

            TestStateMachine.StartStateMachine<IdleState>();

            Assert.IsTrue(TestStateMachine.IsInState<IdleState>());
        }

        [Test]
        public void IsInState_Generic_ReturnsFalseForDifferentState() {
            TestStateMachine.SetStates(IdleState, WalkingState);

            TestStateMachine.StartStateMachine<IdleState>();

            Assert.IsFalse(TestStateMachine.IsInState<WalkingState>());
        }

        [Test]
        public void IsInState_Type_ReturnsTrueForCurrentState() {
            TestStateMachine.SetStates(IdleState);
            TestStateMachine.StartStateMachine<IdleState>();

            Assert.IsTrue(TestStateMachine.IsInState(typeof(IdleState)));
        }

        [Test]
        public void IsInState_Type_ReturnsFalseForDifferentState() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            Assert.IsFalse(TestStateMachine.IsInState(typeof(WalkingState)));
        }

        [Test]
        public void ApplyState_Generic_ChangesCurrentState() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState<WalkingState>();

            Assert.AreEqual(WalkingState, TestStateMachine.CurrentState);
        }

        [Test]
        public void ApplyState_Type_ChangesCurrentState() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState(typeof(WalkingState));

            Assert.AreEqual(WalkingState, TestStateMachine.CurrentState);
        }

        [Test]
        public void ApplyState_CallsExitStateOnPreviousState() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState<WalkingState>();

            Assert.IsTrue(IdleState.Exited);
        }

        [Test]
        public void ApplyState_CallsExitStateExactlyOnce() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState<WalkingState>();

            Assert.AreEqual(1, IdleState.ExitCount);
        }

        [Test]
        public void ApplyState_CallsEnterStateOnNewState() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState<WalkingState>();

            Assert.IsTrue(WalkingState.Entered);
        }

        [Test]
        public void ApplyState_CallsEnterStateExactlyOnceOnNewState() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState<WalkingState>();

            Assert.AreEqual(1, WalkingState.EnterCount);
        }

        [Test]
        public void ApplyState_ForwardsArgumentsToNewState() {
            TestStateMachine.SetStates(IdleState, WalkingState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState<WalkingState>("Hello");

            Assert.AreEqual("Hello", WalkingState.ReceivedArgs);
        }

        [Test]
        public void ApplyState_ToSameState_ExitsAndReEntersState() {
            TestStateMachine.SetStates(IdleState);
            TestStateMachine.StartStateMachine<IdleState>();

            IdleState.Entered = false;
            IdleState.Exited = false;

            TestStateMachine.ApplyState<IdleState>();

            Assert.IsTrue(IdleState.Exited);
            Assert.IsTrue(IdleState.Entered);
        }

        [Test]
        public void ApplyState_ToSameState_CallsLifecycleMethodsAgain() {
            TestStateMachine.SetStates(IdleState);
            TestStateMachine.StartStateMachine<IdleState>();

            TestStateMachine.ApplyState<IdleState>();

            Assert.AreEqual(2, IdleState.EnterCount);
            Assert.AreEqual(1, IdleState.ExitCount);
        }

        [Test]
        public void ApplyState_BeforeStarting_ThrowsNullReferenceException() {
            TestStateMachine.SetStates(IdleState);

            Assert.Throws<NullReferenceException>(() => { TestStateMachine.ApplyState<IdleState>(); });
        }

        [Test]
        public void StartStateMachine_Generic_InvalidState_ThrowsArgumentException() {
            TestStateMachine.SetStates(IdleState);

            var Ex = Assert.Throws<ArgumentException>(TestStateMachine.StartStateMachine<WalkingState>);

            Assert.That(Ex.Message, Contains.Substring("WalkingState"));
            Assert.That(Ex.ParamName, Is.EqualTo("StartingState"));
        }

        [Test]
        public void StartStateMachine_Type_InvalidState_ThrowsArgumentException() {
            TestStateMachine.SetStates(IdleState);

            var Ex = Assert.Throws<ArgumentException>(() => TestStateMachine.StartStateMachine(typeof(WalkingState)));

            Assert.That(Ex.Message, Contains.Substring("WalkingState"));
        }

        [Test]
        public void StartStateMachine_InvalidState_DoesNotSetCurrentState() {
            TestStateMachine.SetStates(IdleState);

            Assert.Throws<ArgumentException>(TestStateMachine.StartStateMachine<WalkingState>);

            Assert.IsNull(TestStateMachine.CurrentState);
        }

        [Test]
        public void ApplyState_InvalidState_ThrowsArgumentException() {
            TestStateMachine.SetStates(IdleState);
            TestStateMachine.StartStateMachine<IdleState>();

            var Ex = Assert.Throws<ArgumentException>(() => TestStateMachine.ApplyState<WalkingState>());

            Assert.That(Ex.Message, Contains.Substring("WalkingState"));
        }

        [Test]
        public void ApplyState_InvalidState_PreservesCurrentState() {
            TestStateMachine.SetStates(IdleState);
            TestStateMachine.StartStateMachine<IdleState>();

            Assert.Throws<ArgumentException>(() => TestStateMachine.ApplyState<WalkingState>());

            Assert.AreEqual(IdleState, TestStateMachine.CurrentState);
        }
        
        private class AttackHandlerState : TestState, IHandle<AttackEvent> {
            public bool Handled;
            public string LastTag;
            public AttackHandlerState(TestStateMachine StateMachine) : base(StateMachine) {
            }

            public bool Handle(AttackEvent Event) {
                Handled = true;
                LastTag = Event.Tag;
                return true;
            }
        }

        [Test]
        public void Send_WhenCurrentStateHandles_ReturnsTrueAndInvokesHandler() {
            TestStateMachine StateMachine = new();
            AttackHandlerState State = new(StateMachine);
            StateMachine.SetStates(State);
            StateMachine.StartStateMachine<AttackHandlerState>();

            bool Result = StateMachine.Send(new AttackEvent("hit"));

            Assert.IsTrue(Result);
            Assert.IsTrue(State.Handled);
            Assert.AreEqual("hit", State.LastTag);
        }
        
        
        private class PlainState : TestState {   // implements no IHandle<>
            public PlainState(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        [Test]
        public void Send_WhenCurrentStateDoesNotImplementHandler_ReturnsFalse() {
            TestStateMachine StateMachine = new();
            PlainState State = new(StateMachine);
            StateMachine.SetStates(State);
            StateMachine.StartStateMachine<PlainState>();

            Assert.IsFalse(StateMachine.Send(new AttackEvent("hit")));
        }
        
        private abstract class DefendingStateBase : TestState, IHandle<AttackEvent> {
            public bool Handled;
            protected DefendingStateBase(TestStateMachine StateMachine) : base(StateMachine) {
            }

            public virtual bool Handle(AttackEvent Event) {
                Handled = true;
                return true;
            }
        }

        private class DefendingLeafState : DefendingStateBase {   // overrides nothing — inherits Handle
            public DefendingLeafState(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        [Test]
        public void Send_FindsHandlerInheritedFromBaseStateClass() {
            TestStateMachine StateMachine = new();
            DefendingLeafState State = new(StateMachine);
            StateMachine.SetStates(State);
            StateMachine.StartStateMachine<DefendingLeafState>();

            bool Result = StateMachine.Send(new AttackEvent("x"));

            Assert.IsTrue(Result);
            Assert.IsTrue(State.Handled);
        }
        
        
        private readonly struct DuckEvent { }

        private class DualHandlerState : TestState, IHandle<HitEvent>, IHandle<DuckEvent> {
            public string Last;
            public DualHandlerState(TestStateMachine StateMachine) : base(StateMachine) {
            }

            public bool Handle(HitEvent Event)  { Last = "Hit";  return true; }
            public bool Handle(DuckEvent Event) { Last = "Duck"; return true; }
        }

        [Test]
        public void Send_RoutesToHandlerOfMatchingEventType() {
            TestStateMachine StateMachine = new();
            DualHandlerState State = new(StateMachine);
            StateMachine.SetStates(State);
            StateMachine.StartStateMachine<DualHandlerState>();

            StateMachine.Send(new DuckEvent());
            Assert.AreEqual("Duck", State.Last);

            StateMachine.Send(new HitEvent("x"));
            Assert.AreEqual("Hit", State.Last);
        }
    }
}