using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgo
{
    class GeneticAlgorithm<T> where T : IChromosome<T>, PriorityQueue.IPriorityQueueItem<T>
    {
        private Random random;

        private T[] population;
        private List<T> newGeneration;

        public PriorityQueue.PriorityQueue<T> priorityQueue
        {
            get
            {
                return new PriorityQueue.PriorityQueue<T>(population);
            }
        }

        public T[] Population
        {
            get
            {
                return population;
            }
        }
        public float SumFitness
        {
            get
            {
                float sum = 0;
                for (int i = 0; i < population.Length; i++)
                {
                    sum += population[i].Fitness;
                }
                return sum;
            }
        }
        public float AverageFitness
        {
            get
            {
                return SumFitness / population.Length;
            }
        }
        public int GenerationCount { private set; get; }
        public float MutationRate { set; get; }

        public GeneticAlgorithm(ChromosomeFactory<T> factory, int populationCount, float mutationRate, int randomSeed = 4000)
        {
            this.random = new Random(randomSeed);
            this.MutationRate = mutationRate;
            this.population = new T[populationCount];
            for (int i = 0; i < populationCount; i++)
            {
                this.population[i] = factory.MakeRandomOne();
            }
            GenerationCount = 1 ;
            this.newGeneration = new List<T>();
        }
        
        public void Generation()
        {
            newGeneration.Clear();

            ParentSelection();
            Crossover();
            Mutation();

            population = newGeneration.ToArray();

            GenerationCount++;
        }

        public T GetBest()
        {
            return priorityQueue.Pop();
        }

        private void ParentSelection()
        {
            for(int i = 0; i < population.Length; i++)
            {
                float rand = (float)random.NextDouble() * SumFitness;

                int index = 0;
                while(rand >= 0)
                {
                    rand -= population[index].Fitness;
                    index++;
                }
                index--;
                if (!newGeneration.Contains(population[index]))
                {
                    newGeneration.Add(population[index]);
                }
            }
        }

        private void Crossover()
        {
            List<T> children = new List<T>();
            int partnerIndex = 1;
            while(newGeneration.Count + children.Count < population.Length)
            {
                for (int i = 0; i < newGeneration.Count; i++)
                {
                    T child = newGeneration[i].Crossover(random, newGeneration[(i + partnerIndex) % newGeneration.Count]);
                    children.Add(child);

                    if(newGeneration.Count + children.Count >= population.Length)
                    {
                        break;
                    }
                }
                partnerIndex++;
            }

            foreach (var c in children)
            {
                newGeneration.Add(c);
            }
        }

        private void Mutation()
        {
            foreach (var chromosome in newGeneration)
            {
                chromosome.Mutate(random, MutationRate);
            }
        }
    }

    abstract class ChromosomeFactory<T> where T : IChromosome<T>
    {
        protected Random random = new Random(4000);

        public abstract T MakeOne();
        public abstract T MakeRandomOne();
        public void SetRandomSeed(int seed)
        {
            random = new Random(seed);
        }
    }

    interface IChromosome<T> where T : IChromosome<T>
    {
        ChromosomeFactory<T> factory { set; }

        int Age { set; get; }
        float Fitness { get; }
        T Crossover(Random random, T partner);
        void Mutate(Random random, float mutationRate);
    }
}
