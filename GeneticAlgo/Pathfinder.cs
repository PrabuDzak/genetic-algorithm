using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathfinder
{
    class Pathfinder<T> where T : class, IPathfinderNode<T>
    {
        public static bool CheckPath(T startNode, T endNode)
        {
            bool sucess = false;

            List<T> openSet = new List<T>();
            HashSet<T> closedSet = new HashSet<T>();

            openSet.Add(startNode);

            while(openSet.Count > 0)
            {
                T currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if(openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost)
                    {
                        if (openSet[i].hCost < currentNode.hCost)
                            currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if(currentNode == endNode)
                {
                    sucess = true;
                    break;
                }

                foreach (T neighbour in currentNode.neighbour)
                {
                    if (closedSet.Contains(neighbour)) continue;

                    float neighbourGCost = currentNode.gCost + currentNode.HeuristicDistance(neighbour);

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                    else if (neighbourGCost >= neighbour.gCost)
                    {
                        continue;
                    }

                    neighbour.parent = currentNode;
                    neighbour.gCost = neighbourGCost;
                    neighbour.hCost = neighbour.HeuristicDistance(endNode);
                }
            }

            return sucess;
        }
    }

    interface IPathfinderNode<T>
    {
        float gCost { set; get; }
        float hCost { set; get; }
        float fCost { get; }

        T parent { set; get; }
        T[] neighbour { get; }
        float HeuristicDistance(T other);
    }
}
