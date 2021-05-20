namespace BordlessFramework
{
    public abstract class State : IState
    {
        public string Name { get; private set; }
        public FSM FSM { get; private set; }

        public State()
        {
            Name = GetType().Name;
        }

        public virtual void Init(FSM fsm)
        {
            FSM = fsm;
        }

        protected virtual void ChangeState<T>() where T : class, IState
        {
            FSM.ChangeState<T>();
        }

        protected virtual void ChangeState(string stateName)
        {
            FSM.ChangeState(stateName);
        }

        public abstract void Reason();
        public virtual void OnUpdateState(float deltaTime) { }
        public virtual void OnFixedUpdateState(float fixedDeltaTime) { }
        public virtual void OnEnterState() { }
        public virtual void OnExitState() { }
    }

    public interface IState
    {
        void Init(FSM fsm);
        void Reason();
        void OnUpdateState(float deltaTime);
        void OnFixedUpdateState(float fixedDeltaTime);
        void OnEnterState();
        void OnExitState();
        string Name { get; }
    }
}