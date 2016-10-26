#if HYPERCUBE_INPUT
/**
 * Author: Daniel Wilches
 */

using UnityEngine;
using System.Collections;
using System.Threading;

/**
 * This class allows a Unity program to continually check for messages from a
 * serial device.
 * 
 * It creates a Thread that communicates with the serial port and continually
 * polls the messages on the wire.
 * That Thread puts all the messages inside a Queue, and this SerialController
 * class polls that queue by menas of invoking SerialThread.GetSerialMessage().
 * 
 * The serial device must send its messages separated by a newline character.
 * Neither the SerialController nor the SerialThread perform any validation
 * on the integrity of the message. It's up to the one that makes sense of the
 * data.
 */
public class SerialController : MonoBehaviour
{
    [Tooltip("Port name with which the SerialPort object will be created.")]
    public string portName = "COM4";

    [Tooltip("Baud rate that the serial device is using to transmit data.")]
    public int baudRate = 115200;

    [Tooltip("After an error in the serial communication, or an unsuccessful " +
             "connect, how many milliseconds we should wait.")]
    public int reconnectionDelay = 1000;

    [Tooltip("Maximum number of unread data messages in the queue. " +
             "New messages will be discarded.")]
    public int maxUnreadMessages = 1;

    [Tooltip("Maximum number of failed connections before disabling component. ")]
    public int maxFailuresAllowed = 7;

    private bool _readDataAsString = true;
    public bool readDataAsString
    {
        get { return _readDataAsString; }
        set
        {
            _readDataAsString = value;
            if (serialThread != null)
                serialThread.readDataAsString = _readDataAsString;
        }
    }

    int failures = 0;

    // Constants used to mark the start and end of a connection. There is no
    // way you can generate clashing messages from your serial device, as I
    // compare the references of these strings, no their contents. So if you
    // send these same strings from the serial device, upon reconstruction they
    // will have different reference ids.
    public const string SERIAL_DEVICE_CONNECTED = "__Connected__";
    public const string SERIAL_DEVICE_DISCONNECTED = "__Disconnected__";


    // Internal reference to the Thread and the object that runs in it.
    private Thread thread;
    private SerialThread serialThread;

    //force this component to always start disabled, so that the settings can be set before it starts
    void Awake()
    {
        this.enabled = false;
    }


    // ------------------------------------------------------------------------
    // Invoked whenever the SerialController gameobject is activated.
    // It creates a new thread that tries to connect to the serial device
    // and start reading from it.
    // ------------------------------------------------------------------------
    void OnEnable()
    {
        serialThread = new SerialThread(portName, baudRate, reconnectionDelay, maxUnreadMessages);
        serialThread.readDataAsString = readDataAsString;

        thread = new Thread(new ThreadStart(serialThread.RunForever));
        thread.Start();
    }

    // ------------------------------------------------------------------------
    // Invoked whenever the SerialController gameobject is deactivated.
    // It stops and destroys the thread that was reading from the serial device.
    // ------------------------------------------------------------------------
    void OnDisable()
    {
        disconnect();
    }

    void OnDestroy()
    {
        disconnect();
    }

    void disconnect()
    {
        // The serialThread reference should never be null at this point,
        // unless an Exception happened in the OnEnable(), in which case I've
        // no idea what face Unity will make.
        if (serialThread != null)
        {
            serialThread.RequestStop();
            serialThread = null;
        }

        // This reference shouldn't be null at this point anyway.
        if (thread != null)
        {
            thread.Join();
            thread = null;
        }
    }

    public string ReadSerialMessage()
    {
        if (serialThread == null)
            return null;

        string data = serialThread.ReadSerialMessage(); // Read the next message from the queue

        if (ReferenceEquals(data, SerialController.SERIAL_DEVICE_CONNECTED))
        {
            Debug.Log("Connection established to " + portName);
            failures = 0;
            return null; 
        }
        else if (ReferenceEquals(data, SerialController.SERIAL_DEVICE_DISCONNECTED))
        {
            failures++;
            if (maxFailuresAllowed > 0 && failures >= maxFailuresAllowed) //shut ourselves down
                enabled = false;
            Debug.LogWarning("Connection attempt failed or disconnection detected on " + portName + ". Attempting reconnection");
            return null;
        }
    
        return data;
    }


    public void SendSerialMessage(string message)
    {
        serialThread.SendSerialMessage(message);
    }
}
#endif

