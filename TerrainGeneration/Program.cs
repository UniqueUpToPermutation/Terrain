using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Platform;
using OpenTK.Graphics.OpenGL;

namespace TerrainGeneration
{
    public class Program
    {
        public const bool bLoadConfig = true;
        public const string ConfigFile = "Config\\Config.xml";

        static void Main(string[] args)
        {
            // Load configuration
            var options = ApplicationOptions.Default;
            if (bLoadConfig)
                options = ApplicationOptions.FromFile(ConfigFile);

            // Create our application and run it
            var window = new RenderWindow();
            window.Run();

            System.Diagnostics.Debug.WriteLine("Closing Program...");
        }
    }
}
