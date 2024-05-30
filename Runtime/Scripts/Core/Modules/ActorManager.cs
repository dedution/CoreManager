using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using core.gameplay;
using UnityEngine.Experimental.AI;
using System.Linq;

namespace core.modules
{
    // public enum ActorTypes
    // {
    //     System,
    //     Player,
    //     Enemy,
    //     Character,
    //     Interactable // Pinged by the player
    // }

    public class ActorManager : BaseModule
    {
        private Dictionary<int, baseGameActor> m_registeredActors = new Dictionary<int, baseGameActor>();
        private delegate void ActorUpdater();
        private ActorUpdater UpdateActors;

        // TODO
        // Pooling and handling of game objects such as particles
        // Needs to be created a Pool class to be reused across other modules and game logic

        public override void onInitialize()
        {

        }

        public void RegisterActor(baseGameActor _actor)
        {
            if (!ReferenceEquals(_actor, null))
            {
                m_registeredActors.Add(_actor.gameObject.GetInstanceID(), _actor);
                
                if(_actor.actorUpdatesViaManager)
                    UpdateActors += _actor.onUpdate;
            }
        }

        public void UnregisterActor(baseGameActor _actor)
        {
            if (!ReferenceEquals(_actor, null))
            {
                m_registeredActors.Remove(_actor.gameObject.GetInstanceID());

                if(_actor.actorUpdatesViaManager)
                    UpdateActors -= _actor.onUpdate;
            }
        }

        // Inefficient way of getting specific types - DEPRECATED
        // public baseGameActor[] GetActorsByType(ActorTypes _aType)
        // {
        //     var _actors = m_registeredActors.Where(_actor => _actor.actorType == _aType).ToArray();
        //     return _actors;
        // }

        public baseGameActor FindActorByInstanceID(GameObject _gameobject)
        {
            if(m_registeredActors.ContainsKey(_gameobject.GetInstanceID()))
                return m_registeredActors[_gameobject.GetInstanceID()];
            else
                return null;
        }

        public baseGameActor FindActorByInstanceID(int _IDx)
        {
            if(m_registeredActors.ContainsKey(_IDx))
                return m_registeredActors[_IDx];
            else
                return null;
        }
        
        public T[] FindActorsInCollisionRange<T>(float searchRadius, Vector3 searchPoint,  LayerMask searchLayer)
        {
            Collider[] hitColliders = Physics.OverlapSphere(searchPoint, searchRadius, searchLayer);
            List<T> availableActors = new List<T>();

            foreach (Collider collider in hitColliders)
            {
                baseGameActor _actor = FindActorByInstanceID(collider.gameObject.GetInstanceID());
                
                if (!ReferenceEquals(_actor, null) && _actor.GetType().IsSubclassOf(typeof(T)))
                {
                    availableActors.Add((T)(object)_actor);
                }
            }

            return availableActors.ToArray();
        }

        public override void UpdateModule()
        {
            if (!ReferenceEquals(UpdateActors, null))
                UpdateActors();
        }
    }
}