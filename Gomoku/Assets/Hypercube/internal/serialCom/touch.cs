using UnityEngine;
using System.Collections;


//this class exposes the touch data from Volume to a developer in a way that will be consistent across different models of Volume

//TODO save original local touch coord.

namespace hypercube 
{

    //touchInterface is an internal class, used by the touchScreenManager to communicate with touches.
    public class touchInterface
    {        
        public bool active = false;
        public System.UInt16 _id = System.UInt16.MaxValue;
        public Vector2 normalizedPos;
        public Vector2 physicalPos;

        public int rawTouchScreenX;
        public int rawTouchScreenY;

        public float getDistance(touchInterface i)
        {
            return Vector2.Distance(normalizedPos, i.normalizedPos);
        }
        public float getPhysicalDistance(touchInterface i)
        {
            return Vector2.Distance(physicalPos, i.physicalPos);
        }
    }

    //Note that resolution dependent dims are not exposed.
    //This is important because different devices will host different resolutions and all users of this API should create device independent software.
    //Un-needed data has been abstracted away for maximum compatibility among all types of Volume hardware.
    public class touch
    {
        public readonly bool frontScreen;
        public System.UInt16 id { get; private set; }

        public enum activationState
        {
            TOUCHDOWN = 1,
            ACTIVE = 0,
            TOUCHUP = -1,  //touch will be marked as 'destroyed' next frame
            DESTROYED = -2  //error out on any use.
        }
       public activationState state { get; private set; }

        private float _posX; public float posX { get { if (activeCheck()) return _posX; return 0f; } } //0-1 normalized position of this touch
        private float _posY; public float posY { get { if (activeCheck()) return _posY; return 0f; } } //0-1 normalized position of this touch

        private float _diffX; public float diffX { get { if (activeCheck()) return _diffX; return 0f; } } //normalized relative movement this frame inside 0-1 
        private float _diffY; public float diffY { get { if (activeCheck()) return _diffY; return 0f; } } //normalized relative movement this frame inside 0-1

        private float _distX; public float distX { get { if (activeCheck()) return _distX; return 0f; } } //the physical distance that the touch traveled in Centimeters
        private float _distY; public float distY { get { if (activeCheck()) return _distY; return 0f; } } //the physical distance that the touch traveled in Centimeters

        public touch(bool _frontScreen)
        {
            frontScreen = _frontScreen;
            _posX = _posY = physicalPos.x = physicalPos.y = _diffX = _diffY = _distX = _distY = 0;
            touchScreenX = touchScreenY = 0;
            state = activationState.DESTROYED;
        }
        public Vector3 getWorldPos(hypercubeCamera c)
        {
            return c.transform.TransformPoint(getLocalPos());
        }
        public Vector3 getLocalPos() //get the local coordinate relative to the hypercubeCamera
        {
            if (!activeCheck())
                return Vector3.zero;

            if (frontScreen)
                return new Vector3(_posX - .5f, _posY - .5f, -.5f);
            else
                return new Vector3((1f - _posX) + .5f, _posY - .5f, .5f);
        }

        //how much time since touchDown
        public float touchDownTime { get; private set; }
        public float age { get { if (state == activationState.DESTROYED) return 0f; return Time.timeSinceLevelLoad - touchDownTime; } }

        private Vector2 physicalPos;

        private int touchScreenX; //these are the raw coordinates from the touchscreen.  They are not used in this class but stored here for convenience use with calibration tools.
        private int touchScreenY;


        public float getPhysicalDistanceTo(touch t) 
        { 
            touchInterface i = null;
            t._getInterface(ref i);
            return Vector2.Distance(i.physicalPos, physicalPos);
        }

        public void _getInterface(ref touchInterface i)
        {
            i.normalizedPos.x = _posX;
            i.normalizedPos.y = _posY;
         
            i.physicalPos = physicalPos;
            i.rawTouchScreenX = touchScreenX;
            i.rawTouchScreenY = touchScreenY;

            i._id = id;
            if (state < activationState.ACTIVE)
                i.active = false;
            else
                i.active = true;

        }   
        public void _interface(touchInterface i)
        {
            if (!i.active) //deactivate this touch.
            {
                deactivate();
                return;
            }

            if (state < activationState.ACTIVE)
            {
                state = activationState.TOUCHDOWN;
                touchDownTime = Time.timeSinceLevelLoad;

                _diffX = _diffY = _distX = _distY = 0f; //this is a touch down: we don't want to compare this to zeroed out values and get crazy values on the first frame active.
            }
            else
            {
                state = activationState.ACTIVE;
                _diffX = i.normalizedPos.x - posX;
                _diffY = i.normalizedPos.y - posY;
                _distX = i.physicalPos.x - physicalPos.x;
                _distY = i.physicalPos.y - physicalPos.y;
            }

            _posX = i.normalizedPos.x;
            _posY = i.normalizedPos.y;
            physicalPos = i.physicalPos;

            touchScreenX = i.rawTouchScreenX;
            touchScreenY = i.rawTouchScreenY;

            id = i._id; //faster and easier to just set it all the time than check if this is a new touch or not.
        }
        void deactivate()
        {
            if (state == activationState.DESTROYED)
                return;

            if (state == activationState.TOUCHDOWN) //rare for a touch to last only 1 hardware frame, but possible.  Force it down.
                state = activationState.ACTIVE; //from here it will drop down to touchUp

            state--;
             _diffX = _diffY = _distX = _distY = 0f;

             if (state == activationState.DESTROYED)
             {
                 touchDownTime = _posX = _posY = physicalPos.x = physicalPos.y = 0f;
                 touchScreenX = touchScreenY = 0;
             }
        }

        bool activeCheck()
        {
            if (state == activationState.DESTROYED)
            {
                Debug.LogError("Code attempted to use a destroyed touch! Test for this first by checking touch.state.\nDon't hold pointers to touches once they are 'Destroyed'.");
                return false;
            }
            return true;
        }

        //if you need to map this touch to a gui area or other coordinate sub-area, use this to help you.
        public Vector2 mapToRange(Vector2 leftTop, Vector2 rightBottom)
        {
            return mapToRange(leftTop.y, rightBottom.x, rightBottom.y, leftTop.x);
        }
        public Vector2 mapToRange(float top, float right, float bottom, float left)
        {
            float outX;
            float outY;
            touchScreenInputManager.mapToRange(_posX, _posY, top, right, bottom, left, out outX, out outY);
            return new Vector2(outX, outY);
        }


    }


}
