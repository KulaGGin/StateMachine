using Kulagin.StateMachine.Core;
using NUnit.Framework;

namespace Kulagin.StateMachine.Unity.Tests {
    public class UnityStateTests {
        private class TestStateMachine : StateMachine<TestStateMachine, TestUnityState> {
        }

        private class TestUnityState : UnityState<TestStateMachine, TestUnityState> {
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

            public TestUnityState(TestStateMachine StateMachine) : base(StateMachine) {
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
        
        private TestStateMachine _StateMachine;
        private TestUnityState _State;

        [SetUp]
        public void Setup() {
            _StateMachine = new TestStateMachine();
            _State = new TestUnityState(_StateMachine);
        }

        [Test]
        public void Constructor_StoresStateMachineReference() {
            Assert.AreEqual(_StateMachine, _State.StateMachine);
        }

        [Test]
        public void Awake_CanBeOverridden() {
            _State.Awake();

            Assert.IsTrue(_State.AwakeCalled);
        }

        [Test]
        public void Awake_CallsOverrideExactlyOnce() {
            _State.Awake();

            Assert.AreEqual(1, _State.AwakeCallCount);
        }

        [Test]
        public void Start_CanBeOverridden() {
            _State.Start();

            Assert.IsTrue(_State.StartCalled);
        }

        [Test]
        public void Start_CallsOverrideExactlyOnce() {
            _State.Start();

            Assert.AreEqual(1, _State.StartCallCount);
        }

        [Test]
        public void FrameUpdate_CanBeOverridden() {
            _State.FrameUpdate();

            Assert.IsTrue(_State.FrameUpdateCalled);
        }

        [Test]
        public void FrameUpdate_CallsOverrideExactlyOnce() {
            _State.FrameUpdate();

            Assert.AreEqual(1, _State.FrameUpdateCallCount);
        }

        [Test]
        public void PhysicsUpdate_CanBeOverridden() {
            _State.PhysicsUpdate();

            Assert.IsTrue(_State.PhysicsUpdateCalled);
        }

        [Test]
        public void PhysicsUpdate_CallsOverrideExactlyOnce() {
            _State.PhysicsUpdate();

            Assert.AreEqual(1, _State.PhysicsUpdateCallCount);
        }

        [Test]
        public void LateUpdate_CanBeOverridden() {
            _State.LateUpdate();

            Assert.IsTrue(_State.LateUpdateCalled);
        }

        [Test]
        public void LateUpdate_CallsOverrideExactlyOnce() {
            _State.LateUpdate();

            Assert.AreEqual(1, _State.LateUpdateCallCount);
        }

        [Test]
        public void MultipleLifecycleMethods_CanAllBeCalledIndependently() {
            _State.Awake();
            _State.Start();
            _State.FrameUpdate();
            _State.PhysicsUpdate();
            _State.LateUpdate();

            Assert.IsTrue(_State.AwakeCalled);
            Assert.IsTrue(_State.StartCalled);
            Assert.IsTrue(_State.FrameUpdateCalled);
            Assert.IsTrue(_State.PhysicsUpdateCalled);
            Assert.IsTrue(_State.LateUpdateCalled);
        }

        [Test]
        public void MultipleLifecycleMethods_TrackIndependentCallCounts() {
            _State.Awake();
            _State.Awake();

            _State.Start();

            _State.FrameUpdate();
            _State.FrameUpdate();
            _State.FrameUpdate();

            _State.PhysicsUpdate();

            _State.LateUpdate();
            _State.LateUpdate();

            Assert.AreEqual(2, _State.AwakeCallCount);
            Assert.AreEqual(1, _State.StartCallCount);
            Assert.AreEqual(3, _State.FrameUpdateCallCount);
            Assert.AreEqual(1, _State.PhysicsUpdateCallCount);
            Assert.AreEqual(2, _State.LateUpdateCallCount);
        }
    }
}