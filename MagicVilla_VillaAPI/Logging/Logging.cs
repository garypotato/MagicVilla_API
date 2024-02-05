namespace MagicVilla_VillaAPI.Logging
{
    public class Logging : ILogging
    {
        public void Log(string message, string type)
        {
            if (type == "error")
            {
                Console.WriteLine("EROOR - " + message);
            } else
            {
                Console.WriteLine("INFO - " + message);
            }

        }
    }
}
