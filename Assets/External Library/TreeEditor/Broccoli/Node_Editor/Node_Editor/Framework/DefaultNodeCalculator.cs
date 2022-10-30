﻿using System;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.NodeEditorFramework;

namespace Broccoli.NodeEditorFramework.Standard
{
	public class DefaultCanvasCalculator : NodeCanvasTraversal
	{
		// A list of Nodes from which calculation originates -> Call StartCalculation
		public List<Node> workList;
		private int calculationCount;

		public DefaultCanvasCalculator (NodeCanvas canvas) : base(canvas)
		{
		}

		/// <summary>
		/// Recalculate from every Input Node.
		/// Usually does not need to be called at all, the smart calculation system is doing the job just fine
		/// </summary>
		public override void TraverseAll () 
		{
			workList = new List<Node> ();
			for (int i = 0; i < nodeCanvas.nodes.Count; i++)
			{
				if (nodeCanvas.nodes[i].isInput ())
				{ // Add all Inputs
					nodeCanvas.nodes[i].ClearCalculation ();
					workList.Add (nodeCanvas.nodes[i]);
				}
			}
			StartCalculation ();
		}

		/// <summary>
		/// Recalculate from this node. 
		/// Usually does not need to be called manually
		/// </summary>
		public override void OnChange (Node node) 
		{
			node.ClearCalculation ();
			workList = new List<Node> { node };
			StartCalculation ();
		}

		/// <summary>
		/// Iterates through workList and calculates everything, including children
		/// </summary>
		private void StartCalculation () 
		{
			if (workList == null || workList.Count == 0)
				return;
			// this blocks iterates through the worklist and starts calculating
			// if a node returns false, it stops and adds the node to the worklist
			// this workList is worked on until it's empty or a limit is reached
			calculationCount = 0;
			bool limitReached = false;
			for (int roundCnt = 0; !limitReached; roundCnt++)
			{ // Runs until every node possible is calculated
				limitReached = true;
				for (int workCnt = 0; workCnt < workList.Count; workCnt++)
				{
					if (ContinueCalculation (workList[workCnt]))
						limitReached = false;
				}
				if (roundCnt > 1000)
					limitReached = true;
			}
		}

		/// <summary>
		/// Recursive function which continues calculation on this node and all the child nodes
		/// Usually does not need to be called manually
		/// Returns success/failure of this node only
		/// </summary>
		private bool ContinueCalculation (Node node) 
		{
			if (node.calculated)
				return false;
			if ((node.descendantsCalculated () || node.isInLoop ()) && node.Calculate ())
			{ // finished Calculating, continue with the children
				node.calculated = true;
				calculationCount++;
				workList.Remove (node);
				if (node.ContinueCalculation && calculationCount < 1000) 
				{
					for (int i = 0; i < node.Outputs.Count; i++)
					{
						for (int j = 0; j < node.Outputs [i].connections.Count; j++) {
							ContinueCalculation (node.Outputs [i].connections [j].body);
						}
					}
				}
				else if (calculationCount >= 1000)
					Debug.LogError ("Stopped calculation because of suspected Recursion. Maximum calculation iteration is currently at 1000!");
				return true;
			}
			else if (!workList.Contains (node)) 
			{ // failed to calculate, add it to check later
				workList.Add (node);
			}
			return false;
		}
	}
}

