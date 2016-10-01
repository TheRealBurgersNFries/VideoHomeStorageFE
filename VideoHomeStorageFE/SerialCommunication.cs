using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoHomeStorage.FE
{
    class SerialCommunication
    {
        private SerialPort ArduinoCommunication;

        public SerialCommunication(string _portName)
        {
            try
            {
                ArduinoCommunication = new SerialPort(_portName);
            } catch (Exception e) {
                Debug.WriteLine(e.Message);
            }
        }

        public void Play()
        {
            ArduinoCommunication.WriteLine("");
        }
        public void Record()
        {
            ArduinoCommunication.WriteLine("");
        }

        public void Stop()
        {
            ArduinoCommunication.WriteLine("");
        }

        public void Eject()
        {
            ArduinoCommunication.WriteLine("");
        }
    }
}
