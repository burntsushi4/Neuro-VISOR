﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C2M2.NeuronalDynamics.Simulation;
namespace C2M2.NeuronalDynamics.Interaction
{
    public class CloseNDSimulation : MonoBehaviour
    {
        public NDSimulation sim = null;

        public void CloseSimulation()
        {
            if(sim != null)
            {
                // Destroy the cell
                Destroy(sim.gameObject);

                if(GameManager.instance.cellPreviewer != null)
                {
                    // Reenable the cell previewer
                    GameManager.instance.cellPreviewer.SetActive(true);
                }

                // Destroy this control panel
                Destroy(transform.root.gameObject);
            }
        }
    }
}
