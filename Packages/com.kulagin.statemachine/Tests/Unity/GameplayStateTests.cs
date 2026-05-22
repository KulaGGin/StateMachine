using Kulagin.StateMachine.Core;
using NUnit.Framework;

namespace Kulagin.StateMachine.Unity.Tests {
    public class GameplayStateTests {
        private class TestStateMachine : StateMachine<TestStateMachine, TestGameplayState> {
        }

        private class TestGameplayState : GameplayState<TestStateMachine, TestGameplayState> {
            public bool AwakeCalled;
            public bool StartCalled;
            public bool FrameUpdateCalled;
            public bool PhysicsUpdateCalled;
            public bool LateUpdateCalled;

            public int AwakeCallCount;
            public int StartCallCount;
            public int FrameUpdateCallCount;
            public int PhysicsUpdateCallCount;
            public int LateUpdateCallCount;

            public TestGameplayState(TestStateMachine StateMachine) : base(StateMachine) {
            }

            public override void Awake() {
                AwakeCalled = true;
                AwakeCallCount++;
            }

            public override void Start() {
                StartCalled = true;
                StartCallCount++;
            }

            public override void FrameUpdate() {
                FrameUpdateCalled = true;
                FrameUpdateCallCount++;
            }

            public override void PhysicsUpdate() {
                PhysicsUpdateCalled = true;
                PhysicsUpdateCallCount++;
            }

            public override void LateUpdate() {
                LateUpdateCalled = true;
                LateUpdateCallCount++;
            }
        }

        [Test]
        public void Constructor_StoresStateMachineReference() {
            var StateMachine = new TestStateMachine();

            var State = new TestGameplayState(StateMachine);

            Assert.AreEqual(StateMachine, State.StateMachine);
        }

        [Test]
        public void Awake_CanBeOverridden() {
            var StateMachine = new TestStateMachine();

            var State = new TestGameplayState(StateMachine);

            State.Awake();

            Assert.IsTrue(State.AwakeCalled);
        }

        [Test]
        public void Awake_CallsOverrideExactlyOnce() {
            var StateMachine = new TestStateMachine();

            var State = new TestGameplayState(StateMachine);

            State.Awake();

            Assert.AreEqual(1, State.AwakeCallCount);
        }

        [Test]
        public void Start_CanBeOverridden() {
            var StateMachine = new TestStateMachine();

            var State = new TestGameplayState(StateMachine);

            State.Start();

            Assert.IsTrue(State.StartCalled);
        }

        [Test]
        public void Start_CallsOverrideExactlyOnce() {
            var StateMachine = new TestStateMachine();

            var State = new TestGameplayState(StateMachine);

            State.Start();

            Assert.AreEqual(1, State.StartCallCount);
        }

        [Test]
        public void FrameUpdate_CanBeOverridden() {
            var StateMachine = new TestStateMachine();

            var State = new TestGameplayState(StateMachine);

            State.FrameUpdate();

            Assert.IsTrue(State.FrameUpdateCalled);
        }

        [Test]
        public void FrameUpdate_CallsOverrideExactlyOnce() {
            var StateMachine = new TestStateMachine();

            var State = new TestGameplayState(StateMachine);

            State.FrameUpdate();

            Assert.AreEqual(1, State.FrameUpdateCallCount);
        }

        [Test]
        public void PhysicsUpdate_CanBeOverridden() {
            var StateMachine = new TestStateMachine();

            var State = new TestGameplayState(StateMachine);

            State.PhysicsUpdate();

            Assert.IsTrue(State.PhysicsUpdateCalled);
        }

        [Test]
        public void PhysicsUpdate_CallsOverrideExactlyOnce() {
            var StateMachine = new TestStateMachine();

            var State = new TestGameplayState(StateMachine);

            State.PhysicsUpdate();

            Assert.AreEqual(1, State.PhysicsUpdateCallCount);
        }

        [Test]
        public void LateUpdate_CanBeOverridden() {
            var StateMachine = new TestStateMachine();

            var State = new TestGameplayState(StateMachine);

            State.LateUpdate();

            Assert.IsTrue(State.LateUpdateCalled);
        }

        [Test]
        public void LateUpdate_CallsOverrideExactlyOnce() {
            var StateMachine = new TestStateMachine();

            var State = new TestGameplayState(StateMachine);

            State.LateUpdate();

            Assert.AreEqual(1, State.LateUpdateCallCount);
        }

        [Test]
        public void MultipleLifecycleMethods_CanAllBeCalledIndependently() {
            var StateMachine = new TestStateMachine();

            var State = new TestGameplayState(StateMachine);

            State.Awake();
            State.Start();
            State.FrameUpdate();
            State.PhysicsUpdate();
            State.LateUpdate();

            Assert.IsTrue(State.AwakeCalled);
            Assert.IsTrue(State.StartCalled);
            Assert.IsTrue(State.FrameUpdateCalled);
            Assert.IsTrue(State.PhysicsUpdateCalled);
            Assert.IsTrue(State.LateUpdateCalled);
        }

        [Test]
        public void MultipleLifecycleMethods_TrackIndependentCallCounts() {
            var StateMachine = new TestStateMachine();

            var State = new TestGameplayState(StateMachine);

            State.Awake();
            State.Awake();

            State.Start();

            State.FrameUpdate();
            State.FrameUpdate();
            State.FrameUpdate();

            State.PhysicsUpdate();

            State.LateUpdate();
            State.LateUpdate();

            Assert.AreEqual(2, State.AwakeCallCount);
            Assert.AreEqual(1, State.StartCallCount);
            Assert.AreEqual(3, State.FrameUpdateCallCount);
            Assert.AreEqual(1, State.PhysicsUpdateCallCount);
            Assert.AreEqual(2, State.LateUpdateCallCount);
        }
    }
}