namespace EasySslStream.Misc
{
    /// <summary>
    /// Miscellaneous methods
    /// </summary>
    public class Misc
    {

        public void TestRaiseMessage()
        {

            DynamicConfiguration.RaiseMessage?.Invoke("Test Message", "test title");
        }


    }
}
