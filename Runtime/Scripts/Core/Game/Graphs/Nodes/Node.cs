using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.graphs
{
    public abstract class Node : ScriptableObject
    {
        public enum State
        {
            Running,
            Success,
            Fail
        }

        public State state = State.Running;
        public bool started = false;
        public string guid;
        public Vector2 position;

        public State Update()
        {
            if (!started)
            {
                onStart();
                started = true;
            }

            state = onUpdate();

            if (state == State.Fail || state == State.Success)
            {
                onFinish();
                started = false;
            }

            return state;
        }

        protected abstract void onStart();
        protected abstract State onUpdate();
        protected abstract void onFinish();
    }
}