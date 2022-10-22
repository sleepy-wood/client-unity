using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Element of a pipeline.
	/// </summary>
	[System.Serializable]
	public abstract class PipelineElement : ScriptableObject {
		#region Vars
		/// <summary>
		/// Base weight for structure generator elements.
		/// </summary>
		public static int structureGeneratorWeight = 0;
		/// <summary>
		/// Base weight for structure transform elements.
		/// </summary>
		public static int structureTransformWeight = 100;
		/// <summary>
		/// Base weight for mesh generator elements.
		/// </summary>
		public static int meshGeneratorWeight = 200;
		/// <summary>
		/// Base weight for mapper elements.
		/// </summary>
		public static int mapperWeight =300;
		/// <summary>
		/// Base weight for effect elements.
		/// </summary>
		public static int effectWeight = 400;
		/// <summary>
		/// Possible type of connection on the pipeline for this <see cref="Broccoli.Pipe.PipelineElement"/>.
		/// </summary>
		public enum ConnectionType {
			Source,
			Sink,
			Transform
		};
		/// <summary>
		/// Types of elements according to their function on the pipeline processing.
		/// </summary>
		public enum ElementType {
			StructureGenerator,
			StructureTransform,
			MeshGenerator,
			MeshTransform,
			SproutGenerator,
			SproutTransform,
			Positioner,
			Baker
		};
		/// <summary>
		/// Class type unique for each <see cref="Broccoli.Pipe.PipelineElement"/>.
		/// </summary>
		public enum ClassType {
			Base,
			LSystem,
			GirthTransform,
			BranchMeshGenerator,
			Positioner,
			SparsingTransform,
			LengthTransform,
			WindEffect,
			BranchBender,
			BranchMapper,
			SproutGenerator,
			SproutMeshGenerator,
			SproutMapper,
			StructureGenerator,
			TrunkMeshGenerator,
			ProceduralBranchMapper,
			Baker
		};
		/// <summary>
		/// Randomization is using a fixed seed.
		/// </summary>
		public bool isSeedFixed = false;
		/// <summary>
		/// The fixed seed used to randomize all processing for this <see cref="Broccoli.Pipe.PipelineElement"/>.
		/// </summary>
		public int fixedSeed = 0;
		/// <summary>
		/// Seed used for processing on this <see cref="Broccoli.Pipe.PipelineElement"/>, if isSeedFixed is true
		/// then it has the same value as fixedSeed.
		/// </summary>
		public int seed = 0;
		/// <summary>
		/// If true the <see cref="Broccoli.Pipe.PipelineElement"/> takes part in the processing, otherwise it does not.
		/// </summary>
		public bool isActive = true;
		/// <summary>
		/// Parent pipeline.
		/// </summary>
		[System.NonSerialized]
		public Pipeline pipeline = null;
		/// <summary>
		/// The <see cref="Broccoli.Pipe.PipelineElement"/> is on valid pipeline.
		/// </summary>
		[System.NonSerialized]
		public bool isOnValidPipeline = false;
		/// <summary>
		/// The pipeline is marked for deletion, used on the serialization process to
		/// discard this <see cref="Broccoli.Pipe.PipelineElement"/> from serializing.
		/// </summary>
		[System.NonSerialized]
		public bool isMarkedForDeletion = false;
		/// <summary>
		/// Gets the type of the connection for the <see cref="Broccoli.Pipe.PipelineElement"/>.
		/// </summary>
		/// <value>The type of the connection.</value>
		public virtual ConnectionType connectionType { get; private set; }
		/// <summary>
		/// Gets the type of the <see cref="Broccoli.Pipe.PipelineElement"/>.
		/// </summary>
		/// <value>The type of the element.</value>
		public virtual ElementType elementType { get; private set; }
		/// <summary>
		/// Gets the unique class type for the <see cref="Broccoli.Pipe.PipelineElement"/>.
		/// </summary>
		/// <value>The type of the class.</value>
		public virtual ClassType classType { get; private set; }
		/// <summary>
		/// Value used to position elements in the pipeline. The greater the more towards the end of the pipeline.
		/// </summary>
		public virtual int positionWeight { get { return 0; } }
		/// <summary>
		/// Indicating whether this <see cref="Broccoli.Pipe.PipelineElement"/> uses randomization.
		/// </summary>
		/// <value><c>true</c> if uses randomization; otherwise, <c>false</c>.</value>
		public virtual bool usesRandomization { get { return false; } }
		/// <summary>
		/// Gets a value indicating whether this <see cref="Broccoli.Pipe.PipelineElement"/> unique on pipeline.
		/// </summary>
		/// <value><c>true</c> if unique on pipeline; otherwise, <c>false</c>.</value>
		public virtual bool uniqueOnPipeline { get { return true; } }
		/// <summary>
		/// The source, or upstream, <see cref="Broccoli.Pipe.PipelineElement"/> this element is connected to.
		/// </summary>
		[System.NonSerialized]
		public PipelineElement srcElement = null;
		/// <summary>
		/// The sink, or downstream, <see cref="Broccoli.Pipe.PipelineElement"/> this element is connected to.
		/// </summary>
		[System.NonSerialized]
		public PipelineElement sinkElement = null;
		/// <summary>
		/// Unique identifier of the element on the pipeline.
		/// </summary>
		public int id = -1;
		/// <summary>
		/// Flag to use a unique keyname to identify this element instance.
		/// </summary>
		public bool useKeyName = false;
		/// <summary>
		/// Unique key to identify this pipeline element when running queries.
		/// </summary>
		public string keyName = string.Empty;
		/// <summary>
		/// True if this element has a key assigned to it.
		/// </summary>
		/// <value>True if a key has been assigned, false otherwise.</value>
		public bool hasKeyName {
			get { return useKeyName && !string.IsNullOrEmpty (keyName); }
		}
		/// <summary>
		/// Identifier of the element connected at the src pad.
		/// </summary>
		public int srcElementId = -1;
		/// <summary>
		/// Identifier of the element connected at the sink pad.
		/// </summary>
		public int sinkElementId = -1;
		/// <summary>
		/// The index for the <see cref="Broccoli.Pipe.PipelineElement"/> on the pipeline serialized array.
		/// </summary>
		public int index = -1;
		/// <summary>
		/// The index of the source <see cref="Broccoli.Pipe.PipelineElement"/> on the pipeline serialized array.
		/// </summary>
		public int srcElementIndex = -1;
		/// <summary>
		/// The index of the sink <see cref="Broccoli.Pipe.PipelineElement"/> on the pipeline serialized array.
		/// </summary>
		public int sinkElementIndex = -1;
		/// <summary>
		/// The type of the source element class, needed for serialization.
		/// </summary>
		public ClassType srcElementClassType = ClassType.Base;
		/// <summary>
		/// The type of the sink element class, needed for serialization.
		/// </summary>
		public ClassType sinkElementClassType = ClassType.Base;
		/// <summary>
		/// If true, the element has new data that needs
		/// to be consider to refresh the processing.
		/// </summary>
		[System.NonSerialized]
		public bool hasChanged = true;
		/// <summary>
		/// The node position for this element on the editor.
		/// </summary>
		[SerializeField]
		public Vector2 nodePosition = Vector2.zero;
		/// <summary>
		/// Log queue for relevant events on the factory.
		/// </summary>
		[System.NonSerialized]
		public Queue<LogItem> log = new Queue<LogItem> (1);
		#endregion

		#region Operations
		/// <summary>
		/// Prepares the seed for all the element operation.
		/// </summary>
		public void PrepareSeed () {
			if (isSeedFixed) {
				seed = fixedSeed;
			} else if (pipeline != null) {
				seed = pipeline.seed;
			} else {
				seed = (int)System.DateTime.Now.Ticks;
			}
			Random.InitState (seed);
		}
		#endregion

		#region Traversing
		/// <summary>
		/// Gets the downstream or sink connected element.
		/// </summary>
		/// <returns>The downstream element.</returns>
		/// <param name="classType">Class type.</param>
		public PipelineElement GetDownstreamElement (ClassType classType) {
			if (sinkElement != null) {
				if (sinkElement.classType == classType) {
					return sinkElement;
				} else {
					return sinkElement.GetDownstreamElement (classType);
				}
			}
			return null;
		}
		/// <summary>
		/// Gets the downstream or sink connected elements.
		/// </summary>
		/// <returns>The downstream elements.</returns>
		/// <param name="classType">Class type.</param>
		public List<PipelineElement> GetDownstreamElements (ClassType classType) {
			// TODO: protect against looping.
			List<PipelineElement> downstreamElements = new List<PipelineElement> ();
			PipelineElement downstreamElement = sinkElement;
			while (downstreamElement != null) {
				if (downstreamElement.classType == classType) {
					downstreamElements.Add (downstreamElement);
				}
				downstreamElement = downstreamElement.sinkElement;
			}
			return downstreamElements;
		}
		/// <summary>
		/// Gets the upstream or source connected element.
		/// </summary>
		/// <returns>The upstream element.</returns>
		/// <param name="classType">Class type.</param>
		public PipelineElement GetUpstreamElement (ClassType classType) {
			if (srcElement != null) {
				if (srcElement.classType == classType) {
					return srcElement;
				} else {
					return srcElement.GetUpstreamElement (classType);
				}
			}
			return null;
		}
		/// <summary>
		/// Gets the upstream or source connected elements.
		/// </summary>
		/// <returns>The upstream elements.</returns>
		/// <param name="classType">Class type.</param>
		public List<PipelineElement> GetUpstreamElements (ClassType classType) {
			// TODO: protect against looping.
			List<PipelineElement> upstreamElements = new List<PipelineElement> ();
			PipelineElement upstreamElement = srcElement;
			while (upstreamElement != null) {
				if (upstreamElement.classType == classType) {
					upstreamElements.Add (upstreamElement);
				}
				upstreamElement = upstreamElement.srcElement;
			}
			return upstreamElements;
		}
		#endregion

		#region Validation
		/// <summary>
		/// Validate this instance.
		/// </summary>
		public virtual bool Validate () { return true; }
		#endregion

		#region Events
		/// <summary>
		/// Raises the add to pipeline event.
		/// </summary>
		public virtual void OnAddToPipeline () {
		}
		/// <summary>
		/// Raises the remove from pipeline event.
		/// </summary>
		public virtual void OnRemoveFromPipeline () {
		}
		/// <summary>
		/// Raises the update event.
		/// </summary>
		public virtual void OnUpdate () {
		}
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		public abstract PipelineElement Clone ();
		protected void SetCloneProperties (PipelineElement clone) {
			clone.id = id;
			clone.useKeyName = useKeyName;
			clone.keyName = keyName;
			clone.isSeedFixed = isSeedFixed;
			clone.fixedSeed = fixedSeed;
			clone.seed = seed;
			clone.isActive = isActive;
			clone.index = index;
			clone.srcElementId = srcElementId;
			clone.srcElementIndex = srcElementIndex;
			clone.srcElementClassType = srcElementClassType;
			clone.sinkElementId = sinkElementId;
			clone.sinkElementIndex = sinkElementIndex;
			clone.sinkElementClassType = sinkElementClassType;
			clone.nodePosition = new Vector2 (nodePosition.x, nodePosition.y);
		}
		#endregion
	}
}