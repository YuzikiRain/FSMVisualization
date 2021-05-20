using System;
using System.Collections.Generic;
using UnityEngine;

namespace BordlessFramework
{
    public class FSM
    {
        public IState PreviousState { get; protected set; }
        public IState CurrentState { get; protected set; }
        public IState NextState { get; protected set; }
        public readonly Dictionary<string, IState> States = new Dictionary<string, IState>();

        public TState AddState<TState>() where TState : IState, new()
        {
            string stateName = typeof(TState).Name;
            var state = new TState();
            state.Init(this);
            if (States.Count == 0)
            {
                PreviousState = CurrentState;
                CurrentState = state;
                NextState = state;
            }

            States[stateName] = state;

            return state;
        }

        public State AddState(Type type)
        {
            var state = Activator.CreateInstance(type) as State;
            state.Init(this);
            if (States.Count == 0)
            {
                PreviousState = CurrentState;
                CurrentState = state;
                NextState = state;
            }

            States[type.Name] = state;

            return state;
        }

        public TState RemoveState<TState>() where TState : class, IState
        {
            var name = typeof(TState).Name;
            IState state;
            if (States.TryGetValue(name, out state))
            {
                States.Remove(name);
                return state as TState;
            }
            else
            {
                return null;
            }
        }

        public State RemoveState(string stateName)
        {
            IState state;
            if (States.TryGetValue(stateName, out state))
            {
                States.Remove(stateName);
                return state as State;
            }
            else
            {
                return null;
            }
        }

        public TState GetState<TState>() where TState : class, IState
        {
            States.TryGetValue(typeof(TState).Name, out IState state);
            return state as TState;
        }

        public State GetState(string stateName)
        {
            IState state = null;
            States.TryGetValue(stateName, out state);
            return state as State;
        }

        public void ChangeState<T>()
        {
            string stateName = typeof(T).Name;
            ChangeState(stateName);
        }

        public void ChangeState(string stateName)
        {
            if (States.TryGetValue(stateName, out _))
            {
                var nextState = States[stateName];
                ChangeState(nextState);
            }
            else
            {
                Debug.LogError($"there is no {stateName} State, or you forget to add {stateName} State to FSM");
            }
        }

        public void ChangeState(IState nextState)
        {
            NextState = nextState;
        }

        protected virtual void UpdateStateChange()
        {
            if (NextState != null && CurrentState != null)
            {
                CurrentState.OnExitState();
                PreviousState = CurrentState;
                CurrentState = NextState;
                NextState = null;
                CurrentState.OnEnterState();
            }
        }

        public virtual void Update(float deltaTime)
        {
            CurrentState?.Reason();
            UpdateStateChange();
            CurrentState?.OnUpdateState(deltaTime);
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            CurrentState?.OnFixedUpdateState(fixedDeltaTime);
        }
    }
}