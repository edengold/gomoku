using UnityEngine;
using System.Collections;

//this class manages a constant stream of byte data input.
//it will split the data up when the delimiter sequence is found, and then call 'processData' on those chunks of data.

//this class is a base class for each com port device on the Volume to inherit from  ie frontTouchscreen, backTouchcreen, and magicLeap
namespace hypercube
{

    public class streamedInputManager
    {

        private System.Int32 bufferSize = 0;
        private System.Int32 itr; //the buffer iterator
        private byte[] buffer = null;

        private ushort dItr = 0;  //the delimiter iterator
        private byte[] delimiter = null;

#if HYPERCUBE_INPUT
        public readonly SerialController serial;

        public streamedInputManager(SerialController _serial, byte[] _delimiter, int _bufferSize)
        {
            serial = _serial;

            itr = 0;
            bufferSize = _bufferSize;
            buffer = new byte[bufferSize];
            delimiter = _delimiter;
        }
#endif
        public void addData(byte[] bytes)
        {

            if (bytes.Length == 0)
                return;

            for (int i = 0; i < bytes.Length; i++)
            {
                if (itr == bufferSize)
                    itr = 0;

                buffer[itr] = bytes[i];

                if (buffer[itr] == delimiter[dItr])
                {
                    dItr++;
                    if (dItr == delimiter.Length)
                    {
                        //we found a chunk of data, cut out the delimiter and send it to the delegate for processing.
                        byte[] outBytes = new byte[itr - dItr + 1];
                        System.Buffer.BlockCopy(buffer, 0, outBytes, 0, itr - dItr + 1);
                        
                        itr = -1; //-1 so that the itr++ below will process it correctly to 0...
                        dItr = 0;
                        try //just for safety.  we can't afford to have our iterators screwed up by bugs or mishaps in the higher level event processing code.
                        {
                            processData(outBytes);
                        }
                        catch { }
                        
                    }
                }
                else
                    dItr = 0; //we haven't found our delimiter

                itr++;
            }
        }

        public virtual void update(bool debug)
        {

        }

        protected virtual void processData(byte[] dataChunk)
        {

        }
 

    }

}