using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using core.gameplay;
using System.Linq;

namespace core.modules
{
    public class ActorManager : BaseModule
    {
        private baseGameActor m_playerController;

        public baseGameActor PlayerController
        {
            set { m_playerController = value; }
            get { return m_playerController; }
        }

        private Dictionary<Type, List<baseGameActor>> m_registeredActors = new Dictionary<Type, List<baseGameActor>>();
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
                if(!m_registeredActors.ContainsKey(_actor.GetType().BaseType))
                    m_registeredActors.Add(_actor.GetType().BaseType, new List<baseGameActor>());

                m_registeredActors[_actor.GetType().BaseType].Add(_actor);

                if(_actor.actorUpdatesViaManager)
                    UpdateActors += _actor.onUpdate;
            }
        }

        public void UnregisterActor(baseGameActor _actor)
        {
            if (!ReferenceEquals(_actor, null))
            {
                if(m_registeredActors.ContainsKey(_actor.GetType().BaseType))
                    m_registeredActors[_actor.GetType().BaseType].Remove(_actor);

                if(_actor.actorUpdatesViaManager)
                    UpdateActors -= _actor.onUpdate;
            }
        }

        // Returns actors based on class type and gameobject instance id
        public T[] FindActors<T>(int _actorInstanceID)
        {
            if(m_registeredActors.ContainsKey(typeof(T)) || typeof(T).IsSubclassOf(typeof(T)))
                return m_registeredActors[typeof(T)].Where(_actor => _actor.gameObject.GetInstanceID() == _actorInstanceID).Cast<T>().ToArray();
            else
                return null;
        }

        public T[] FindActorsInCollisionRange<T>(float searchRadius, Vector3 searchPoint,  LayerMask searchLayer)
        {
            Collider[] hitColliders = Physics.OverlapSphere(searchPoint, searchRadius, searchLayer);
            List<T> availableActors = new List<T>();

            for (int i = 0; i < hitColliders.Length; i++)
            {
                T[] _actors = FindActors<T>(hitColliders[i].gameObject.GetInstanceID());

                if(!ReferenceEquals(_actors, null))
                    availableActors.AddRange(_actors);
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