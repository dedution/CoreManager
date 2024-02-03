using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using core.gameplay;
using UnityEngine.Experimental.AI;
using System.Linq;

namespace core.modules
{
    public enum ActorTypes
    {
        System,
        Player,
        Enemy,
        Character,
        Interactable // Pinged by the player
    }

    public class ActorManager : BaseModule
    {
        private List<baseGameActor> m_registeredActors = new List<baseGameActor>();

        // TODO
        // Pooling and handling of game objects
        public override void onInitialize()
        {

        }

        public void RegisterActor(baseGameActor _actor)
        {
            if (!ReferenceEquals(_actor, null))
                m_registeredActors.Add(_actor);
        }

        public void UnregisterActor(baseGameActor _actor)
        {
            if (!ReferenceEquals(_actor, null))
                m_registeredActors.Remove(_actor);
        }

        public baseGameActor[] GetActorsByType(ActorTypes _aType)
        {
            var _actors = m_registeredActors.Where(_actor => _actor.actorType == _aType).ToArray();
            return _actors;
        }
    }
}