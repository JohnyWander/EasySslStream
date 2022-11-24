using EasySslStream;

namespace Debug_mode_test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            EasySslStream.Misc.Misc misc = new EasySslStream.Misc.Misc();




            EasySslStream.DynamicConfiguration.EnableDebugMode(DynamicConfiguration.DEBUG_MODE.Console);
            misc.TestRaiseMessage();
            EasySslStream.DynamicConfiguration.DisableDebugMode();

            Console.ReadKey();


            EasySslStream.DynamicConfiguration.EnableDebugMode(DynamicConfiguration.DEBUG_MODE.MessageBox);
            misc.TestRaiseMessage();
            EasySslStream.DynamicConfiguration.DisableDebugMode();

            Console.ReadKey();



            EasySslStream.DynamicConfiguration.EnableDebugMode(DynamicConfiguration.DEBUG_MODE.LocalVariable);
            EasySslStream.DynamicConfiguration.DebugLocalVariableEvent += void () =>
            {
                Console.WriteLine($"local variable debug event says: msg title{DynamicConfiguration.debug_title} msg {DynamicConfiguration.debug_message}");


            };

            misc.TestRaiseMessage();



            EasySslStream.DynamicConfiguration.Save();

        }
    }
}