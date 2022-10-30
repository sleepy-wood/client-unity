using System.Collections.Generic;

using Broccoli.Pipe;
using Broccoli.Component;

namespace Broccoli.Manager
{
	/// <summary>
	/// Component manager.
	/// </summary>
	public class ComponentManager {
		#region Vars
		/// <summary>
		/// Contains the components managed by this instance.
		/// </summary>
		Dictionary <int, TreeFactoryComponent> components = 
			new Dictionary <int, TreeFactoryComponent> ();
		/// <summary>
		/// Used to keep track of components relevant to the pipeline.
		/// </summary>
		List<int> keepAliveComponents = new List<int> ();
		/// <summary>
		/// Ids for the components to delete.
		/// </summary>
		List<int> toDeleteComponentIds = new List<int> ();
		#endregion

		#region Management
		/// <summary>
		/// Get a factory component assigned to a pipeline element.
		/// </summary>
		/// <returns>The factory component.</returns>
		/// <param name="pipelineElement">Pipeline element.</param>
		public TreeFactoryComponent GetFactoryComponent (PipelineElement pipelineElement) {
			TreeFactoryComponent factoryComponent;
			if (components.ContainsKey (pipelineElement.GetInstanceID ())) {
				factoryComponent = components [pipelineElement.GetInstanceID ()];
			} else {
				factoryComponent = CreateFactoryComponent (pipelineElement);
				components.Add (pipelineElement.GetInstanceID (), factoryComponent);
			}
			keepAliveComponents.Add (pipelineElement.GetInstanceID ());
			return factoryComponent;
		}
		/// <summary>
		/// Determines whether this instance has a factory component assigned to a specified pipeline element.
		/// </summary>
		/// <returns><c>true</c> if this instance has factory component the specified pipelineElement; 
		/// otherwise, <c>false</c>.</returns>
		/// <param name="pipelineElement">Pipeline element.</param>
		public bool HasFactoryComponent (PipelineElement pipelineElement) {
			return components.ContainsKey (pipelineElement.GetInstanceID ());
		}
		/// <summary>
		/// Creates a tree factory component assigned to a pipeline element.
		/// </summary>
		/// <returns>The factory component.</returns>
		/// <param name="pipelineElement">Pipeline element.</param>
		TreeFactoryComponent CreateFactoryComponent (PipelineElement pipelineElement) {
			TreeFactoryComponent factoryComponent = null;
			switch (pipelineElement.classType) {
			case PipelineElement.ClassType.StructureGenerator:
				factoryComponent = new StructureGeneratorComponent ();
				break;
			case PipelineElement.ClassType.LSystem:
				factoryComponent = new LSystemComponent ();
				break;
			case PipelineElement.ClassType.BranchMeshGenerator:
				factoryComponent = new BranchMeshGeneratorComponent ();
				break;
			case PipelineElement.ClassType.TrunkMeshGenerator:
				factoryComponent = new TrunkMeshGeneratorComponent ();
				break;
			case PipelineElement.ClassType.GirthTransform:
				factoryComponent = new GirthTransformComponent ();
				break;
			case PipelineElement.ClassType.BranchBender:
				factoryComponent = new BranchBenderComponent ();
				break;
			case PipelineElement.ClassType.LengthTransform:
				factoryComponent = new LengthTransformComponent ();
				break;
			case PipelineElement.ClassType.SparsingTransform:
				factoryComponent = new SparsingTransformComponent ();
				break;
			case PipelineElement.ClassType.SproutGenerator:
				factoryComponent = new SproutGeneratorComponent ();
				break;
			case PipelineElement.ClassType.SproutMeshGenerator:
				factoryComponent = new SproutMeshGeneratorComponent ();
				break;
			case PipelineElement.ClassType.BranchMapper:
				factoryComponent = new BranchMapperComponent ();
				break;
			case PipelineElement.ClassType.ProceduralBranchMapper:
				factoryComponent = new ProceduralBranchMapperComponent ();
				break;
			case PipelineElement.ClassType.SproutMapper:
				factoryComponent = new SproutMapperComponent ();
				break;
			case PipelineElement.ClassType.WindEffect:
				factoryComponent = new WindEffectComponent ();
				break;
			case PipelineElement.ClassType.Positioner:
				factoryComponent = new PositionerComponent ();
				break;
			case PipelineElement.ClassType.Baker:
				factoryComponent = new BakerComponent ();
				break;
			}
			return factoryComponent;
		}
		/// <summary>
		/// Call clear on all the managed factory components.
		/// </summary>
		public void CallClearOnComponents () {
			var componentsEnumerator = components.GetEnumerator ();
			while (componentsEnumerator.MoveNext ()) {
				componentsEnumerator.Current.Value.Clear ();
			}
		}
		/// <summary>
		/// Clear all the components on this instance.
		/// </summary>
		public void Clear () {
			CallClearOnComponents ();
			components.Clear ();
		}
		#endregion

		#region Usage
		/// <summary>
		/// Begins usage on this instance, thus keeping track on
		/// those components relevant to the pipeline.
		/// </summary>
		public void BeginUsage () {
			keepAliveComponents.Clear ();
		}
		/// <summary>
		/// Ends usage on this instance and removes those components
		/// no longer relevant to the pipeline.
		/// </summary>
		public void EndUsage () {
			toDeleteComponentIds.Clear ();
			var componentsEnumerator = components.GetEnumerator ();
			while (componentsEnumerator.MoveNext ()) {
				if (!keepAliveComponents.Contains (componentsEnumerator.Current.Key)) {
					componentsEnumerator.Current.Value.Clear ();
					toDeleteComponentIds.Add (componentsEnumerator.Current.Key);
				}
			}
			for (int i = 0; i < toDeleteComponentIds.Count; i++) {
				components.Remove (toDeleteComponentIds[i]);
			}
			toDeleteComponentIds.Clear ();
		}
		#endregion
	}
}