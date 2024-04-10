using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using core.AI;
using core.modules;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;
using static core.GameManager;

namespace core.gameplay
{
    // Common logic for enemy and friendly NPCs
    // State machine brain should be defined here
    // Implement active ragdoll support with a separate controller or behavior
    // Separate the ragdoll control into its own component
    // Path finding should be over time and async
    // Implement a dialog system with callbacks for each reply and easy implementation
    // The dialog system should also support conversations with other characters
    // Do analysis on implementing a dialog tree system on a grid
    // Implement a pathway system so characters can follow looping and closed paths

    public abstract class baseCharacterActor : baseGameActor
    {
        // AI Common components
        private NavMeshAgent characterAgent;
        private Animator characterAnimator;
        private CharacterController characterController;

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

            // Most of the character logic will be inside separate states
            AIStateManager.Update();
        }

        private void RequestPathFinding()
        {

        }

        private void CalculateLookAt(Transform target)
        {

        }

        private void HandleMovement()
        {
            // TODO
            // Add arguments to control what kind of movement this character should do
            // Some characters stand still in the same place and just turn to face the player
            // Other characters follow pathways and others even walk around talking with other characters
        }

        private string GetStatePrefix()
        {
            // Prefix should be able to be a custom string
            return GetType().Name + "_State_";
        }

        private void FindAndCreateStates()
        {
            // State classes need to be formated as follows: 
            // CharName_State_StateName
            
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
        }

        private List<Type> FindStatesInNamespace<T>()
        {
            string ns = typeof(T).Namespace;
            Type instanceType = typeof(T);
            
            // Find types in the same assembly as self  
            List<Type> results = GetType().Assembly.GetTypes().Where(tt => tt.Namespace == ns &&
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
                    if(!t.Name.Contains(GetStatePrefix()))
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