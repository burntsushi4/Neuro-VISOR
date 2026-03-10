using C2M2.Interaction;
using C2M2.NeuronalDynamics.Interaction.UI;
using UnityEngine;

[RequireComponent(typeof(NDLineGraph))]
public class NDGraph : NDInteractables
{
    public NDGraphManager GraphManager { get { return simulation.graphManager; } }

    public NDLineGraph ndlinegraph;

    //if set, this graph plots synapse current instead of the other plot
    public Synapse trackedSynapse = null;
    // Start is called before the first frame update
    void Awake()
    {
        ndlinegraph = GetComponent<NDLineGraph>();
    }

    // Update is called once per frame
    void Update()
    {
        if (simulation == null || GraphManager == null)
        {
            ndlinegraph.DestroyPlot();
        }
    }

    private void OnDestroy()
    {
        GraphManager.graphs.Remove(this);
    }

    public override void Place(int index)
    {
        if (FocusVert == -1)
        {
            Debug.LogError("Invalid vertex given to NDLineGraph");
            Destroy(this);
        }
        //reflects whether this is a synapse plot or voltage plot... specify neuron-vert?
        if (trackedSynapse != null)
            name = "Graph(" + simulation.name + ")[synapse-vert" + FocusVert + "]";
        else
            name = "Graph(" + simulation.name + ")[vert" + FocusVert + "]";

        GraphManager.graphs.Add(this);
    }

    protected override void AddHitEventListeners()
    {
    }
}
