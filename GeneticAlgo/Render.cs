using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpGL;

namespace GeneticAlgo
{
    public partial class Render : Form
    {
        public Render()
        {
            InitializeComponent();

            Setup();
        }

        private void openGLControl1_OpenGLDraw(object sender, RenderEventArgs e)
        {
            OpenGL gl = this.openGLControl1.OpenGL;

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();
            gl.Translate(0, -4, -40);

            if (genetaionToggle)
            {
                ga.Generation();
            }

            Map candidate = ga.GetBest();
            if(best == null || best.Fitness < candidate.Fitness)
            {
                best = candidate.Copy();
                Console.Write("New Best at " + ga.GenerationCount + "     \t\t");
                for (int i = 0; i < best.roadConnection.Length; i++)
                {
                    Console.Write(best.roadConnection[i]);
                }
                Console.WriteLine();
            }

            gl.Translate(-5, 0, 0);
            candidate.Draw(gl, 0, 1, 1, 0.3f);

            gl.Translate(20, 0, 0);

            best.Draw(gl);

            this.label1.Text = string.Format("Generation: {0}     Best: {1} {3}    Current Generation Best: {2} {4}       Average: {5}", ga.GenerationCount, best.Fitness, candidate.Fitness, best.roadCost, candidate.roadCost, ga.AverageFitness);
            if (ga.GenerationCount >= 1000000000) genetaionToggle = false;
            gl.Flush();
        }

        MapChromosomeFactory mapFactory;
        GeneticAlgorithm<Map> ga;
        Map best;
        bool genetaionToggle = true;

        private void Setup()
        {
            City[] case1 = (new City[]
            {
                new City(-9, 4),
                new City(1, 2),
                new City(4, 5),
                new City(0, 0),
                new City(3, 10),
                new City(1, -1),
                new City(2, -2),
                new City(4, -3),
            });

            City[] case2 = (new City[]
            {
                new City(8,4),
                new City(4,4),
                new City(0,0),
                new City(12,8),
                new City(8,6)
            });

            mapFactory = new MapChromosomeFactory(case1);

            mapFactory.SetRandomSeed(4);

            ga = new GeneticAlgorithm<Map>(mapFactory, 50, 0.05f);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            genetaionToggle = !genetaionToggle;
        }
    }
}
