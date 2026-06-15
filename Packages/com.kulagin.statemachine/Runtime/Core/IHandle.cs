namespace Kulagin.StateMachine.Core {
    public interface IHandle<TEvent> {
        bool Handle(TEvent Event);   // true = handled, stop bubbling
    }
}