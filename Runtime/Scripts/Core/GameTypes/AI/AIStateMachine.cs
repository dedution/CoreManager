namespace core.AI
{
    /*
     * Initialization of the state machine
     * 
        protected AIStateMachine<baseAI> AIStateManager { get; set; }
        AIStateManager = new AIStateMachine<baseAI>(this);
        AIStateManager.ChangeState(State_Idle.Instance);
    */

    public class AIStateMachine<T>
    {
        public State<T> currentState
        {
            get;
            private set;
        }

        public T NPC;

        public AIStateMachine(T _npc)
        {
            NPC = _npc;
            currentState = null;
        }

        public void ChangeState(State<T> _newState)
        {
            if (_newState != null)
            {
                if (currentState != null)
                    currentState.ExitState(NPC);

                currentState = _newState;

                currentState.EnterState(NPC);
            }
        }

        public void Update()
        {
            if (currentState != null)
                currentState.UpdateState(NPC);
        }
    }

    public abstract class State<T>
    {
        public abstract void EnterState(T _npc);
        public abstract void ExitState(T _npc);
        public abstract void UpdateState(T _npc);
    }
}
