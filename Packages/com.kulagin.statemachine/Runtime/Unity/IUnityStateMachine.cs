using Kulagin.StateMachine.Core;

namespace Kulagin.StateMachine.Unity {
    public interface IUnityStateMachine : IStateMachine {
        void Awake();
        void Start();
        void FrameUpdate();
        void PhysicsUpdate();
        void LateUpdate();
    }
}