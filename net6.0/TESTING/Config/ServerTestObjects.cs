using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.TESTING.Config
{
    /// <summary>
    /// Files, Directories, Messages to send during test, defaults are pre-configured
    /// </summary>
    public class ServerTestObjects
    {
        public string MessageFromServer = "Test Message from server ęąćńéęëě";
        public string MessageExpectedFromClient = "Test Message from client ęąćńéęëě";

        public byte[] BytesFromServer = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 };
        public byte[] BytesExpectedFromServer = new byte[] { 0x05, 0x04, 0x03, 0x02, 0x01 };

        public string FileFromServer = "C:\\testfile.txt";
        public string DirectoryFromServer = "C:\\TEST";

    }
}
