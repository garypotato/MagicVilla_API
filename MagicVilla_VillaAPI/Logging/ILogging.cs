/////////////////////////////
// why interface?
// define the method and properties that the class must implement
// can be used by multiple classes
/////////////////////////////
namespace MagicVilla_VillaAPI.Logging
{
    public interface ILogging
    {
        public void Log(string message, string type);
    }
}
