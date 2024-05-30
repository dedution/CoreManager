
using UnityEngine;
using static core.GameManager;

namespace core.gameplay
{
    public class baseGameInteraction : baseGameActor
    {
        [Header("Interaction Parameters")]
        public bool m_isInteractable = true;
        public Transform m_customInteractionTarget;

        protected override void onStart()
        {
            base.onStart();
        }

        public override void onUpdate()
        {
            base.onUpdate();
        }

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
    }
}