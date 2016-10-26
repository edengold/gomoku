using UnityEngine;
using System.Collections;

//inherit from this class to automatically receive touch events from the hypercube
//alternatively, you can foreach loop on input.frontTouchScreen.touches or input.backTouchScreen.touches

namespace hypercube
{
    public class touchScreenTarget : MonoBehaviour
    {
        protected virtual void OnEnable()   //to use your own OnEnable use:   protected override void OnEnable()  {base.OnEnable()}
        {
            input._setTouchScreenTarget(this, true);
        }
        protected virtual void OnDisable() //to use your own OnDisable use:   protected override void OnDisable()  {base.OnDisable()}
        {
            input._setTouchScreenTarget(this, false);
        }
        protected virtual void OnDestroy() //to use your own OnDestroy use:   protected override void OnDestroy()  {base.OnDestroy()}
        {
            input._setTouchScreenTarget(this, false);
        }

        public virtual void onTouchDown(hypercube.touch touch)
        {
        }

        public virtual void onTouchUp(hypercube.touch touch)
        {
        }

        public virtual void onTouchMoved(hypercube.touch touch)
        {
        }
    }
}
