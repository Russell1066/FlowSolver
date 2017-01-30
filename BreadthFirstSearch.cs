using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowSolver
{
    class BreadthFirstSearch
    {
        class Node
        {
            public int parent;
            public int id;

            public Node(int index = -1)
            {
                parent = -1;
                id = index;
            }
        };

        static public List<int> Search(int start, int end, Func<int, List<int>> getChildren)
        {
            List<int> retv = new List<int>();
            Dictionary<int, Node> knownNodes = new Dictionary<int, Node>();
            List<Node> nodes = new List<Node>();

            if (start == end)
            {
                retv.Add(start);

                return retv;
            }

            knownNodes[start] = new Node() { parent = start, id = start };
            nodes.Add(knownNodes[start]);
            while(nodes.Count > 0)
            {
                var node = nodes[0];
                nodes.RemoveAt(0);

                foreach(var child in getChildren(node.id))
                {
                    Node childNode;
                    if(!knownNodes.ContainsKey(child))
                    {
                        knownNodes[child] = new Node(child);
                    }

                    childNode = knownNodes[child];

                    if (childNode.parent != -1 || childNode.id == node.id)
                    {
                        continue;
                    }

                    childNode.parent = node.id;
                    if(child == end)
                    {
                        return CreatePath(start, retv, knownNodes, childNode);
                    }

                    nodes.Add(childNode);
                }
            }

            return retv;
        }

        private static List<int> CreatePath(int start, List<int> retv, Dictionary<int, Node> knownNodes, Node childNode)
        {
            while (childNode.parent != childNode.id)
            {
                retv.Add(childNode.id);
                childNode = knownNodes[childNode.parent];
            }

            retv.Add(start);

            return retv;
        }
    }
}
