using System.Collections.Generic; // For List
using System; // For Func (function ref)

using System.Collections;
using UnityEngine;


public struct FrontierElement {
    public int index;
    public float cost;
    public FrontierElement(int _index, float _cost) {
        this.index = _index;
        this.cost = _cost;
    }
}

// TODO :: Find better name : "Mapper" ?
public class Trajectory {
    /* Compute and follow a trajectory */

        // Fields
    private List<Node> nodes = new List<Node>();
    private List<float[]> bounds;
    private Func<Node, Node, float> heuristic;
    private Func<Node, Node, float> distance;
    private Func<Node, Node, bool> areNeighbors;
      // Only required for trajectory computation but saved for later display
    float[] nodeCosts;
    float[] nodeHeuristics;
    int[] precessor;
    public List<FrontierElement> frontier;
    public List<Node> path;

        // Constructors
    public Trajectory(
        List<float[]> _bounds,
        Func<Node, Node, float> _heuristic,
        Func<Node, Node, float> _distance,
        Func<Node, Node, bool> _areNeighbors) {
        this.bounds = _bounds;
        this.heuristic = _heuristic;
        this.distance = _distance;
        this.areNeighbors = _areNeighbors;
    }

        // Getters / Setters
    public List<Node> getNodes() { return this.nodes; }

        // Methods
    public void addSamples(int numSamples) {
        for (int i = 0; i < numSamples; i++) {
            this.nodes.Add(Node.sample(this.bounds));
        }
    }

    public void computeAllNeighbors() {
        // Compute neighborhood between each pair of nodes [O(n**2) complexity] using a given criterium
        // The pairs are defined by distinct indices (Index1, Index2) such that [0 <= Index1 < Index2 < maxIndex] holds
        for (int node1Index = 0; node1Index < (this.nodes.Count - 1); node1Index++) { // -1 to ignore last index (need pair of distinct nodes)
            Node node1 = this.nodes[node1Index];
            for (int node2Index = node1Index+1; node2Index < this.nodes.Count; node2Index++) {
                Node node2 = this.nodes[node2Index];
                    // Compute neighborhoodness of <node2> w.r.t. <node1> using provided criterium
                if (areNeighbors(node1, node2)) { node1.addNeighborIndex(node2Index); }
                    // Non-holonomic neighborhood criteria may not be associative
                    // TODO :: Check from criteria instance if holonomic
                if (areNeighbors(node2, node1)) { node2.addNeighborIndex(node1Index); }
            }
        }
    }

    public void computeNeighbor(int nodeIndex) {
        // Compute neighborhood of a node using a given criterium
        Node node = this.nodes[nodeIndex];
        for (int i = 0; i < this.nodes.Count; i++) {
            if (i == nodeIndex) { continue; } // A node cannot be neighbor with itself
            if (areNeighbors(node, this.nodes[i])) { node.addNeighborIndex(i); }
        }
    }

    public void addNode(Node node) { addNode(node, false); }
    public void addNode(Node node, bool addNeighbors) {
        int nodeIndex = this.nodes.Count;
        this.nodes.Add(node);
        if (!addNeighbors) { return; }
        for (int neighborIndex = 0; neighborIndex < nodeIndex; neighborIndex++) {
            Node neighbor = this.nodes[neighborIndex];
            if (areNeighbors(node, neighbor)) { node.addNeighborIndex(neighborIndex); }
            if (areNeighbors(neighbor, node)) { neighbor.addNeighborIndex(nodeIndex); }
        }
    }

    public void resetTrajectoryVariables(int startNodeIndex, int endNodeIndex) {
        nodeCosts = new float[this.nodes.Count];
        nodeHeuristics = new float[this.nodes.Count];
        precessor = new int[this.nodes.Count];
        for (int i = 0; i < this.nodes.Count; i++) {
            nodeHeuristics[i] = this.heuristic(this.nodes[i], this.nodes[endNodeIndex]);
            precessor[i] = -1;
            nodeCosts[i] = float.PositiveInfinity;
        }
        nodeCosts[startNodeIndex] = 0; // Correct previous values
        frontier = new List<FrontierElement>();
        path = new List<Node>();;
    }

    public List<FrontierElement> explorationStep(List<FrontierElement> frontier, int endNodeIndex, bool onlineNeighborComputation) {
            // Extract lowest-distance element of frontier
        int frEltToRemove = -1; // Remember index of frontier element to remove
        int nodeIndex = -1; // Index of the node to explore next
        float nodeDist = float.PositiveInfinity;
        for (int i = (frontier.Count - 1); i >= 0; i--) { // Start at the end to be able to delete elements and continue iterating
            FrontierElement elt = frontier[i];
                // Delete no longer relevant frontier elements (because we found a shorter path)
            if (elt.cost >= nodeCosts[endNodeIndex]) {
                frontier.RemoveAt(i);
            }
            else {
                if (elt.cost < nodeDist) {
                    nodeIndex = elt.index;
                    nodeDist = elt.cost;
                    frEltToRemove = i;
                }
            }
        }
        if (nodeIndex == -1) { return frontier; } // We started looking for a new element, but all elts in frontier were too far
        frontier.RemoveAt(frEltToRemove);
        if (onlineNeighborComputation) { computeNeighbor(nodeIndex); } // When optimizing, only compute relevant neighborhood
        Node element = this.nodes[nodeIndex];

            // For each neighbor, assess if we should add them
        foreach (int neighborIndex in element.getNeighborIndices()) {
            float neighborNewCost = nodeCosts[nodeIndex] + this.distance(this.nodes[nodeIndex], this.nodes[neighborIndex]);
                // If relevant (first time or lower cost than previously found), we add them to the frontier
            if (neighborNewCost < nodeCosts[neighborIndex]) {
                precessor[neighborIndex] = nodeIndex;
                nodeCosts[neighborIndex] = neighborNewCost;
                bool alreadyInFrontier = false;
                for (int elt_index = 0; elt_index < frontier.Count; elt_index++) {
                    FrontierElement elt = frontier[elt_index];
                    if (elt.index == neighborIndex) {
                        alreadyInFrontier = true;
                        elt.cost = nodeCosts[neighborIndex] + nodeHeuristics[neighborIndex];
                        break;
                    }
                }
                if (!alreadyInFrontier) {
                    float _cost = nodeCosts[neighborIndex] + nodeHeuristics[neighborIndex];
                    frontier.Add(new FrontierElement(neighborIndex, _cost));
                }
            }
        }
        return frontier;
    }

    public List<Node> computeTrajectory(int startNodeIndex, int endNodeIndex) {
        return computeTrajectory(startNodeIndex, endNodeIndex, false);
    }
    public List<Node> computeTrajectory(int startNodeIndex, int endNodeIndex, bool onlineNeighborComputation) {
        // Compute a trajectory from <startNode> to <endNode>, as defined by their indices, from one node to the next considering local neighborhood
        // TODO :: chek strat and end indices are valid (> 0 & < maxIndex)
        resetTrajectoryVariables(startNodeIndex, endNodeIndex); // Reset heuristics, costs and precessor for new trajectory computation
            // Define frontier
        FrontierElement startElement = new FrontierElement(startNodeIndex, 0f);
        frontier.Add(startElement); // TODO :: Simplify by nesting

            // Perform seach while there are elements in <frontier>
        while (frontier.Count > 0) {
            frontier = this.explorationStep(frontier, endNodeIndex, onlineNeighborComputation);
        }
        extractPath(startNodeIndex, endNodeIndex);
        return path;
    }

    public void extractPath(int startNodeIndex, int endNodeIndex) {
        path = new List<Node>();
        int _nodeIndex = endNodeIndex;
        while (_nodeIndex != startNodeIndex && _nodeIndex != -1) {
            path.Insert(0, this.nodes[_nodeIndex]);
            _nodeIndex = precessor[_nodeIndex];
        }
        if (path.Count <= 1) { path = new List<Node>(); } // Reset path if none was found
    }

        // Class methods
    public override string ToString() {
        string toPrint = "[Trajectory] :: " + this.nodes.Count + " nodes" + '\n';
        foreach (Node node in this.nodes) {
            toPrint += "\t" + node.ToString() + "\n";
        }
        return toPrint;
    }
}
