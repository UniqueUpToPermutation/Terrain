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
        static void Main(string[] args)
        {
            // Create our application and run it
            var window = new RenderWindow();
            window.Run();

            System.Diagnostics.Debug.WriteLine("Closing Program...");
        }
    }
}
