
using UnityEngine;
using static core.GameManager;

namespace core.gameplay
{
    public abstract class baseGameInteraction : baseGameActor
    {
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
    }
}