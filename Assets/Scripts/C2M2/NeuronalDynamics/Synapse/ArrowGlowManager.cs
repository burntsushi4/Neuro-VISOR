/*
using C2M2.NeuronalDynamics.Simulation;
using UnityEngine;

public class ArrowGlowManager : MonoBehaviour
{
    public Material glowMat;
    private Material originalMaterial;

    public bool isSynapseActive { get; set; } = false;

    /// <summary>
    /// A script to update arrow effects based on voltage activity
    /// </summary>

    private void Awake()
    {
        // Store the original material for restoration later
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
        }
    }

    public void SetGlow()
    {
        if (glowMat != null)
        {
            foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
            {
                mr.material = glowMat;
            }
        }
    }

    public void DeactivateGlow()
    {
        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            mr.material = originalMaterial;
        }
    }

    private void Update()
    {
        // Example: Sync glow state with the synapse active state
        if (isSynapseActive)
        {
            SetGlow();
        }
        else
        {
            DeactivateGlow();
        }
    }
}
*/