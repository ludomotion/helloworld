using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace HelloWorld.Misc
{
    public static class CCL
    {
        private static int[] locations = { -1, 0, -1, -1, 0, -1, 1, -1 };
        public static int[] ccl(int[] data, int width, out int largestLabel)
        {
            Debug.Assert(data.Length % width == 0);

            int height = data.Length / width;
            int[] labels = new int[data.Length];
            UnionFind linked = new UnionFind();

            int nextlabel = 1;

            for (int i = 0; i < data.Length; i++)
            {
                if ((data[i]) > 0)
                    continue;
                int x = i % width;
                int y = i / width;

                int[] neighbors = findneighbors(data, labels, width, height, x, y);

                int closest = int.MaxValue;
                for (int j = 0; j < neighbors.Length; j++)
                    if (neighbors[j] > 0)
                        closest = Math.Min(closest, neighbors[j]);
                if (closest != int.MaxValue)
                {
                    labels[i] = closest;
                    for (int j = 0; j < neighbors.Length; j++)
                        if (neighbors[j] > 0)
                            linked.union(neighbors[j], closest);
                }
                else
                {
                    labels[i] = nextlabel;
                    linked.find(nextlabel);
                    nextlabel++;
                }

            }
            
            Dictionary<int, int> labelCount = new Dictionary<int, int>();
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] == 0)
                    continue;
                labels[i] = linked.find(labels[i]);
                if(labelCount.ContainsKey(labels[i])) {
                    labelCount[labels[i]]++;
                } else {
                    labelCount[labels[i]] = 0;
                }
            }

            int count = -1;
            largestLabel = -1;
            foreach (KeyValuePair<int, int> pair in labelCount)
            {
                if (pair.Value > count)
                {
                    count = pair.Value;
                    largestLabel = pair.Key;
                }
            }

            return labels;
        }

        private static int[] findneighbors(int[] data, int[] labels, int width, int height, int x, int y)
        {
            int[] neighbors = new int[locations.Length / 2];
            for (int i = 0; i < locations.Length; i += 2)
                if ((n(data, width, height, x, y, locations[i], locations[i + 1])) == 0)
                    neighbors[i / 2] = n(labels, width, height, x, y, locations[i], locations[i + 1]);
            return neighbors;
        }

        private static int n(int[] data, int width, int height, int x, int y, int n, int m)
        {
            x = x + n;
            y = y + m;
            if (x < 0 || x >= width || y < 0 || y >= height)
                return 1;
            return data[y * width + x];
        }

        public class UnionFind
        {
            private class Node
            {
                public Node parent;
                public Node child;

                public int value;

                public int rank;

                public Node(int v)
                {
                    value = v;
                    rank = 0;
                }
            }

            private List<Node> nodes;
            private Stack<Node> stack;

            public UnionFind()
            {
                this.nodes = new List<Node>();
                this.stack = new Stack<Node>();
            }
            private Node findNode(int a)
            {
                Node na = null;
                if (a < nodes.Count)
                    na = nodes[a];

                if (na == null)
                {
                    Node root = new Node(a);
                    root.child = new Node(a);
                    root.child.parent = root;
                    while (nodes.Count <= a)
                        nodes.Add(null);
                    nodes[a] = root.child;
                    return root;
                }
                return findNode(na);
            }
            public int find(int a)
            {
                return this.findNode(a).value;
            }
            private Node findNode(Node node)
            {
                while (node.parent.child == null)
                {
                    stack.Push(node);
                    node = node.parent;
                }

                Node rootChild = node;

                while (stack.Count > 0)
                {
                    node = stack.Pop();
                    node.parent = rootChild;
                }

                return rootChild.parent;
            }

            public bool isEquiv(int a, int b)
            {
                return findNode(a) == findNode(b);
            }
            public void union(int a, int b)
            {
                if (a == b) return;

                Node na = findNode(a);
                Node nb = findNode(b);

                if (na == nb) return;

                if (na.rank > nb.rank)
                {
                    nb.child.parent = na.child;
                    na.value = b;
                }
                else
                {
                    na.child.parent = nb.child;
                    nb.value = b;

                    if (na.rank == nb.rank)
                    {
                        nb.rank++;
                    }
                }
            }
        }
    }
}
