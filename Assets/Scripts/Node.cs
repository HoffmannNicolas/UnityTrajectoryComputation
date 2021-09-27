
using System.Collections.Generic; // For List<> // TODO :: Remove when useless


public class Node {
    /* Encapsulate data related nodes (contains the neighbors for example) */

        // Fields
    // TODO :: replace variable-sized list with fixed size one :: float[] nodeCosts = new float[this.nodes.Count];
    private float[] coordinates;
    private List<int> neighborIndices = new List<int>();

        // Constructors
    public Node(float[] _coordinates) {
        this.coordinates = _coordinates;
    }
    public Node(float[] _coordinates, List<int> _neighborIndices) {
        this.coordinates = _coordinates;
        this.neighborIndices = _neighborIndices;
    }

        // Getters / Setters
    public float[] getCoordinates() { return this.coordinates; }
    public List<int> getNeighborIndices() { return this.neighborIndices; }

        // Methods
    public void addNeighborIndex(int neighborIndex) { this.neighborIndices.Add(neighborIndex); }
    public void addNeighbors(List<int> _neighborIndices) {
        foreach (int neighborIndex in _neighborIndices) {
            addNeighborIndex(neighborIndex);
        }
    }

        // Class methods
    public static Node sample(List<float[]> bounds) {
        // Uniformely sample a nodes within <bounds>
        // TODO :: Check upperbound > lowerBound
        // TODO :: Check each bound has size 2
        int dim = bounds.Count;
        float[] sampleCoordinates = new float[dim];
        for (int dim_index = 0; dim_index < dim; dim_index++) {
            float lowerBound = bounds[dim_index][0];
            float upperBound = bounds[dim_index][1];
            sampleCoordinates[dim_index] = UnityEngine.Random.Range(lowerBound, upperBound);
        }
        return new Node(sampleCoordinates);
    }
    public override string ToString() { // One-liner to print nice with trajectory ToString()
        string toPrint = "[Node] :: " + this.neighborIndices.Count + " neighbors :: [";
        bool firstCoordinate = true;
        foreach (float coordinate in this.coordinates) {
            if (firstCoordinate)
                firstCoordinate = false;
            else
                toPrint += ", ";
            toPrint += coordinate;
        }
        toPrint += "]";
        return toPrint;
    }
}
