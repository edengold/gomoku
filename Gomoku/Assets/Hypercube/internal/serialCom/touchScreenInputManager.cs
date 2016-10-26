using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace hypercube
{

public class touchScreenInputManager  : streamedInputManager
{
    //current touches, updated every frame
    public touch[] touches { get; private set; } 
    public uint touchCount { get; private set; }

    //external interface..
    public Vector2 averagePos { get; private set; } //The 0-1 normalized average position of all touches
    public Vector2 averageDiff { get; private set; } //The normalized distance the touch moved 0-1
    public Vector2 averageDist { get; private set; } //The distance the touch moved, in Centimeters


    public float twist { get; private set; }
    public float pinch { get; private set; }//0-1
    public float touchSize { get; private set; } //0-1
    public float touchSizeCm { get; private set; } //the ave distance between the farthest 2 touches in 1 axis, in centimeters

    public float firmwareVersion { get; private set; }
    

#if HYPERCUBE_INPUT
    private float lastSize = 0f; //0-1
    float lastTouchAngle = 0f; //used to calculate twist, this is the angle between the two farthest touches in the last frame
    public Vector3 getAverageTouchWorldPos(hypercubeCamera c) { return c.transform.TransformPoint(getAverageTouchLocalPos()); }
    public Vector3 getAverageTouchLocalPos()
    {
        if (isFront)
            return new Vector3(averagePos.x + .5f, averagePos.y + .5f, -.5f);
        else
            return new Vector3((1f - averagePos.x) + .5f, averagePos.y + .5f, .5f);
    }

  public readonly string deviceName;

    //is this the front touch screen?
    public readonly bool isFront;

    //we don't know the architecture of software that will use this code. So I chose a pool instead of a factory pattern here to avoid affecting garbage collection in any way 
    //at the expense of a some kb of memory (touchPoolSize * sizeof(touch)).  The problem with using a pool, is that a pointer to a touch might be held, meanwhile it gets recycled by touchScreenInputManager 
    //to represent a new touch.  To avoid this, I implemented the 'destroyed' state on touches, and when any access occurs on it while 'destroyed', the touch throws an error.  
    //The other part of trying to keep touches difficult to misuse is to make the poolSize large enough so that some time will pass before recycling, 
    //giving any straggling pointers to touches a chance to throw those errors and let the dev know that their design needs change.
    //so in short: DON'T HOLD POINTERS TO THE TOUCHES FOR MORE THAN THE UPDATE
    const int touchPoolSize = 128; 
    touch[] touchPool = new touch[touchPoolSize];
    int touchPoolItr = 1; //index 0 is forever an inactive touch.

    touchInterface[] interfaces = new touchInterface[touchPoolSize]; //these are used to update the touches internally, allowing us to expose all data but not any controls to anything outside of this class or the touch class.

    const int maxTouches = 12;
    //Dictionary<int, int> touchIdMap = new Dictionary<int, int>(); //dictionary instead of array prevents duplicate entries
    int[] touchIdMap = new int[maxTouches];  //this maps the touchId coming from hardware to its arrayPosition in the touchPool;
    System.UInt16 currentTouchID = 0; //strictly for external convenience

 
    //these variables map the raw touches into coherent positional data that is consistent across devices.
    float screenResX = 800f; //these are not public, as the touchscreen res can vary from device to device.  We abstract this for the dev as 0-1.
    float screenResY = 450f;
    float projectionWidth = 20f; //the physical size of the projection, in centimeters
    float projectionHeight = 12f;
    float projectionDepth = 20f;
    float touchScreenWidth = 20f; // physical size of the touchscreen, in centimeters
    float touchScreenHeight = 12f;
    float topLimit = 0f;
    float bottomLimit = 1f;
    float leftLimit = 0f;
    float rightLimit = 1f;

    static readonly byte[] emptyByte = new byte[] { 0 };

    touchInterface mainTouchA = null; //instead of dynamically searching for touches with each update of data, we try to reuse the same ones across frames so that pinch and twist are less jittery
    touchInterface mainTouchB = null;

    public touchScreenInputManager(string _deviceName, SerialController _serial, bool _isFrontTouchScreen) : base(_serial, new byte[]{255,255}, 1024)
    {
        firmwareVersion = -9999f;
        touches = new touch[0];
        touchCount = 0;
        pinch = 1f;
        twist = 0f;
        deviceName = _deviceName;
        isFront = _isFrontTouchScreen;

        for (int i = 0; i < touchPool.Length; i++ )
        {
            touchPool[i] = new touch(isFront);
        }
        for (int i = 0; i < interfaces.Length; i++)
        {
            interfaces[i] = new touchInterface();
        }
        for (int i = 0; i < touchIdMap.Length; i++)
        {
            touchIdMap[i] = 0;
        }
    }



    public void setTouchScreenDims(dataFileDict d)
    {
        if (d == null)
            return;

        if (!d.hasKey("touchScreenResX") ||
            !d.hasKey("touchScreenResY") ||
            !d.hasKey("projectionCentimeterWidth") ||
            !d.hasKey("projectionCentimeterHeight") ||
            !d.hasKey("projectionCentimeterDepth") ||
            !d.hasKey("touchScreenMapTop") ||
            !d.hasKey("touchScreenMapBottom") ||
            !d.hasKey("touchScreenMapLeft") ||
            !d.hasKey("touchScreenMapRight")  //this one is necessary to keep the hypercube aspect ratio
            )
            Debug.LogWarning("Volume config file lacks touch screen hardware specs!"); //these must be manually entered, so we should warn if they are missing.

        screenResX = d.getValueAsFloat("touchScreenResX", screenResX);
        screenResY = d.getValueAsFloat("touchScreenResY", screenResY);
        projectionWidth = d.getValueAsFloat("projectionCentimeterWidth", projectionWidth);
        projectionHeight = d.getValueAsFloat("projectionCentimeterHeight", projectionHeight);
        projectionDepth = d.getValueAsFloat("projectionCentimeterDepth", projectionDepth);

        topLimit = d.getValueAsFloat("touchScreenMapTop", topLimit); //use averages.
        bottomLimit = d.getValueAsFloat("touchScreenMapBottom", bottomLimit);
        leftLimit = d.getValueAsFloat("touchScreenMapLeft", leftLimit);
        rightLimit = d.getValueAsFloat("touchScreenMapRight", rightLimit);

        touchScreenWidth = projectionWidth * (1f/(rightLimit - leftLimit));
        touchScreenHeight = projectionHeight * (1f/(topLimit - bottomLimit));
    }



    public override void update(bool debug)
    {
        
        string data = serial.ReadSerialMessage();
        while (data != null)
        {

            if (debug)
                Debug.Log(deviceName +": "+ data);

            if (serial.readDataAsString)
            {
                if (data.StartsWith("firmwareVersion:"))
                {
                    string[] toks = data.Split(':');
                    firmwareVersion = dataFileDict.stringToFloat(toks[1], firmwareVersion);
                }

                if (data == "init:done" || data.Contains("init:done"))
                {
                    serial.readDataAsString = false; //start capturing data
                    Debug.Log(deviceName + " is ready and initialized.");
                }

                return; //still initializing
            }

            addData(System.Text.Encoding.Unicode.GetBytes(data));
     
            data = serial.ReadSerialMessage();
        }

        postProcessData();
    }


    protected override void processData(byte[] dataChunk)
    {
        /*  the expected data here is ..
         * 1 byte = total touches
         * 
         * 1 byte = touch id
         * 2 bytes = touch x
         * 2 bytes = touch y
         * 
         *  1 byte = touch id for next touch  (optional)
         *  ... etc
         *  
         * */

        if (dataChunk == emptyByte)
            return;

        uint totalTouches = dataChunk[0];

        if (dataChunk.Length != (totalTouches * 5) + 1)  //unexpected chunk length! Assume it is corrupted, and dump it.
            return;

        //assume no one is touched.
        for (int i = 0; i < touchPoolSize; i++)
        {
            interfaces[i].active = false;
        }     


        float x = 0;
        float y = 0;
        uint iX = 0;
        uint iY = 0;

        for (int i = 1; i < dataChunk.Length; i= i + 5) //start at 1 and jump 5 each time.
        {
            int id = dataChunk[i];
            iX = System.BitConverter.ToUInt16(dataChunk, i + 1);
            iY = System.BitConverter.ToUInt16(dataChunk, i + 3);
            x = (float)iX;
            y = (float)iY;

            //sometimes the hardware sends us funky data.
            //if the stats are funky, throw it out.
            if (id == 0 || id >= maxTouches)
                continue; 
            if (x < 0 || x > screenResX)
                continue;
            if (y < 0 || y > screenResY)
                continue;
        
            //is this a new touch?  If so, assign it to a new item in the pool, and update our iterators.
            if (touchPool[touchIdMap[id]].state < touch.activationState.ACTIVE ) //a new touch!  Point it to a new element in our touchPool  (we know it is new because the place where the itr is pointing to is deactivated. Hence, it must have gone through at least 1 frame where no touch info was received for it.)
            {
                //we can not allow either key duplicates or value duplicates in the map.
                //key duplicates are already handled because an array index is by definition unique.
                //however here, we have to make sure another id is not already pointing to our desired touchPoolItr.
                for (int k = 0; k < maxTouches; k++ )
                {
                    if (touchIdMap[k] == touchPoolItr && k != id)  
                        touchIdMap[k] = 0; //point it to our always defunct element.  Without this, we can cause our touchCount to be incorrect.
                }

                touchIdMap[id] = touchPoolItr; //point the id to the current iterator 

                currentTouchID++;
                interfaces[touchIdMap[id]]._id = currentTouchID; //this id, is purely for external convenience and does not affect our functions here.

                touchPoolItr++;
                if (touchPoolItr >= touchPoolSize)
                    touchPoolItr = 1;  //we rely on element 0 always being inactive so we can use it to fix any duplicates.
            }
       
            interfaces[touchIdMap[id]].active = true;

            interfaces[touchIdMap[id]].rawTouchScreenX = (int)iX;
            interfaces[touchIdMap[id]].rawTouchScreenY = (int)iY;

            //set the normalized x/y to the touchscreen limits
            mapToRange(x/screenResX, y/screenResY, topLimit, rightLimit, bottomLimit, leftLimit, 
                out interfaces[touchIdMap[id]].normalizedPos.x, out interfaces[touchIdMap[id]].normalizedPos.y); 

            interfaces[touchIdMap[id]].physicalPos.x = (x / screenResX) * touchScreenWidth;
            interfaces[touchIdMap[id]].physicalPos.y = (y / screenResY) * touchScreenHeight;
        }

    }

    void postProcessData()
    {
        touchCount = 0;
        for (int k = 0; k < maxTouches; k++)
        {
            if (interfaces[touchIdMap[k]].active)
                touchCount++;
        }

        float averageNormalizedX = 0f;
        float averageNormalizedY = 0f;
        float averageDiffX = 0f;
        float averageDiffY = 0f;
        float averageDistX = 0f;
        float averageDistY = 0f;

        touchInterface highX = null;
        touchInterface lowX = null;
        touchInterface highY = null;
        touchInterface lowY = null;

        touches = new touch[touchCount];

        //main touches are a way for us to stabilize the twist and scale outputs by not hopping around different touches, instead trying to calculate the values from the same touches if possible
        bool updateMainTouches = false;
        if (touchCount > 1 && (mainTouchA == null || mainTouchB == null || mainTouchA.active == false || mainTouchB.active == false))
            updateMainTouches = true; 

        int t = 0;
        for (int i = 0; i < touchPoolSize; i++)
        {
            touchPool[i]._interface(interfaces[i]); //update and apply our new info to each touch.     
            if (interfaces[i].active)
            {
                touches[t] = touchPool[i];//these are the touches that can be queried from hypercube.input.front.touches              
                t++;

                averageNormalizedX += touchPool[i].posX;
                averageNormalizedY += touchPool[i].posY;
                averageDiffX += touchPool[i].diffX;
                averageDiffY += touchPool[i].diffY;
                averageDistX += touchPool[i].distX;
                averageDistY += touchPool[i].distY;

                if (updateMainTouches)
                {
                    if (highX == null || interfaces[i].physicalPos.x > highX.physicalPos.x)
                        highX = interfaces[i];
                    if (lowX == null || interfaces[i].physicalPos.x < lowX.physicalPos.x)
                        lowX = interfaces[i];
                    if (highY == null || interfaces[i].physicalPos.y > highY.physicalPos.y)
                        highY = interfaces[i];
                    if (lowY == null || interfaces[i].physicalPos.y < lowY.physicalPos.y)
                        lowY = interfaces[i];
                }

            }
        }
        
        if (touchCount < 2)
        {
            touchSize = touchSizeCm = 0f;
            pinch = 1f;
            lastTouchAngle = twist = 0f;
            mainTouchA = mainTouchB = null;
            if (touchCount == 0)
                averagePos = averageDiff = averageDist = Vector2.zero;
            else //1 touch only.
            {
                averagePos = new Vector2(touches[0].posX, touches[0].posY);
                averageDiff = new Vector2(touches[0].diffX, touches[0].diffY);
                averageDist = new Vector2(touches[0].distX, touches[0].distY);
            }          
        }
        else
        {
            averagePos = new Vector2(averageNormalizedX / (float)touchCount, averageNormalizedX / (float)touchCount);
            averageDiff = new Vector2(averageDiffX / (float)touchCount, averageDiffY / (float)touchCount);
            averageDist = new Vector2(averageDistX / (float)touchCount, averageDistY / (float)touchCount);

            if (averageDiff.x < -.3f || averageDiff.x > .3f || averageDiff.y < -.3f || averageDiff.y > .3) //this is too fast to be a real movement, its probably an artifact.
            {
                averageDiff = averageDist = Vector2.zero;
            }

            //pinch and twist
            if (updateMainTouches)
            {
                if ((highX.physicalPos.x - lowX.physicalPos.x) > (highY.physicalPos.y - lowY.physicalPos.y))//use the bigger of the two differences, and then use the true distance
                {
                    mainTouchA = highX; mainTouchB = lowX;
                }
                else
                {
                    mainTouchA = highY; mainTouchB = lowY;
                }
            }

            touchSizeCm = mainTouchA.getPhysicalDistance(mainTouchB);
            touchSize = mainTouchA.getDistance(mainTouchB);

            float angle = angleBetweenPoints(mainTouchA.normalizedPos, mainTouchB.normalizedPos);            


            //validate everything coming out of here... ignore crazy values that may come from hardware artifacts.
            if (lastTouchAngle == 0)
                twist = 0;
            else
                twist = angle - lastTouchAngle;

            if (twist < -20f || twist > 20f) //more than 20 degrees in 1 frame?!.. most likely junk. toss it.
                twist = angle = 0f;
            lastTouchAngle = angle;

            if (lastSize == 0f)
                pinch = 1f;
            else
                pinch = touchSizeCm / lastSize;

            if (pinch < .7f || pinch > 1.3f) //the chances that this is junk data coming from the touchscreen are very high. dump it.
                pinch = 1f;
            
             lastSize = touchSizeCm;
        }

        //finally, send off the events to touchScreenTargets.
        for (int i = 0; i < touchPoolSize; i++)
        {
            if (touchPool[i].state != touch.activationState.DESTROYED)
            input._processTouchScreenEvent(touchPool[i]);
        }
    }
#endif

    static float angleBetweenPoints(Vector2 v1, Vector2 v2)
    {      
        return Mathf.Atan2(v1.x - v2.x, v1.y - v2.y) * Mathf.Rad2Deg;
    }

     public static void mapToRange(float x, float y, float top, float right, float bottom, float left, out float outX, out float outY)
     {
         outX = map(x, left, right, 0f, 1.0f);
         outY = map(y, bottom, top, 0f, 1.0f);
     }
     static float map(float s, float a1, float a2, float b1, float b2)
     {
         return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
     }

}



}
