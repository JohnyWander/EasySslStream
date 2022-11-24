using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace EasySslStream
{
    [Serializable]
    internal class SerializeConfiguration
    {
        public enum Mode
        {
            Save,
            Load
        }
        private bool DEBUG;
        private DynamicConfiguration.DEBUG_MODE debugMode;

        private string? debug_message;
        private string? debug_title;

        private DynamicConfiguration.RaiseMessageDelegate? RaiseMessage;


        public SerializeConfiguration(Mode mode)
        {
           if(mode == Mode.Save)
            {
                Serialize();
            }

           if(mode == Mode.Load)
            {
            //    Deserialize();
            }

        }

        public void Serialize()
        {


            Stream serialized = File.OpenWrite("temp.dat");

            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(serialized, this);

        }

        


    }
}
