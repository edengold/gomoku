using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace hypercube
{


    public class input : MonoBehaviour
    {
        //singleton pattern
        private static input instance = null;
        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this.gameObject);
            //end singleton

            frontScreen = backScreen = null;

            setupSerialComs();
        }
      
        public int baudRate = 115200;
        public int reconnectionDelay = 500;
        public int maxUnreadMessage = 5;
        public int maxAllowedFailure = 3;
        public bool debug = false;

        const int maxTouchesPerScreen = 9;

        public static touchScreenInputManager frontScreen { get; private set; }  //the front touchscreen
        public static touchScreenInputManager backScreen {get; private set; } //the back touchscreen

        static HashSet<touchScreenTarget> eventTargets = new HashSet<touchScreenTarget>();
        public static void _setTouchScreenTarget(touchScreenTarget t, bool addRemove)
        {
            if (addRemove)
                eventTargets.Add(t);
            else
                eventTargets.Remove(t);
        }

        //use this instead of Start(),  that way we know we have our hardware settings info ready before we begin receiving data
        public static void init(dataFileDict d)
        {
            if (!d)
            {
                Debug.LogError("Input was passed bad hardware dataFileDict!");
                return;
            }

            if (!instance)
                return;

#if HYPERCUBE_INPUT

            if (frontScreen != null)
                frontScreen.setTouchScreenDims(d);

            if (backScreen != null)
                frontScreen.setTouchScreenDims(d);
#endif
        }

#if HYPERCUBE_INPUT

        private bool _useFrontScreen = true;
        public bool useFrontScreen { get { return _useFrontScreen; } set { if (frontScreen != null) frontScreen.serial.enabled = value; _useFrontScreen = value; } }
        private bool _useBackScreen = false;
        public bool useBackScreen { get { return _useBackScreen; } set { if (backScreen != null) backScreen.serial.enabled = value; _useBackScreen = value; } }


        public static void _processTouchScreenEvent(touch t)
        {
            if (t == null)
            {
                Debug.LogWarning("Please report a bug in hypercube input. A null touch event was sent for processing.");
                return;
            }

            if (t.state == touch.activationState.TOUCHDOWN)
            {
                    foreach(touchScreenTarget target in eventTargets)
                        target.onTouchDown(t);
            }
            else if (t.state == touch.activationState.ACTIVE)
            {
                foreach (touchScreenTarget target in eventTargets)
                    target.onTouchMoved(t);
            }
            else if (t.state == touch.activationState.TOUCHUP)
            {
                foreach (touchScreenTarget target in eventTargets)
                    target.onTouchUp(t);
            }               
        }

        void setupSerialComs()
        {
            string frontComName = "";
            string[] names = getPortNames();

            if (names.Length == 0)
            {
                Debug.LogWarning("Can't get input from Volume because no ports were detected! Confirm that Volume is connected via USB.");
                return;
            }

            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].StartsWith("COM") || names[i].Contains("usbmodem"))
                {
                    frontComName = names[i];
                }
            }

            if (frontScreen == null && useFrontScreen)
                frontScreen = new touchScreenInputManager("Front Touch Screen", addSerialPortInput(frontComName), true);

            if (backScreen == null && useBackScreen)
                backScreen = new touchScreenInputManager("Back Touch Screen", addSerialPortInput(frontComName), false);
        }

        void Update()
        {
            if (frontScreen != null && frontScreen.serial.enabled)
                frontScreen.update(debug);
            if (backScreen != null && backScreen.serial.enabled)
                frontScreen.update(debug);
        }

        static string[] getPortNames()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return System.IO.Ports.SerialPort.GetPortNames();
#else
            //this code is from http://answers.unity3d.com/questions/643078/serialportsgetportnames-error.html
            int p = (int)Environment.OSVersion.Platform;
            List<string> serial_ports = new List<string>();

            // Are we on Unix?
            if (p == 4 || p == 128 || p == 6)
            {
                string[] ttys = System.IO.Directory.GetFiles("/dev/", "tty.*");  //In the GetPortNames function, it looks for ports that begin with "/dev/ttyS" or "/dev/ttyUSB" . However, OS X ports begin with "/dev/tty.".
                foreach (string dev in ttys)
                {
                    if (dev.StartsWith("/dev/tty."))
                        serial_ports.Add(dev);
                }
            }

            return serial_ports.ToArray();
#endif
        }
       

        SerialController addSerialPortInput(string comName)
        {
            SerialController sc = gameObject.AddComponent<SerialController>();
            sc.portName = comName;
            sc.baudRate = baudRate;
            sc.reconnectionDelay = reconnectionDelay;
            sc.maxUnreadMessages = maxUnreadMessage;
            sc.maxFailuresAllowed = maxAllowedFailure;
            sc.enabled = true;
            return sc;
        }

        static castMesh[] getCastMeshes()
        {
            List<castMesh> outcams = new List<castMesh>();

            castMesh[] cameras = GameObject.FindObjectsOfType<castMesh>();
            foreach (castMesh ca in cameras)
            {
                outcams.Add(ca);
            }
            return outcams.ToArray();
        }

        public static bool isHardwareReady() //can the touchscreen hardware get/send commands?
        {
            if ( !instance)
                return false;

            if (frontScreen != null)
            {
                if (!frontScreen.serial.enabled)
                    frontScreen.serial.readDataAsString = true; //we must wait for another init:done before we give the go-ahead to get raw data again.
                else if (frontScreen.serial.readDataAsString == false)
                    return true;
            }
           
            return false;
        }

  /*      public static bool sendCommandToHardware(string cmd)
        {
            if (isHardwareReady())
            {
                touchScreenFront.serial.SendSerialMessage(cmd + "\n\r");
                return true;
            }
            else
                Debug.LogWarning("Can't send message to hardware, it is either not yet initialized, disconnected, or malfunctioning.");

            return false;
        }
*/
   
#else //We use HYPERCUBE_INPUT because I have to choose between this odd warning below, or immediately throwing a compile error for new users who happen to have the wrong settings (IO.Ports is not included in .Net 2.0 Subset).  This solution is odd, but much better than immediately failing to compile.
    
        void setupSerialComs()
        {

        }

        public static bool isHardwareReady() //can the touchscreen hardware get/send commands?
        {
            return false;
        }
        public static void sendCommandToHardware(string cmd)
        {
            printWarning();
        }
    
        void Start () 
        {
            printWarning();
            this.enabled = false;
        }

        static void printWarning()
        {
            Debug.LogWarning("TO USE HYPERCUBE INPUT: \n1) Go To - Edit > Project Settings > Player    2) Set Api Compatability Level to '.Net 2.0'    3) Add HYPERCUBE_INPUT to Scripting Define Symbols (separate by semicolon, if there are others)");
        }
#endif
    }

}
