namespace DataBridge.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Pipeline.LoadAndExecute(args[0]);
        }
    }
}