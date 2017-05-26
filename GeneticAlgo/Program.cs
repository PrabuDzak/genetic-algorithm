using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GeneticAlgo
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Render());
        }
    }
}
