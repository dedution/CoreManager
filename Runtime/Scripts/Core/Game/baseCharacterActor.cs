using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using core.AI;
using core.modules;
using UnityEngine;
using static core.GameManager;

namespace core.gameplay
{
    // Common logic for enemy and friendly NPCs
    // State machine brain should be defined here
    // Implement active ragdoll support with a separate controller or behavior
    // Path finding should be over time and async

    public abstract class baseCharacterActor : baseGameActor
    {
        public Dictionary<string, State<baseCharacterActor>> m_characterStates;
        protected AIStateMachine<baseCharacterActor> AIStateManager { get; set; }

        protected override void onStart()
        {
            base.onStart();

            AIStateManager = new AIStateMachine<baseCharacterActor>(this);

            // Finds and instantiates the FSM proper states for this AI actor 
            FindAndCreateStates();
        }

        public override void onUpdate()
        {
            base.onUpdate();

            AIStateManager.Update();
        }

        private string GetStatePrefix()
        {
            // Prefix should be able to be a custom string
            return GetType().Name + "_State_";
        }

        private void FindAndCreateStates()
        {
            // Find an automatic way of finding the states and instantiate them
            // Use the character class name as reference
            // Find all the states under a specific namespace
            // State classes need to be formated as follows: 
            // CharName_State_StateName
            // StateName will be the identifier for the state in the dictionary
            
            ActOnModule((AIManager _ref) => {
                m_characterStates = _ref.GetRegisteredAIStates(this);
            });

            // States where loaded via another AI of the same type
            if(m_characterStates.Count > 0)
                return;

            foreach (State<baseCharacterActor> _state in InstantiateStates<State<baseCharacterActor>>())
            {
                // Reformat state class name to simplify it
                string stateName = _state.GetType().Name;
                stateName = stateName.Replace(GetStatePrefix(), "");

                m_characterStates.Add(stateName, _state);
            }

            ActOnModule((AIManager _ref) => {
                _ref.RegisterAIStates(this);
            });

            // Log states
            if(m_characterStates.Count > 0)
                Debug.Log("AI loaded " + m_characterStates.Count + " states");
        }

        private List<Type> FindStatesInNamespace<T>()
        {
            string ns = typeof(T).Namespace;
            Type instanceType = typeof(T);
            List<Type> results = instanceType.Assembly.GetTypes().Where(tt => tt.Namespace == ns &&
                                                                              tt != instanceType).ToList();
            return results;
        }

        private List<T> InstantiateStates<T>()
        {
            List<T> instances = new List<T>();

            foreach (Type t in FindStatesInNamespace<T>())
            {
                if (t.IsSubclassOf(typeof(T)))
                {
                    // Only instantiate states for the current character
                    if(!typeof(T).Name.Contains(GetStatePrefix()))
                        continue;

                    T i = (T)Activator.CreateInstance(t);
                    instances.Add(i);
                }
            }

            return instances;
        }

        protected State<baseCharacterActor> GetStateByName(string _stateName)
        {
            return m_characterStates.ContainsKey(_stateName) ? m_characterStates[_stateName] : null;
        }
    }
}