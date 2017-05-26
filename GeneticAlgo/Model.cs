using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Pathfinder;
using PriorityQueue;
using SharpGL;

namespace GeneticAlgo
{
    class MapChromosomeFactory : ChromosomeFactory<Map>
    {

        private City[] cities;
        public int numberOfCities { set; get; }

        public MapChromosomeFactory(City[] cities)
        {
            this.cities = cities;
        }

        public override Map MakeOne()
        {
            Map map = new Map();
            map.SetCityBlueprint(cities);
            map.factory = this;
            return map;
        }

        public override Map MakeRandomOne()
        {
            Map map = new Map();
            map.SetCityBlueprint(cities);
            map.RandomizeConnection(random);
            map.factory = this;
            return map;
        }
    }

    class Map : IChromosome<Map>, IPriorityQueueItem<Map>
    {
        private City[] cities;
        private int[] m_roadConnection;

        public ChromosomeFactory<Map> factory { set; private get; }
        public float roadCost
        {
            get
            {
                float road = 0;

                for (int i = 0; i < roadConnection.Length; i++)
                {
                    if (roadConnection[i] == 0) continue;

                    City cityA, cityB;

                    GetCitiesByConnectionIndex(i, out cityA, out cityB);

                    road += cityA.Distance(cityB);
                }

                return road;
            }
        }

        public int[] roadConnection
        {
            get { return m_roadConnection; }
            set
            {
                m_roadConnection = value;
                for (int i = 0; i < value.Length; i++)
                {
                    SetConnectionValue(i, value[i]);
                }
            }
        }

        public void SetCityBlueprint(City[] cities)
        {
            this.cities = cities;
            roadConnection = new int[MaxConnectionCount(cities.Length - 1)];
        }

        public void RandomizeConnection(Random random)
        {
            for (int i = 0; i < roadConnection.Length; i++)
            {
                SetConnectionValue(i, random.NextDouble() <= 0.5 ? 1 : 0);
            }
        }

        // IChromosome

        public int Age { set; get; }

        public float Fitness
        {
            get
            {
                if (!AllCitiesConnected()) return 0;
                
                return 1000/(float)Math.Pow(roadCost, 2);
            }
        }

        public Map Crossover(Random random, Map partner)
        {
            int middle = random.Next() % roadConnection.Length;
            int[] newChromosomeState1 = new int[roadConnection.Length];

            for (int i = 0; i < roadConnection.Length; i++)
            {
                if (i < middle)
                {
                    newChromosomeState1[i] = roadConnection[i];
                }
                else
                {
                    newChromosomeState1[i] = partner.roadConnection[i];
                }
            }

            Map map = factory.MakeOne();
            map.roadConnection = newChromosomeState1;

            return map;
        }

        public void Mutate(Random random, float mutationRate)
        {
            for (int i = 0; i < roadConnection.Length; i++)
            {
                if(random.NextDouble() < mutationRate)
                {
                    SetConnectionValue(i, roadConnection[i] == 0 ? 1 : 0);
                }
            }
        }

        // Helper

        private void SetConnectionValue(int connectionIndex, int value)
        {
            City cityA;
            City cityB;

            GetCitiesByConnectionIndex(connectionIndex, out cityA, out cityB);

            cityA.ChangeNeighbourhoodStatus(cityB, value);
            cityB.ChangeNeighbourhoodStatus(cityA, value);

            roadConnection[connectionIndex] = value;
        }

        private void GetCitiesByConnectionIndex(int connectionIndex, out City cityA, out City cityB)
        {
            int left, right;
            left = right = -1;
            if (connectionIndex != 2 && connectionIndex != 0)
            {

                int n = 1;
                while (MaxConnectionCount(n) <= connectionIndex) n++;

                left = connectionIndex % MaxConnectionCount(n - 1);
                right = n;
            }
            else
            {
                if (connectionIndex == 0)
                {
                    left = 0; right = 1;
                }
                else if (connectionIndex == 2)
                {
                    left = 1; right = 2;
                }
            }

            cityA = cities[left];
            cityB = cities[right];
        }

        private int MaxConnectionCount(int numberOfCities)
        {
            // a + b(n-1) + c(n-1)(n-2)/2! + ...
            return 1 + 2 * (numberOfCities - 1) + 1 * (numberOfCities - 1) * (numberOfCities - 2) / 2;
        }

        private void ReevaluateNeighbourhood()
        {
            for (int i = 0; i < roadConnection.Length; i++)
            {
                SetConnectionValue(i, roadConnection[i]);
            }
        }

        private bool AllCitiesConnected()
        {
            ReevaluateNeighbourhood();
            for (int i = 1; i < cities.Length; i++)
            {
                if(!CheckPath(cities[0], cities[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckPath(City A, City B)
        {
            return Pathfinder<City>.CheckPath(A, B);
        }

        // IPriorityQueue

        public int incomingIndex { set; get; }
        public int CompareTo(Map map)
        {
            return Fitness.CompareTo(map.Fitness);
        }

        // Drawing

        public void Draw(OpenGL gl, float r = 1f, float g = 0, float b = 0, float a = 1f)
        {
            int ctr = 0;
            foreach (var city in cities)
            {
                gl.Color(0, 1f, 0);
                gl.PointSize(10);
                gl.Begin(OpenGL.GL_POINTS);
                gl.Vertex(city.x, city.y);
                gl.End();
            }

            for (int i = 0; i < roadConnection.Length; i++)
            {
                City A, B;

                GetCitiesByConnectionIndex(i, out A, out B);
                if(roadConnection[i] > 0)
                {
                    ctr++;
                    gl.Color(r, g, b, a);
                    gl.Begin(OpenGL.GL_LINES);
                    gl.Vertex(A.x, A.y);
                    gl.Vertex(B.x, B.y);
                    gl.End();
                }
            }
        }

        // Copy

        public Map Copy()
        {
            Map map = factory.MakeOne();
            map.roadConnection = (int[])this.roadConnection.Clone();
            return map;
        }
    }

    class City : IPathfinderNode<City>
    {
        public float x, y;

        public City(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float Distance(City other)
        {
            return (float)Math.Sqrt(x * x + y * y);
        }

        private List<City> m_neighbour = new List<City>();

        public bool ChangeNeighbourhoodStatus(City ci, int status)
        {
            switch (status)
            {
                case 0:
                    if (m_neighbour.Contains(ci))
                    {
                        m_neighbour.Remove(ci);
                        return true;
                    }
                    break;
                case 1:
                    if (!m_neighbour.Contains(ci))
                    {
                        m_neighbour.Add(ci);
                        return true;
                    }
                    break;
            }

            return false;
        }

        /// IPathFindingNode

        public City parent { set; get; }

        public float gCost { set; get; }
        public float hCost { set; get; }
        public float fCost { get { return gCost + hCost; } }

        public float HeuristicDistance(City other)
        {
            return Distance(other);
        }

        public City[] neighbour { get { return m_neighbour.ToArray(); } }
    }
}
