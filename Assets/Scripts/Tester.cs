using System; // For Action (Func that returns void)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tester : MonoBehaviour {

    public bool stepByStep = false;
    public bool speedOptimized = true;
    public bool autoMode = false;
    public bool visualize = true;

    public GameObject spherePrefab;
    public GameObject cylinderPrefab;

    public TextMeshProUGUI StateDisplay;

    private MasterGameObjectPooler masterObjectPooler;
    private CarUsecase usecase;
    private Trajectory trajectory;

    private void initializeTrajectory() {

            // Instanciate empty trajectory
        this.trajectory = new Trajectory(this.usecase.bounds, this.usecase.heuristic, this.usecase.distance, this.usecase.areNeighbors);
        // Debug.Log("Empty instanciation\n" + trajectory);

            // Sample n nodes in <bounds>
        trajectory.addSamples(this.usecase.numNodes);
        // Debug.Log("Add samples\n" + trajectory);

            // Compute all neighbors
        if (!speedOptimized) {
            trajectory.computeAllNeighbors();
            // Debug.Log("Compute neighbors\n" + trajectory);
        }

            // Add start & end nodes
        trajectory.addNode(new Node(new float[3]{10, 10, 0}), !speedOptimized);
        trajectory.addNode(new Node(new float[3]{0, 0, 0}), !speedOptimized);
        // Debug.Log("Start / End nodes\n" + trajectory);
    }

    private void computePath() {
            // Compute <path>
        int nodeNumber = trajectory.getNodes().Count;
        int startNodeIndex = nodeNumber - 1;
        int endNodeIndex = nodeNumber - 2;
        trajectory.computeTrajectory(startNodeIndex, endNodeIndex, speedOptimized);
    }

    void updateVisualization() {
            // Free previous drawing s ressources
        this.masterObjectPooler.freeAll();
        GC.Collect(); // GIMME MY RAM BACK !! >_<'
            // Draw anew
        if (this.visualize) {
            draw(drawSphere_build, drawLine_build);
        }
    }

    void UpdateParams() {
          // Number of nodes
        if (Input.GetKeyDown(KeyCode.I)) this.usecase.numNodes = (int) (this.usecase.numNodes / 1.1f);
        if (Input.GetKeyDown(KeyCode.O)) this.usecase.numNodes = (int) (this.usecase.numNodes * 1.1f);
          // Maximum neighbor distance
        if (Input.GetKeyDown(KeyCode.K)) this.usecase.maxDist /= 1.1f;
        if (Input.GetKeyDown(KeyCode.L)) this.usecase.maxDist *= 1.1f;
          // Auto mode
        if (Input.GetKeyDown(KeyCode.Return)) this.autoMode = ! this.autoMode;
          // Step-By-Step mode
        if (Input.GetKeyDown(KeyCode.B)) this.stepByStep = ! this.stepByStep;
          // Speed optimization
        if (Input.GetKeyDown(KeyCode.S)) this.speedOptimized = ! this.speedOptimized;
          // Hiding
        if (Input.GetKeyDown(KeyCode.H)) {
            this.visualize = ! this.visualize;
            updateVisualization();
        }
          // Update display
        string todisplay = "Nodes : " + this.usecase.numNodes + '\n' + "Max dist : " + this.usecase.maxDist + '\n';
        if (this.autoMode) todisplay += "[ON] AutoMode\n";
        else todisplay += "[OFF] AutoMode\n";
        if (this.stepByStep) todisplay += "[ON] StepByStep\n";
        else todisplay += "[OFF] StepByStep\n";
        if (this.speedOptimized) todisplay += "[ON] SpeedOptim\n";
        else todisplay += "[OFF] SpeedOptim\n";
        if (this.visualize) todisplay += "[ON] Visualize\n";
        else todisplay += "[OFF] Visualize\n";

        this.StateDisplay.text = todisplay;
    }

    void Start() {
            // Define our use-case (bounds, distance measure, heuristic, neighborhood criteria)
        this.usecase = new CarUsecase();
        // this.usecase.distanceTest();

            // For efficient drawing
        this.masterObjectPooler = new MasterGameObjectPooler("MasterObjectPooler");

        initializeTrajectory();
        computePath();
        updateVisualization();
    }

    void Update() {
        UpdateParams();
        if (Input.GetKeyDown(KeyCode.Space) || this.autoMode) {
            if (!stepByStep) {
                initializeTrajectory();
                computePath();
            }
            else {
                int nodeNumber = trajectory.getNodes().Count;
                int startNodeIndex = nodeNumber - 1;
                int endNodeIndex = nodeNumber - 2;
                    // Reset if needed
                if (trajectory.frontier.Count == 0) {
                    initializeTrajectory();
                    trajectory.resetTrajectoryVariables(startNodeIndex, endNodeIndex);
                    FrontierElement startElement = new FrontierElement(startNodeIndex, 0f);
                    trajectory.frontier.Add(startElement); // TODO :: Simplify by nesting
                }
                else {
                    trajectory.frontier = trajectory.explorationStep(trajectory.frontier, endNodeIndex, speedOptimized);
                    if (trajectory.frontier.Count == 0)
                        trajectory.extractPath(startNodeIndex, endNodeIndex);
                }
            }
            updateVisualization();
        }
        if (Input.GetKey("escape")) Application.Quit();

    }

    private void drawSphere_gizmo(Vector3 position, Color color, float radius) {
        Gizmos.color = color;
        Gizmos.DrawSphere(position, radius);
    }
    private void drawSphere_build(Vector3 position, Color color, float radius) {
        radius *= 2;
        GameObject sphere = this.masterObjectPooler.get(spherePrefab);
        sphere.transform.position = position;
        sphere.GetComponent<Renderer>().material.SetColor("_Color", color);
        sphere.transform.localScale = new Vector3(radius, radius, radius);
    }
    private void drawLine_gizmo(Vector3 p1, Vector3 p2, Color color) {
        Gizmos.color = color;
        Gizmos.DrawLine(p1, p2);
    }
    private void drawLine_build(Vector3 p1, Vector3 p2, Color color) {
        if (color == Color.green)
            drawLine_build(p1, p2, color, 0.05f);
        else
            drawLine_build(p1, p2, color, 0.02f);
    }
    private void drawLine_build(Vector3 p1, Vector3 p2, Color color, float radius) {
        GameObject cylinder = this.masterObjectPooler.get(cylinderPrefab);
        cylinder.transform.position = (p1 + p2) / 2.0f;
        cylinder.GetComponent<Renderer>().material.color = color;
        float dx = p1[0] - p2[0];
        float dy = p1[2] - p2[2];
        float dist = Mathf.Sqrt(dx * dx + dy * dy);
        cylinder.transform.LookAt(p2);
        cylinder.transform.Rotate(90.0f, 0.0f, 0.0f, Space.Self);
        cylinder.transform.localScale = new Vector3(radius, dist/2, radius);
    }

    void OnDrawGizmos() {
        draw(drawSphere_gizmo, drawLine_gizmo);
    }

    void draw(Action<Vector3, Color, float> drawSphere, Action<Vector3, Vector3, Color> drawLine) {
            // Do not try to draw <trajectory> if it is not defined yet
        if (this.trajectory == null) { return; }

        int NodeNumber = trajectory.getNodes().Count;
        List<Node> nodes = this.trajectory.getNodes();
        for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++) {
            Node node = nodes[nodeIndex];
            float[] nodeCoord = node.getCoordinates();
            Vector3 nodePosition = new Vector3(nodeCoord[0], 0.0f, nodeCoord[1]);
                // Start and End nodes are highlighted
            if (nodeIndex == NodeNumber - 2) { drawSphere(nodePosition, Color.green, 0.2f); }
            else if (nodeIndex == NodeNumber - 1) { drawSphere(nodePosition, Color.cyan, 0.2f); }
            else if (trajectory.frontier.Exists(x => x.index == nodeIndex)) {
                Color frontierColor = new Color(0.5f, 0.5f, 1f);
                drawSphere(nodePosition, frontierColor, 0.13f);
            }
            else { drawSphere(nodePosition, Color.yellow, 0.1f); }
                // Draw neighborhoods
            foreach (int neighborIndex in node.getNeighborIndices()) {
                float[] neighborCoord = nodes[neighborIndex].getCoordinates();
                Vector3 neighborPosition = new Vector3(neighborCoord[0], 0.0f, neighborCoord[1]);
                drawLine(nodePosition, neighborPosition, Color.white);
            }
        }
            // Draw path as well
        if (trajectory.path.Count > 0) {
            float[] pathNodeCoord_prev = nodes[NodeNumber - 1].getCoordinates();
            Vector3 pathNodePos_prev = new Vector3(pathNodeCoord_prev[0], 0.0f, pathNodeCoord_prev[1]);
            foreach (Node pathNode in trajectory.path) {
                float[] pathNodeCoord = pathNode.getCoordinates();
                Vector3 pathNodePos = new Vector3(pathNodeCoord[0], 0.0f, pathNodeCoord[1]);
                drawLine(pathNodePos_prev, pathNodePos, Color.green);
                pathNodePos_prev = pathNodePos;
            }
        }
    }
}
