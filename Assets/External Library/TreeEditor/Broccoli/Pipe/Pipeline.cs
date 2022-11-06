using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.Factory;

namespace Broccoli.Pipe {
	/// <summary>
	/// Pipeline structure, holds all the elements used to create a tree.
	/// </summary>
	[System.Serializable]
	public class Pipeline : ScriptableObject, ISerializationCallbackReceiver {
		#region UndoControl class
		/// <summary>
		/// Class used to keep account of the undo process for this pipeline.
		/// </summary>
		[System.Serializable]
		public class UndoControl
		{
			/// <summary>
			/// The undo count.
			/// </summary>
			public int undoCount = 0;
			/// <summary>
			/// The canvas undo count.
			/// </summary>
			public int canvasUndoCount = 0;
		}
		#endregion

		#region Vars
		/// <summary>
		/// Possibles states of validation for the pipeline.
		/// </summary>
		public enum State
		{
			Valid,
			Empty,
			NoSourceElement,
			NoSinkElement,
			MultiElement,
			MultiplePipelines,
			InvalidConnection
		};
		/// <summary>
		/// Elements used to create a tree.
		/// </summary>
		[System.NonSerialized]
		public List<PipelineElement> elements = new List<PipelineElement> (); // TODO: 05/04/2017 make it private
                /// <summary>
                /// Dictionary for the relationship between ids and elements.
                /// </summary>
                [System.NonSerialized]
                public Dictionary<int, PipelineElement> idToElement = new Dictionary<int, PipelineElement> ();
		/// <summary>
		/// On a single valid sequence of connected elements the first element of them.
		/// </summary>
		[System.NonSerialized]
		public PipelineElement root = null;
		/// <summary>
		/// The state of validation for this pipeline.
		/// </summary>
		[System.NonSerialized]
		public State state = State.Empty;
		/// <summary>
		/// Number of valid connected series of elements.
		/// </summary>
		[System.NonSerialized]
		public int validPipelines = 0;
		/// <summary>
		/// Point of origin to create the tree.
		/// </summary>
		[System.NonSerialized]
		public Vector3 origin = Vector3.zero;
		/// <summary>
		/// Object used to serialize the pipeline.
		/// </summary>
		[SerializeField]
		public PipelineSerializable _serializedPipeline = new PipelineSerializable ();
		/// <summary>
		/// Sprout groups on the pipeline, used to create leafs and other offspring from the branches.
		/// </summary>
		[SerializeField]
		public SproutGroups _sproutGroups = new SproutGroups ();
		/// <summary> 
		/// Accessor for sprout groups.
		/// </summary>
		/// <value>The sprout groups.</value>
		public SproutGroups sproutGroups { get { return _sproutGroups; } private set { } }
		/// <summary>
		/// The tree factory preferences.
		/// </summary>
		public TreeFactoryPreferences treeFactoryPreferences = new TreeFactoryPreferences ();
		/// <summary>
		/// This pipeline is a catalog item.
		/// </summary>
		public bool isCatalogItem = false;
		/// <summary>
		/// The undo control.
		/// </summary>
		public UndoControl undoControl = new UndoControl ();
		/// <summary>
		/// The checked elements already checked when validating the pipeline.
		/// </summary>
		public List<int> checkedElementsOnValidation = new List<int> ();
		/// <summary>
		/// To delete pipeline elements.
		/// </summary>
		public List<PipelineElement> toDeletePipelineElements = new List<PipelineElement> ();
		/// <summary>
		/// The source elements.
		/// </summary>
		public List<PipelineElement> srcElements = new List<PipelineElement>();
		/// <summary>
		/// The seed used to process the pipeline.
		/// </summary>
		public int seed = -1;
		/// <summary>
		/// The random state used to process the pipeline.
		/// </summary>
		public Random.State randomState;
		/// <summary>
		/// Maintains a relationship between pipeline elements and their keynames.
		/// </summary>
		/// <typeparam name="string">Pipeline element keyname.</typeparam>
		/// <typeparam name="PipelineElement">Pipeline element.</typeparam>
		/// <returns>Pipeline element.</returns>
		public Dictionary<string, PipelineElement> keyNameToPipelineElement = new Dictionary<string, PipelineElement> ();
		#endregion

		#region Events
		/// <summary>
		/// Init this instance.
		/// </summary>
		public void Init () {
			for (int i = 0; i < elements.Count; i++) {
				elements[i].pipeline = this;
				elements[i].OnAddToPipeline ();
			}
			Validate ();
		}
		/// <summary>
		/// Raises the destroy event.
		/// </summary>
		public void OnDestroy () {
			elements.Clear ();
			idToElement.Clear ();
			keyNameToPipelineElement.Clear ();
		}
		#endregion

		#region Elements
		/// <summary>
		/// Gets the elements.
		/// </summary>
		/// <returns>The elements.</returns>
		public List<PipelineElement> GetElements () {
			return elements;
		}
		/// <summary>
		/// Adds a element to the pipeline.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public bool AddElement (PipelineElement pipelineElement) {
			pipelineElement.pipeline = this;
			BuildIdToElementIndex ();
			SetIdIfUnset (pipelineElement);
			elements.Add (pipelineElement);
			AddPipelineElementToIndex (pipelineElement);
			if (!idToElement.ContainsKey (pipelineElement.id)) {
				idToElement.Add (pipelineElement.id, pipelineElement);
				pipelineElement.OnAddToPipeline ();
				return true;
			} else {
				// Repeated element with repeated id.
				elements.Remove (pipelineElement);
			}
			return false;
		}
		/// <summary>
		/// Sets the identifier of the element if unset.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		private void SetIdIfUnset (PipelineElement pipelineElement) {
			if (pipelineElement.id < 0) {
				int id = 0;
				while (idToElement.ContainsKey (id)) {
					id++;
				}
				pipelineElement.id = id;
			}
		}
		/// <summary>
		/// Removes an element from the pipeline.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public void RemoveElement (PipelineElement pipelineElement) {
			pipelineElement.OnRemoveFromPipeline ();
			if (pipelineElement.srcElement != null) {
				pipelineElement.srcElement.sinkElement = null;
				pipelineElement.srcElement.sinkElementIndex = -1;
				pipelineElement.srcElement.sinkElementId = -1;
			}
			if (pipelineElement.sinkElement != null) {
				pipelineElement.sinkElement.srcElement = null;
				pipelineElement.sinkElement.srcElementIndex = -1;
				pipelineElement.sinkElement.srcElementId = -1;
			}
			pipelineElement.srcElement = null;
			pipelineElement.sinkElement = null;
			if (idToElement.ContainsKey (pipelineElement.id)) {
				idToElement.Remove (pipelineElement.id);
				RemovePipelineElementFromIndex (pipelineElement);
			}
			elements.Remove (pipelineElement);
		}
		/// <summary>
		/// Removes all elements.
		/// </summary>
		public void RemoveAllElements () {
			for (int i = 0; i < elements.Count; i++) {
				elements[i].isMarkedForDeletion = true;
			}
			Update ();
		}
		/// <summary>
		/// Gets the number of elementson the pipeline, connected or not.
		/// </summary>
		/// <returns>The elements count.</returns>
		public int GetElementsCount () {
			return elements.Count;
		}
		/// <summary>
		/// Update the pipeline and its elements.
		/// </summary>
		public void Update() {
			toDeletePipelineElements.Clear ();
			for (int i = elements.Count - 1; i >= 0; i--) {
				if (elements [i].isMarkedForDeletion) {
					toDeletePipelineElements.Add (elements[i]);
				}
			}
			for (int i = 0; i < toDeletePipelineElements.Count; i++) {
				RemoveElement (toDeletePipelineElements[i]);
			}
		}
		/// <summary>
		/// Updates all elements of certain type on the pipeline.
		/// </summary>
		/// <param name="classType">Class type.</param>
		public void UpdateElementsOfType (PipelineElement.ClassType classType) {
			for (int i = 0; i < elements.Count; i++) {
				if (elements[i].classType == classType) {
					elements[i].OnUpdate ();
				}
			}
		}
		/// <summary>
		/// Gets the first PipelineElement found on a pipeline of a ClassType.
		/// </summary>
		/// <returns>First element found on the pipeline.</returns>
		/// <param name="classType">Class type of the element.</param>
		/// <param name="connectionValid">If set to <c>true</c> the element must be on a valid pipeline.</param>
		public PipelineElement GetElement (PipelineElement.ClassType classType, bool connectionValid = true) {
			if (elements.Count > 0) {
				for (int i = 0; i < elements.Count; i++) {
					if (elements[i].classType == classType) {
						if (connectionValid && elements[i].isOnValidPipeline) {
							return elements[i];
						} else {
							return elements[i];
						}
					}
				}
			}
			return null;
		}
		/// <summary>
		/// Gets the element by identifier.
		/// </summary>
		/// <returns>The element by identifier.</returns>
		/// <param name="id">Identifier.</param>
		public PipelineElement GetElementById (int id) {
			if (id >= 0) {
				if (idToElement.ContainsKey (id)) {
					return idToElement [id];
				}
			}
			return null;
		}
		/// <summary>
		/// Gets all the elements on a pipeline of a ClassType.
		/// </summary>
		/// <returns>The elements.</returns>
		/// <param name="classType">Class type.</param>
		/// <param name="connectionValid">If set to <c>true</c> the element must be on a valid pipeline.</param>
		public List<PipelineElement> GetElements (PipelineElement.ClassType classType, bool connectionValid = true) {
			List<PipelineElement> pipelineElements = new List<PipelineElement> ();
			if (elements.Count > 0) {
				for (int i = 0; i < elements.Count; i++) {
					if (elements[i].classType == classType) {
						if (connectionValid && elements[i].isOnValidPipeline) {
							pipelineElements.Add (elements[i]);
						} else {
							pipelineElements.Add (elements[i]);
						}
					}
				}
			}
			return pipelineElements;
		}
		#endregion

		#region Sprout Groups
		/// <summary>
		/// Look if certain sprout group is being used in any of the pipeline elements.
		/// </summary>
		/// <returns><c>true</c>, if sprout group is being used, <c>false</c> otherwise.</returns>
		/// <param name="sproutGroupId">Sprout group identifier.</param>
		public bool HasSproutGroupUsage (int sproutGroupId) {
			for (int i = 0; i < elements.Count; i++) {
				if (elements[i] is ISproutGroupConsumer) {
					if (((ISproutGroupConsumer)elements[i]).HasSproutGroupUsage (sproutGroupId)) {
						return true;
					}
				}
			}
			return false;
		}
		/// <summary>
		/// Deletes a sprout sprout group given its id, informing all the pipeline elements
		/// that might be using it.
		/// </summary>
		/// <param name="sproutGroupId">Sprout group identifier.</param>
		public void DeleteSproutGroup (int sproutGroupId) {
			sproutGroups.DeleteSproutGroup (sproutGroupId);
			for (int i = 0; i < elements.Count; i++) {
				if (elements[i] is ISproutGroupConsumer) {
					((ISproutGroupConsumer)elements[i]).StopSproutGroupUsage (sproutGroupId);
				}
			}
		}
		#endregion

		#region Branch Descriptor
		/// <summary>
		/// Look if certain branch descriptor is being used in any of the pipeline elements.
		/// </summary>
		/// <returns><c>true</c>, if branch descriptor is being used, <c>false</c> otherwise.</returns>
		/// <param name="branchDescriptorId">Branch descriptor identifier.</param>
		public bool HasBranchDescriptorUsage (int branchDescriptorId) {
			/*
			for (int i = 0; i < elements.Count; i++) {
				if (elements[i] is IBranchDescriptorConsumer) {
					if (((IBranchDescriptorConsumer)elements[i]).HasBranchDescriptorUsage (branchDescriptorId)) {
						return true;
					}
				}
			}
			*/
			return false;
		}
		/// <summary>
		/// Deletes a sprout branch descriptor given its id, informing all the pipeline elements
		/// that might be using it.
		/// </summary>
		/// <param name="branchDescriptorId">Branch descriptor identifier.</param>
		public void DeleteBranchDescriptor (int branchDescriptorId) {
			//branchDescriptors.DeleteBranchDescriptor (branchDescriptorId);
			/*
			for (int i = 0; i < elements.Count; i++) {
				if (elements[i] is IBranchDescriptorConsumer) {
					((IBranchDescriptorConsumer)elements[i]).StopBranchDescriptorUsage (branchDescriptorId);
				}
			}
			*/
		}
		#endregion

		#region Seed
		/// <summary>
		/// Prepares the seed to process this pipeline. This seeds is used by all the elements
		/// on the pipeline participating in the processing.
		/// </summary>
		public void GenerateSeed () {
            seed = 10;// (int)System.DateTime.Now.Ticks;
        }
		#endregion

		#region Serialization
		/// <summary>
		/// Raises the before serialize event.
		/// </summary>
		public void OnBeforeSerialize () {
			_serializedPipeline.Clear ();
			for (int i = 0; i < elements.Count; i++) {
				if (!elements [i].isMarkedForDeletion) {
					_serializedPipeline.AddElement (elements [i]);
				}
			}
			for (int i = 0; i < elements.Count; i++) {
				_serializedPipeline.UpdateConnections (elements[i]);
			}
		}
		/// <summary>
		/// Raises the after deserialize event.
		/// </summary>
		public void OnAfterDeserialize () {
			elements.Clear ();
			idToElement.Clear ();
			keyNameToPipelineElement.Clear ();
			// Fill elements.
			IEnumerable<PipelineElement> pipelineElements = _serializedPipeline.GetElements ();
			var pipelineElementsEnumerator = pipelineElements.GetEnumerator ();
			while (pipelineElementsEnumerator.MoveNext ()) {
				AddElement (pipelineElementsEnumerator.Current);
			}
			// Make connections.
			for (int i = 0; i < elements.Count; i++) {
				if (elements[i].connectionType == PipelineElement.ConnectionType.Source ||
					elements[i].connectionType == PipelineElement.ConnectionType.Transform) {
					elements[i].sinkElement = GetElementById (elements[i].sinkElementId);
				}
				if (elements[i].connectionType == PipelineElement.ConnectionType.Sink ||
					elements[i].connectionType == PipelineElement.ConnectionType.Transform) {
					elements[i].srcElement = GetElementById (elements[i].srcElementId);
				}
			}
			BuildKeyNameIndex ();
			
			// Check for pipelines ending on  Positioner element, adds a Bakes elements at the end of the pipeline.
			/*
			Validate ();
			PipelineElement lastElement = root;
			while (lastElement.sinkElement != null) {
				lastElement = lastElement.sinkElement;
			}
			if (lastElement.classType == PipelineElement.ClassType.Positioner) {
				BakerElement bakerElement = new BakerElement ();
				AddElement (bakerElement);
				lastElement.sinkElement = bakerElement;
				lastElement.sinkElementId = bakerElement.id;
				bakerElement.srcElement = lastElement;
				bakerElement.srcElementId = lastElement.id;
				AddPipelineElementToIndex (bakerElement);
			}
			*/
		}
		#endregion

		#region Validation
		/// <summary>
		/// Raises the validate event.
		/// </summary>
		void OnValidate(){
			var self = (ISerializationCallbackReceiver) this;
			self.OnAfterDeserialize();
		}
		/// <summary>
		/// Check if there is at least one and no more than one series of
		/// connected nodes from source to sink elements.
		/// If the pipeline is valid, then the 'root' variable points to
		/// the source element of the pipeline.
		/// If the validation was not successful then a code is set for 
		/// the validation error that was found, accesible through the 
		/// 'status' variable.
		/// </summary>
		public bool Validate () {
			if (elements.Count > 0) {
				validPipelines = 0;
				srcElements.Clear ();
				for (int i = 0; i < elements.Count; i++) {
					elements[i].isOnValidPipeline = false;
					elements[i].Validate ();
					if (elements[i].connectionType == PipelineElement.ConnectionType.Source) {
						srcElements.Add (elements[i]);
					}
				}
				if (srcElements.Count == 0) {
					root = null;
					state = State.NoSourceElement;
				} else {
					for (int i = 0; i < srcElements.Count; i++) {
						if (validPipelines == 0)
							root = srcElements[i];
						checkedElementsOnValidation.Clear ();
						if (!CheckElementConnection (srcElements[i])) {
							return false;
						}
					}
					if (validPipelines == 0) {
						state = State.NoSinkElement;
					} else if (validPipelines > 1) {
						state = State.MultiplePipelines;
					} else {
						state = State.Valid;
						SetElementsOnValidPipeline (root);
						return true;
					}
				}
				srcElements.Clear ();
			} else {
				root = null;
				state = State.Empty;
			}
			return false;
		}
		/// <summary>
		/// Determines whether this instance is valid.
		/// </summary>
		/// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
		public bool IsValid () {
			return state == State.Valid;
		}
		/// <summary>
		/// Recursive function to traverse the pipeline searching for complete
		/// connections between pipeline elements.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		private bool CheckElementConnection (PipelineElement pipelineElement, PipelineElement previousElement = null) {
			//Validation against cyclic pipelines
			if (!checkedElementsOnValidation.Contains (pipelineElement.id)) {
				checkedElementsOnValidation.Add (pipelineElement.id);
			} else {
				state = State.InvalidConnection;
				root = null;
				return false;
			}
			if (pipelineElement.connectionType == PipelineElement.ConnectionType.Sink) {
				validPipelines++;
			}
			if (pipelineElement.sinkElement != null) {
				if (previousElement != null &&
				    pipelineElement.positionWeight == previousElement.positionWeight &&
				    pipelineElement.uniqueOnPipeline) {
					state = State.MultiElement;
					root = null;
					return false;
				}
				if (pipelineElement.positionWeight > pipelineElement.sinkElement.positionWeight) {
					// Invalid connection
					state = State.InvalidConnection;
					root = null;
					return false;
				}
				return CheckElementConnection (pipelineElement.sinkElement, pipelineElement);
			}
			return true;
		}
		/// <summary>
		/// Set the elements on a valid pipeline; this means the element is connected
		/// in a valid processing stream.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element to set the pipeline to.</param>
		private void SetElementsOnValidPipeline (PipelineElement rootElement) {
			rootElement.isOnValidPipeline = true;
			if ((rootElement.connectionType == PipelineElement.ConnectionType.Transform ||
				rootElement.connectionType == PipelineElement.ConnectionType.Source) &&
				rootElement.sinkElement != null) {
				SetElementsOnValidPipeline (rootElement.sinkElement);
			}
		}
		/// <summary>
		/// Builds the index of the identifier to element.
		/// </summary>
		public void BuildIdToElementIndex () {
			idToElement.Clear ();
			for (int i = 0; i < elements.Count; i++) {
				if (!idToElement.ContainsKey (elements[i].id)) {
					idToElement.Add (elements[i].id, elements[i]);
				} else {
					elements[i].id = -1;
					elements[i].isMarkedForDeletion = true;
				}
			}
		}
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		public Pipeline Clone (Pipeline clone = null) {
			if (clone == null) {
				clone = ScriptableObject.CreateInstance<Pipeline> ();
			} else {
				clone.elements.Clear ();
				clone.sproutGroups.Clear ();
			}
			// Relation between current objects IDs and clones. To recreate connections.
			Dictionary<int, PipelineElement> instanceIDs = new Dictionary<int, PipelineElement> ();
			for (int i = 0; i < elements.Count; i++) {
				PipelineElement clonedElement = elements[i].Clone();
				clone.elements.Add (clonedElement);
				instanceIDs.Add (elements[i].GetInstanceID (), clonedElement);
			}
			idToElement.Clear ();

			// Stablish connections between elements.
			for (int i = 0; i < elements.Count; i++) {
				if (elements[i].srcElement != null) {
					instanceIDs [elements[i].GetInstanceID ()].srcElement = 
						instanceIDs [elements[i].srcElement.GetInstanceID ()];
				}
				if (elements[i].sinkElement != null) {
					instanceIDs [elements[i].GetInstanceID ()].sinkElement = 
						instanceIDs [elements[i].sinkElement.GetInstanceID ()];
				}
				if (elements[i].id >= 0) {
					idToElement.Add (elements[i].id, elements[i]);
				}
			}

			// Sprout groups.
			List<SproutGroups.SproutGroup> childrenSproutGroups = sproutGroups.GetSproutGroups ();
			for (int i = 0; i < childrenSproutGroups.Count; i++) {
				clone.sproutGroups.AddSproutGroup (childrenSproutGroups[i].Clone ());
			}

			clone.BuildIdToElementIndex ();
			clone.BuildKeyNameIndex ();

			// Tree factory preferences.
			clone.treeFactoryPreferences = treeFactoryPreferences.Clone ();
			clone.isCatalogItem = isCatalogItem;
			clone.sproutGroups.BuildIndexes ();
			clone.sproutGroups.BuildPopupOptions ();
			clone.undoControl = new UndoControl ();
			return clone;
		}
		#endregion

		#region Query
		/// <summary>
		/// Rebuild the key name indexing of element on this pipeline.
		/// </summary>
		public void BuildKeyNameIndex () {
			keyNameToPipelineElement.Clear ();
			for (int i = 0; i < elements.Count; i++) {
				AddPipelineElementToIndex (elements [i]);
			}
		}
		/// <summary>
		/// Add element to the key name dictionary.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		void AddPipelineElementToIndex (PipelineElement pipelineElement) {
			if (pipelineElement.hasKeyName && 
				!keyNameToPipelineElement.ContainsKey (pipelineElement.keyName)) {
					keyNameToPipelineElement.Add (pipelineElement.keyName, pipelineElement);
			}
		}
		/// <summary>
		/// Removes an element from the key name dictionary.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		void RemovePipelineElementFromIndex (PipelineElement pipelineElement) {
			if (keyNameToPipelineElement.ContainsKey (pipelineElement.keyName)) {
				keyNameToPipelineElement.Remove (pipelineElement.keyName);
			}
		}
		/// <summary>
		/// Gets an element from this pipeline given its key name.
		/// </summary>
		/// <param name="keyName">Key name of the element.</param>
		/// <returns>PipelineElement instane if found, otherwise null.</returns>
		public PipelineElement GetElementByKeyName (string keyName) {
			if (keyNameToPipelineElement.ContainsKey (keyName)) {
				return keyNameToPipelineElement [keyName];
			}
			return null;
		}
		/// <summary>
		/// Replaces elements on a pipeline given their key names, if both of them are found
		/// and are of the same type.
		/// </summary>
		/// <param name="targetKeyName">Target element key name.</param>
		/// <param name="replacementKeyName">Element key name to repace the target.</param>
		/// <returns>True if the elements where found, compatible and replaced, otherwise false.</returns>
		public bool ReplaceElements (string targetKeyName, string replacementKeyName) {
			if (keyNameToPipelineElement.ContainsKey (targetKeyName) &&
				keyNameToPipelineElement.ContainsKey (replacementKeyName))
			{
				PipelineElement targetElement = keyNameToPipelineElement [targetKeyName];
				PipelineElement replacementElement = keyNameToPipelineElement [replacementKeyName];
				if (targetElement.classType == replacementElement.classType) {
					PipelineElement targetSrcElement = targetElement.srcElement;
					PipelineElement targetSinkElement = targetElement.sinkElement;
					PipelineElement replacementSrcElement = replacementElement.srcElement;
					PipelineElement replacementSinkElement = replacementElement.sinkElement;
					targetElement.srcElement = null;
					targetElement.sinkElement = null;
					replacementElement.srcElement = null;
					replacementElement.sinkElement = null;

					if (targetSrcElement != null) {
						targetSrcElement.sinkElement = replacementElement;
						replacementElement.srcElement = targetSrcElement;
					}
					if (targetSinkElement != null) {
						targetSinkElement.srcElement = replacementElement;
						replacementElement.sinkElement = targetSinkElement;
					}

					if (replacementSrcElement != null) {
						replacementSrcElement.sinkElement = targetElement;
						targetElement.srcElement = replacementSrcElement;
					}
					if (replacementSinkElement != null) {
						replacementSinkElement.srcElement = targetElement;
						targetElement.sinkElement = replacementSinkElement;
					}
					return true;
				}
			}
			return false;
		}
		#endregion
	}
}