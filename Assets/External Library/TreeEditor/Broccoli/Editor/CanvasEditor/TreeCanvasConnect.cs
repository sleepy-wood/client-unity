using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Broccoli.Pipe;

namespace Broccoli.TreeNodeEditor
{
	using Pipeline = Broccoli.Pipe.Pipeline;
	/// <summary>
	/// Class for nodes GUI style properties.
	/// </summary>
	public class TreeCanvasConnect {
		#region ConnectionCandidate class
		/// <summary>
		/// Connection candidate description class.
		/// </summary>
		public class ConnectionCandidate
		{
			/// <summary>
			/// The candidate element to connect to.
			/// </summary>
			public PipelineElement candidate;
			/// <summary>
			/// If the seeker element can connect to the candidate using its sink pad (candidate is sink).
			/// </summary>
			public bool isSinkCandidate = false;
			/// <summary>
			/// If the seeker element can connect to the candidate using its source pad (candidate is source).
			/// </summary>
			public bool isSrcCandidate = false;
			/// <summary>
			/// True if the seeker element can connect (either using sink or source pad) but the candidate has another
			/// element already using that pad. 
			/// </summary>
			public bool isDroppable = false;
			/// <summary>
			/// Gets the candidate.
			/// </summary>
			/// <returns>The candidate.</returns>
			/// <param name="candidateElement">Candidate element.</param>
			/// <param name="isSinkCandidate">If set to <c>true</c> is sink candidate.</param>
			/// <param name="isSrcCandidate">If set to <c>true</c> is source candidate.</param>
			/// <param name="isDroppable">If set to <c>true</c> is droppable.</param>
			public static ConnectionCandidate GetCandidate (PipelineElement candidateElement, 
				bool isSinkCandidate, 
				bool isSrcCandidate, 
				bool isDroppable = false) {
				ConnectionCandidate candidate = new ConnectionCandidate ();
				candidate.candidate = candidateElement;
				candidate.isSinkCandidate = isSinkCandidate;
				candidate.isSrcCandidate = isSrcCandidate;
				candidate.isDroppable = isDroppable;
				return candidate;
			}
		}
		#endregion

		#region Vars
		/// <summary>
		/// The candidates available to connect to.
		/// </summary>
		public Dictionary<PipelineElement, ConnectionCandidate> candidates = new Dictionary<PipelineElement, ConnectionCandidate> ();
		/// <summary>
		/// Visited control list when seeking for candidates.
		/// </summary>
		public List<PipelineElement> visited = new List<PipelineElement> ();
		/// <summary>
		/// The pipeline element seeking for candidates.
		/// </summary>
		public PipelineElement seeker = null;
		/// <summary>
		/// The pipeline to inspect in search for candidates.
		/// </summary>
		public Pipeline seekingPipeline = null;
		#endregion

		#region Singleton
		/// <summary>
		/// Singleton instance.
		/// </summary>
		private static TreeCanvasConnect _treeCanvasConnect;
		/// <summary>
		/// Gets the singleton instance for this class.
		/// </summary>
		/// <returns>The singleton instance.</returns>
		public static TreeCanvasConnect GetInstance () {
			if (_treeCanvasConnect == null) {
				_treeCanvasConnect = new TreeCanvasConnect ();
			}
			return _treeCanvasConnect;
		}
		#endregion

		#region Init, Destroy and Events
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear () {
			candidates.Clear ();
			visited.Clear ();
			seeker = null;
			seekingPipeline = null;
		}
		#endregion

		#region Candidates
		/// <summary>
		/// Given a seeking pipeline element gets the potential pipeline elements it can connect to.
		/// </summary>
		/// <returns>Connection candidates.</returns>
		/// <param name="seeker">Seeking element.</param>
		/// <param name="seekingPipeline">Pipeline to look for candidates to connect to.</param>
		public Dictionary<PipelineElement, ConnectionCandidate> LookForCandidates (PipelineElement seeker, Pipeline seekingPipeline) {
			Clear ();
			this.seeker = seeker;
			this.seekingPipeline = seekingPipeline;
			if (seeker.connectionType == PipelineElement.ConnectionType.Transform && (seeker.srcElement == null || seeker.sinkElement == null)) {
				List<PipelineElement> pipelineElements = seekingPipeline.GetElements ();
				for (int i = 0; i < pipelineElements.Count; i++) {
					if (pipelineElements[i].classType == seeker.classType) {
						MarkAsVisited (pipelineElements[i]);
					}
				}
				for (int i = 0; i < pipelineElements.Count; i++) {
					if (pipelineElements[i] != seeker && !visited.Contains(pipelineElements[i])) {
						if (seeker.positionWeight > pipelineElements[i].positionWeight) {
							GetCandidatesDownstream (seeker, pipelineElements[i]);
						} else if (seeker.positionWeight < pipelineElements[i].positionWeight) {
							GetCandidatesUpstream (seeker, pipelineElements[i]);
						} else {
							// Elements allowing multiconnections.
						}
					}
				}
			}
			visited.Clear ();
			return candidates;
		}
		/// <summary>
		/// Gets the potential pipeline elements a seeker element can connect to.
		/// </summary>
		public void LookForCandidates() {
			if (seeker != null && seekingPipeline != null) {
				if (seeker.isMarkedForDeletion) {
					seeker = null;
					seekingPipeline = null;
				} else {
					LookForCandidates (this.seeker, this.seekingPipeline);
				}
			}
		}
		/// <summary>
		/// Determines whether a pipeline element is candidate for a connection.
		/// </summary>
		/// <returns><c>true</c> if the pipeline element is a candidate to connect to; otherwise, <c>false</c>.</returns>
		/// <param name="pipelineElement">Pipeline element to check as candidate.</param>
		public bool IsCandidate (PipelineElement pipelineElement) {
			return candidates.ContainsKey (pipelineElement);
		}
		/// <summary>
		/// Determines whether a pipeline element is candidate for a connection using the seeker src pad.
		/// </summary>
		/// <returns><c>true</c> if this instance is a source candidate; otherwise, <c>false</c>.</returns>
		/// <param name="pipelineElement">Pipeline element to check as source candidate.</param>
		public bool IsCandidateSrc (PipelineElement pipelineElement) {
			return candidates.ContainsKey (pipelineElement) && candidates [pipelineElement].isSrcCandidate;
		}
		/// <summary>
		/// Determines whether a pipeline element is candidate for a connection using the seeker sink pad.
		/// </summary>
		/// <returns><c>true</c> if this instance is a sink candidate; otherwise, <c>false</c>.</returns>
		/// <param name="pipelineElement">Pipeline element to check as sink candidate.</param>
		public bool IsCandidateSink (PipelineElement pipelineElement) {
			return candidates.ContainsKey (pipelineElement) && candidates [pipelineElement].isSinkCandidate;
		}
		/// <summary>
		/// Determines whether a pipeline element is candidate for a connection using the seeker sink or source pad (when the candidate
		/// has that connection pad already occupied by another pipeline element).
		/// </summary>
		/// <returns><c>true</c> if this instance is a droppable candidate; otherwise, <c>false</c>.</returns>
		/// <param name="pipelineElement">Pipeline element to check as droppable candidate.</param>
		public bool IsCandidateDroppable (PipelineElement pipelineElement) {
			return candidates.ContainsKey (pipelineElement) && candidates [pipelineElement].isDroppable;
		}
		/// <summary>
		/// Gets the connection candidates.
		/// </summary>
		/// <returns>The candidates.</returns>
		public Dictionary<PipelineElement, ConnectionCandidate> GetCandidates () {
			return candidates;
		}
		/// <summary>
		/// Determines whether this instance has candidates.
		/// </summary>
		/// <returns><c>true</c> if this instance has candidates; otherwise, <c>false</c>.</returns>
		public bool HasCandidates () {
			return candidates.Count > 0;
		}
		/// <summary>
		/// Gets the candidates downstream a reference element.
		/// </summary>
		/// <param name="seeker">Seeker.</param>
		/// <param name="reference">Reference.</param>
		private void GetCandidatesDownstream (PipelineElement seeker, PipelineElement reference) {
			if (seeker.srcElement == null) {
				visited.Add (reference);
				if (reference.sinkElement == null) {
					if (!candidates.ContainsKey (reference))
						candidates.Add (reference, ConnectionCandidate.GetCandidate (reference, true, false, false));
					MarkAsVisited (reference);
				} else if (reference.sinkElement.positionWeight > seeker.positionWeight) {
					if (!candidates.ContainsKey (reference))
						candidates.Add (reference, ConnectionCandidate.GetCandidate (reference, false, true, true));
					visited.Add (reference.sinkElement);
					MarkAsVisited (reference);
				} else {
					GetCandidatesDownstream (seeker, reference.sinkElement);
				}
			}
		}
		/// <summary>
		/// Gets the candidates upstream a reference element.
		/// </summary>
		/// <param name="seeker">Seeker.</param>
		/// <param name="reference">Reference.</param>
		private void GetCandidatesUpstream (PipelineElement seeker, PipelineElement reference) {
			if (seeker.sinkElement == null) {
				visited.Add (reference);
				if (reference.srcElement == null) {
					if (!candidates.ContainsKey (reference))
						candidates.Add (reference, ConnectionCandidate.GetCandidate (reference, false, true, false));
					MarkAsVisited (reference);
				} else if (reference.srcElement.positionWeight < seeker.positionWeight) {
					if (!candidates.ContainsKey (reference))
						candidates.Add (reference, ConnectionCandidate.GetCandidate (reference, true, false, true));
					visited.Add (reference.srcElement);
					MarkAsVisited (reference);
				} else {
					GetCandidatesUpstream (seeker, reference.srcElement);
				}
			}
		}
		/// <summary>
		/// Marks as visited and element when looking for candidates.
		/// </summary>
		/// <param name="reference">Reference element.</param>
		private void MarkAsVisited (PipelineElement reference) {
			PipelineElement orig = reference;
			while (reference != null) {
				if (!visited.Contains (reference))
					visited.Add (reference);
				reference = reference.sinkElement;
			}
			reference = orig;
			while (reference != null) {
				if (!visited.Contains (reference))
					visited.Add (reference);
				reference = reference.srcElement;
			}
		}
		#endregion
	}
}