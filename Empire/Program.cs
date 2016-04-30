using System;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace Empire
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new View.GameView())
                game.Run();
        }
    }
#endif
}
