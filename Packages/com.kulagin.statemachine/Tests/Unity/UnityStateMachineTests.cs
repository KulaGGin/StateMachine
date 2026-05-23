using System;
using NUnit.Framework;
using Kulagin.StateMachine.Core;
using Kulagin.StateMachine.Unity;

namespace Kulagin.StateMachine.Unity.Tests {
    public class UnityStateMachineTests {
        private class TestStateMachine : UnityStateMachine<TestStateMachine, TestState> {
        }

        private class TestState : GameplayState<TestStateMachine, TestState> {
            public bool AwakeCalled;
            public bool StartCalled;
            public bool PhysicsUpdateCalled;
            public bool FrameUpdateCalled;
            public bool LateUpdateCalled;

            public int AwakeCount;
            public int StartCount;
            public int PhysicsCount;
            public int FrameCount;
            public int LateCount;

            public TestState(TestStateMachine StateMachine) : base(StateMachine) {
            }

            public override void Awake() {
                AwakeCalled = true;
                AwakeCount++;
            }

            public override void Start() {
                StartCalled = true;
                StartCount++;
            }

            public override void PhysicsUpdate() {
                PhysicsUpdateCalled = true;
                PhysicsCount++;
            }

            public override void FrameUpdate() {
                FrameUpdateCalled = true;
                FrameCount++;
            }

            public override void LateUpdate() {
                LateUpdateCalled = true;
                LateCount++;
            }
        }

        private class IdleState : TestState {
            public IdleState(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        private class MoveState : TestState {
            public MoveState(TestStateMachine StateMachine) : base(StateMachine) {
            }
        }

        [Test]
        public void Awake_ForwardsToCurrentState() {
            var StateMachine = new TestStateMachine();

            var Idle = new IdleState(StateMachine);
            StateMachine.SetStates(Idle);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.Awake();

            Assert.IsTrue(Idle.AwakeCalled);
        }

        [Test]
        public void Start_ForwardsToCurrentState() {
            var StateMachine = new TestStateMachine();

            var Idle = new IdleState(StateMachine);
            StateMachine.SetStates(Idle);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.Start();

            Assert.IsTrue(Idle.StartCalled);
        }

        [Test]
        public void PhysicsUpdate_ForwardsToCurrentState() {
            var StateMachine = new TestStateMachine();

            var Idle = new IdleState(StateMachine);
            StateMachine.SetStates(Idle);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.PhysicsUpdate();

            Assert.IsTrue(Idle.PhysicsUpdateCalled);
        }

        [Test]
        public void FrameUpdate_ForwardsToCurrentState() {
            var StateMachine = new TestStateMachine();

            var Idle = new IdleState(StateMachine);
            StateMachine.SetStates(Idle);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.FrameUpdate();

            Assert.IsTrue(Idle.FrameUpdateCalled);
        }

        [Test]
        public void LateUpdate_ForwardsToCurrentState() {
            var StateMachine = new TestStateMachine();

            var Idle = new IdleState(StateMachine);
            StateMachine.SetStates(Idle);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.LateUpdate();

            Assert.IsTrue(Idle.LateUpdateCalled);
        }

        [Test]
        public void Lifecycle_Methods_WorkAfterStateChange() {
            var StateMachine = new TestStateMachine();

            var Idle = new IdleState(StateMachine);
            var Move = new MoveState(StateMachine);

            StateMachine.SetStates(Idle, Move);

            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<MoveState>();

            StateMachine.FrameUpdate();
            StateMachine.PhysicsUpdate();
            StateMachine.LateUpdate();

            Assert.IsTrue(Move.FrameUpdateCalled);
            Assert.IsTrue(Move.PhysicsUpdateCalled);
            Assert.IsTrue(Move.LateUpdateCalled);
        }

        [Test]
        public void MultipleLifecycleCalls_AreCountedCorrectly() {
            var StateMachine = new TestStateMachine();

            var Idle = new IdleState(StateMachine);
            StateMachine.SetStates(Idle);

            StateMachine.StartStateMachine<IdleState>();

            StateMachine.Awake();
            StateMachine.Awake();
            StateMachine.Start();

            Assert.AreEqual(2, Idle.AwakeCount);
            Assert.AreEqual(1, Idle.StartCount);
        }

        [Test]
        public void CallingLifecycleBeforeStart_DoesNotThrow() {
            var StateMachine = new TestStateMachine();

            var Idle = new IdleState(StateMachine);
            StateMachine.SetStates(Idle);

            Assert.Throws<InvalidOperationException>(() => { StateMachine.Awake(); });
        }

        [Test]
        public void CurrentState_IsUsedForAllLifecycleCalls() {
            var StateMachine = new TestStateMachine();

            var Idle = new IdleState(StateMachine);
            var Move = new MoveState(StateMachine);

            StateMachine.SetStates(Idle, Move);

            StateMachine.StartStateMachine<IdleState>();
            StateMachine.ApplyState<MoveState>();

            StateMachine.Awake();
            StateMachine.Start();
            StateMachine.FrameUpdate();
            StateMachine.PhysicsUpdate();
            StateMachine.LateUpdate();

            Assert.IsFalse(Idle.AwakeCalled);
            Assert.IsTrue(Move.AwakeCalled);
        }
        
        [Test]
        public void FrameUpdate_WithoutStarting_ThrowsInvalidOperationException() {
            var StateMachine = new TestStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);
            
            var Ex = Assert.Throws<InvalidOperationException>(() => StateMachine.FrameUpdate()
            );

            Assert.That(Ex.Message, Contains.Substring("not been started"));
        }

        [Test]
        public void PhysicsUpdate_WithoutStarting_ThrowsInvalidOperationException() {
            var StateMachine = new TestStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);
            
            var Ex = Assert.Throws<InvalidOperationException>(() => StateMachine.PhysicsUpdate()
            );

            Assert.That(Ex.Message, Contains.Substring("not been started"));
        }

        [Test]
        public void LateUpdate_WithoutStarting_ThrowsInvalidOperationException() {
            var StateMachine = new TestStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);
            
            var Ex = Assert.Throws<InvalidOperationException>(() => StateMachine.LateUpdate()
            );

            Assert.That(Ex.Message, Contains.Substring("not been started"));
        }

        [Test]
        public void FrameUpdate_AfterStart_CallsStateFrameUpdate() {
            var StateMachine = new TestStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);
            StateMachine.StartStateMachine<IdleState>();
            
            StateMachine.FrameUpdate();
            
            Assert.IsTrue(IdleState.FrameUpdateCalled);
        }

        [Test]
        public void PhysicsUpdate_AfterStart_CallsStatePhysicsUpdate() {
            var StateMachine = new TestStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);
            StateMachine.StartStateMachine<IdleState>();
            
            StateMachine.PhysicsUpdate();
            
            Assert.IsTrue(IdleState.PhysicsUpdateCalled);
        }

        [Test]
        public void LateUpdate_AfterStart_CallsStateLateUpdate() {
            var StateMachine = new TestStateMachine();
            var IdleState = new IdleState(StateMachine);

            StateMachine.SetStates(IdleState);
            StateMachine.StartStateMachine<IdleState>();
            
            StateMachine.LateUpdate();
            
            Assert.IsTrue(IdleState.LateUpdateCalled);
        }
    }
}