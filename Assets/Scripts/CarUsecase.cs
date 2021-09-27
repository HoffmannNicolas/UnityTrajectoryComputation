using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarUsecase {

    public List<float[]> bounds = new List<float[]>();
    public float maxDist = 1.0f;
    public int numNodes = 300;

    public CarUsecase() {
        // TODO :: define bounds when declaring it
        float[] b1 = new float[2] {0f, 10f};
        float[] b2 = new float[2] {0f, 10f};
        float[] b3 = new float[2] {0f, 2 * Mathf.PI};
        bounds.Add(b1);
        bounds.Add(b2);
        bounds.Add(b3);
    }

    public bool areNeighbors(Node node1, Node node2) {
        // Assess if node2 is neighbor of node1 by enforcing the (localized) kinematic constraints of a car
        return (distance(node1, node2) < maxDist); // TODO :: Properly implement
    }

    public float heuristic(Node node1, Node node2) {
        // L2 distance between the nodes
        return distance(node1, node2);
    }

    public float distance(Node node1, Node node2) {
        // Positionnal distance between <node1> and <node2> (ignoring the angular difference)
        float[] coord1 = node1.getCoordinates();
        float[] coord2 = node2.getCoordinates();
        float dx = coord1[0] - coord2[0];
        float dy = coord1[1] - coord2[1];
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    public void distanceTest() {
        // Prints a bunch of distance computations
        Node origin = new Node(new float[3]{0, 0, 0});
        for (int i = 0; i < 10; i++) {
            Node testNode = Node.sample(this.bounds);
            float dist = distance(origin, testNode);
            Debug.Log(
              "===== Distance Test =====\n"
              + origin + "\n"
              + testNode + "\n"
              + "dist :: " + dist
            );
        }
    }
}
