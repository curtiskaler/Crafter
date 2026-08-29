// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Auturge.Quantity;

public class DirectedGraph<T> where T : notnull
{
    // The graph is represented by a dictionary where the key is the node's ID 
    // and the value is a list of adjacent nodes (neighbors).
    protected readonly Dictionary<T, HashSet<T>> _adjacencyList = new();

    // Add a new vertex to the graph
    public void AddVertex(T vertex)
    {
        if (!_adjacencyList.ContainsKey(vertex))
        {
            _adjacencyList[vertex] = [];
            ;
        }
    }

    public bool TryFindBFS(T startVertex, T targetVertex, out IEnumerable<T> path)
    {
        path = FindShortedPathBFS(startVertex, targetVertex);
        return path.Any();
    }

    public IEnumerable<T> FindShortedPathBFS(T startVertex, T targetVertex)
    {
        // Quick validation: Ensure both vertices exist in the graph data structure
        if (!_adjacencyList.ContainsKey(startVertex) || !_adjacencyList.ContainsKey(targetVertex))
        {
            return [];
        }

        // Optimization: Handle edge case where target is the start vertex
        if (EqualityComparer<T>.Default.Equals(startVertex, targetVertex))
        {
            return new List<T> { startVertex };
        }

        var queue = new Queue<T>();
        var visited = new HashSet<T>();

        // Key = Child Node, Value = Parent Node (Tracks how we reached each vertex)
        var parentMap = new Dictionary<T, T>();

        // Initialize search
        visited.Add(startVertex);
        queue.Enqueue(startVertex);

        bool pathFound = false;

        while (queue.Count > 0)
        {
            // current is the "latest" addition:
            T current = queue.Dequeue();

            // Early Termination: Stop looking at extra vertices the moment we hit our target
            if (EqualityComparer<T>.Default.Equals(current, targetVertex))
            {
                pathFound = true;
                break;
            }

            var neighbors = GetNeighbors(current).ToList();
            foreach (T neighbor in neighbors)
            {
                if (visited.Add(neighbor))
                {
                    parentMap[neighbor] = current; // Record parent relationship
                    queue.Enqueue(neighbor);
                }
            }
        }

        // If the queue emptied without hitting the target, no connection exists
        if (!pathFound) return [];

        // Backtrack from Target to Start using the parentMap, then reverse it
        var path = new List<T>();
        T currNode = targetVertex;

        while (!EqualityComparer<T>.Default.Equals(currNode, startVertex))
        {
            path.Add(currNode);
            currNode = parentMap[currNode];
        }

        path.Add(startVertex);

        path.Reverse(); // Turn target->start into start->target
        return path;
    }

    // Add a directed edge from source node to destination node
    public void AddEdge(T source, T destination)
    {
        AddVertex(source);
        AddVertex(destination);
        _adjacencyList[source].Add(destination);
        _adjacencyList[destination].Add(source);
    }

    public IEnumerable<T> GetNeighbors(T vertex) =>
        _adjacencyList.TryGetValue(vertex, out HashSet<T>? neighbors)
            ? neighbors
            : Enumerable.Empty<T>();

    public override string ToString()
    {
        string result = string.Empty;
        for (int index = 0; index < _adjacencyList.Count; index++)
        {
            KeyValuePair<T, HashSet<T>> kvp = _adjacencyList.ElementAt(index);
            if (result == string.Empty)
            {
                result += kvp.Key;
            }

            result += " -> ";
            result += kvp.Value;
        }

        return result;
    }

    public HashSet<T> DFS(T startNode)
    {
        var visited = new HashSet<T>();
        DfsRecursive(startNode, visited);
        return visited;
    }

    private void DfsRecursive(T current, HashSet<T> visited)
    {
        if (!visited.Add(current)) return; // Prevents processing if already visited

        Console.Write(current + " ");

        foreach (T neighbor in GetNeighbors(current))
        {
            DfsRecursive(neighbor, visited);
        }
    }

    // Perform Breadth-First Search starting from a given node
    public HashSet<T> BFS(T startNode)
    {
        // A set is used to keep track of visited nodes to prevent cycles and redundant visits
        var visited = new HashSet<T>();

        // if the start node isn't in the list, then bail.
        if (!_adjacencyList.ContainsKey(startNode))
            return visited;

        // A queue is used to keep track of nodes to visit (FIFO order)
        var queue = new Queue<T>();

        // Initialize the search with the starting vertex
        visited.Add(startNode);
        queue.Enqueue(startNode);

        // Console.WriteLine($"Starting BFS from node {startNode}:");

        while (queue.Count > 0)
        {
            T vertex = queue.Dequeue();

            Console.Write(vertex + " ");

            IEnumerable<T> neighbors = GetNeighbors(vertex);
            foreach (T neighbor in neighbors)
            {
                // if the neighbor hasn't been visited yet, queue it
                if (visited.Add(neighbor)) queue.Enqueue(neighbor);
            }
        }

        return visited;
    }
}
