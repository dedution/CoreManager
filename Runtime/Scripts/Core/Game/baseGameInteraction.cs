
using UnityEngine;
using static core.GameManager;

namespace core.gameplay
{
    public abstract class baseGameInteraction : baseGameActor
    {
        [Header("Interaction Parameters")]
        public bool m_isInteractable = true;
        public Transform m_customInteractionTarget;

        public virtual void onInteract()
        {
            
        }

        public Vector3 GetInteractionPosition()
        {
            if(m_customInteractionTarget != null)
                return m_customInteractionTarget.position;
            else
                return transform.position;
        }

        public virtual void onHighlight(bool state)
        {

        }

        public virtual void onEnterRange(bool state)
        {
            
        }
    }
}