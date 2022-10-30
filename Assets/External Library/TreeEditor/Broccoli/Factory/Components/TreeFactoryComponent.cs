using UnityEngine;

using Broccoli.Pipe;
using Broccoli.Model;
using Broccoli.Factory;

namespace Broccoli.Component
{
	/// <summary>
	/// Tree factory component.
	/// A component is assigned to each element on a pipeline to
	/// process data to build a tree.
	/// </summary>
	public abstract class TreeFactoryComponent {
		#region Vars
		/// <summary>
		/// The pipeline element used as reference for processing.
		/// </summary>
		public PipelineElement pipelineElement = null;
		/// <summary>
		/// The tree to process to.
		/// </summary>
		public BroccoTree tree = null;
		/// <summary>
		/// Initial random state when processing this component.
		/// </summary>
		public Random.State randomState;
		#endregion

		#region Configuration
		/// <summary>
		/// Prepares the parameters to process with this component.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		/// <param name="useLocalCache">If set to <c>true</c> use local cache.</param>
		/// <param name="processControl">Process control.</param>
		protected virtual void PrepareParams (TreeFactory treeFactory,
			bool useCache = false, 
			bool useLocalCache = false, 
			TreeFactoryProcessControl processControl = null)
		{
			SaveRandomState ();
			/*
			if (processControl != null && processControl.lodIndex == 1) {
				SaveRandomState ();
			} else if (processControl != null && processControl.lodIndex == 2) {
				LoadRandomState ();
			}
			*/
		}
		protected void SaveRandomState () {
			randomState = Random.state;
		}
		protected void LoadRandomState () {
			Random.state = randomState;
		}
		/// <summary>
		/// Gets the changed aspects on the tree for this component.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public abstract int GetChangedAspects ();
		/// <summary>
		/// Clears the cache.
		/// </summary>
		public virtual void ClearCache () {}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public virtual void Clear () {
			pipelineElement = null;
			tree = null;
			ClearCache ();
		}
		#endregion

		#region Processing
		/// <summary>
		/// Process the tree according to the pipeline element.
		/// </summary>
		/// <param name="treeFactory">Parent tree factory.</param>
		/// <param name="useCache">If set to <c>true</c> use absolute cache.</param>
		/// <param name="useLocalCache">If set to <c>true</c> use local cache.</param>
		/// <param name="processControl">Process control.</param>
		public abstract bool Process (TreeFactory treeFactory, 
			bool useCache = false, 
			bool useLocalCache = false, 
			TreeFactoryProcessControl processControl = null);
		/// <summary>
		/// Removes the product of this component on the factory processing.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		public virtual void Unprocess (TreeFactory treeFactory) {}
		/// <summary>
		/// Processes called only on the prefab creation.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		public virtual void OnProcessPrefab (TreeFactory treeFactory) {}
		/// <summary>
		/// Process a special command or subprocess on this component.
		/// </summary>
		/// <param name="cmd">Cmd.</param>
		/// <param name="treeFactory">Tree factory.</param>
		public virtual void ProcessComponentOnly (int cmd, TreeFactory treeFactory) {}
		/// <summary>
		/// Gets the process prefab weight.
		/// </summary>
		/// <returns>The process prefab weight.</returns>
		/// <param name="treeFactory">Tree factory.</param>
		public virtual int GetProcessPrefabWeight (TreeFactory treeFactory) { return 0; }
		#endregion
	}
	/// <summary>
	/// Tree factory process control.
	/// Controls the the pipeline processing so that only the requiered aspects would be rebuilt.
	/// </summary>
	public class TreeFactoryProcessControl {
		#region Vars
		/// <summary>
		/// Various aspects that could change on the processing.
		/// </summary>
		public enum ChangedAspect {
			None = 0,
			Structure = 1,
			StructurePosition = 2,
			StructureGirth = 4,
			StructureLength = 8,
			StructureBendPoints = 16,
			Mesh = 32,
			Material = 64
		};
		/// <summary>
		/// The pipeline element that elicits the change.
		/// </summary>
		public PipelineElement elicitingPipelineElement = null;
		/// <summary>
		/// LOD definition index to use when processing the pipeline.
		/// </summary>
		public int lodIndex = 1;
		/// <summary>
		/// The changed aspects.
		/// </summary>
		public int changedAspects = (int)ChangedAspect.None;
		/// <summary>
		/// The aspects to lock down the pipeline (so other components don't work on them).
		/// </summary>
		public int lockedAspects = (int)ChangedAspect.None;
		/// <summary>
		/// Types of processing done by the factory.
		/// </summary>
		public enum ProcessType {
			Preview = 0,
			Runtime = 1,
			Prefab = 2
		}
		/// <summary>
		/// Current type of processing.
		/// </summary>
		public ProcessType processType = ProcessType.Preview;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Factory.TreeFactoryProcessControl"/> class.
		/// </summary>
		/// <param name="elicitingPipelineElement">Eliciting pipeline element.</param>
		/// <param name="processType">Type of process for the tree: preview, runtime or prefab.</param>
		/// <param name="lodIndex">The LOD definition to process.</param>
		public TreeFactoryProcessControl (PipelineElement elicitingPipelineElement, ProcessType processType, int lodIndex) {
			this.elicitingPipelineElement = elicitingPipelineElement;
			this.processType = processType;
			this.lodIndex = lodIndex;
		}
		#endregion

		#region Aspects
		/// <summary>
		/// Adds a changed aspect.
		/// </summary>
		/// <param name="changedAspect">Changed aspect.</param>
		public void AddChangedAspect (ChangedAspect changedAspect) {
			this.changedAspects = this.changedAspects | (int)changedAspect;
		}
		/// <summary>
		/// Adds a changed aspects.
		/// </summary>
		/// <param name="changedAspects">Changed aspects.</param>
		public void AddChangedAspects (int changedAspects) {
			this.changedAspects = this.changedAspects | changedAspects;
		}
		/// <summary>
		/// Determines whether this instance has some changed aspect.
		/// </summary>
		/// <returns><c>true</c> if this instance has the specified changedAspect; otherwise, <c>false</c>.</returns>
		/// <param name="changedAspect">Changed aspect.</param>
		public bool HasChangedAspect (ChangedAspect changedAspect) {
			int result = changedAspects & (int)changedAspect; 
			return result != 0;
		}
		/// <summary>
		/// Checks if the current process is to generate a preview tree.
		/// </summary>
		/// <returns>True if the process type is a preview tree.</returns>
		public bool isPreviewProcess {
			get { return this.processType == ProcessType.Preview; }
		}
		/// <summary>
		/// Checks if the current process is to generate a tree at runtime.
		/// </summary>
		/// <returns>True if the process type is a tree at runtime.</returns>
		public bool isRuntimeProcess {
			get { return this.processType == ProcessType.Runtime; }
		}
		/// <summary>
		/// Checks if the current process is to generate a prefab tree.
		/// </summary>
		/// <returns>True if the process type is a prefab tree.</returns>
		public bool isPrefabProcess {
			get { return this.processType == ProcessType.Prefab; }
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear () {
			this.elicitingPipelineElement = null;
		}
		#endregion
	}
}