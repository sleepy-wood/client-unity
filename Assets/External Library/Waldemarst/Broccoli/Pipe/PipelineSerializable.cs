using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Pipeline serializable class.
	/// </summary>
	[System.Serializable]
	public class PipelineSerializable {
		#region Elements
		public List<StructureGeneratorElement> structureGenerators = new List<StructureGeneratorElement> ();
		public List<LSystemElement> lSystemElements = new List<LSystemElement> ();
		public List<LengthTransformElement> lengthTransforms = new List<LengthTransformElement> ();
		public List<BranchBenderElement> branchBenders = new List<BranchBenderElement> ();
		public List<GirthTransformElement> girthTransforms = new List<GirthTransformElement> ();
		public List<SparsingTransformElement> sparsingTransforms = new List<SparsingTransformElement> ();
		public List<BranchMeshGeneratorElement> branchMeshGenerators = new List<BranchMeshGeneratorElement> ();
		public List<TrunkMeshGeneratorElement> trunkMeshGenerators = new List<TrunkMeshGeneratorElement> ();
		public List<BranchMapperElement> barkMappers = new List<BranchMapperElement> ();
		public List<ProceduralBranchMapperElement> proceduralBarkMappers = new List<ProceduralBranchMapperElement> ();
		public List<SproutMapperElement> sproutMappers = new List<SproutMapperElement> ();
		public List<SproutGeneratorElement> sproutGenerators = new List<SproutGeneratorElement> ();
		public List<SproutMeshGeneratorElement> sproutMeshGenerators = new List<SproutMeshGeneratorElement> ();
		public List<WindEffectElement> windEffects = new List<WindEffectElement> ();
		public List<PositionerElement> positioners = new List<PositionerElement> ();
		public List<BakerElement> bakers = new List<BakerElement> ();
		#endregion

		#region Public Methods
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.PipelineSerializable"/> class.
		/// </summary>
		public PipelineSerializable () { }
		/// <summary>
		/// Adds an element to the serializable class.
		/// </summary>
		/// <param name="element">Element.</param>
		public void AddElement (PipelineElement element) {
			switch (element.classType) {
			case PipelineElement.ClassType.StructureGenerator:
				structureGenerators.Add ((StructureGeneratorElement)element);
				element.index = lSystemElements.Count - 1;
				break;
			case PipelineElement.ClassType.LSystem:
				lSystemElements.Add ((LSystemElement)element);
				element.index = lSystemElements.Count - 1;
				break;
			case PipelineElement.ClassType.BranchMeshGenerator:
				branchMeshGenerators.Add ((BranchMeshGeneratorElement)element);
				element.index = branchMeshGenerators.Count - 1;
				break;
			case PipelineElement.ClassType.TrunkMeshGenerator:
				trunkMeshGenerators.Add ((TrunkMeshGeneratorElement)element);
				element.index = trunkMeshGenerators.Count - 1;
				break;
			case PipelineElement.ClassType.GirthTransform:
				girthTransforms.Add ((GirthTransformElement)element);
				element.index = girthTransforms.Count - 1;
				break;
			case PipelineElement.ClassType.BranchBender:
				branchBenders.Add ((BranchBenderElement)element);
				element.index = branchBenders.Count - 1;
				break;
			case PipelineElement.ClassType.SparsingTransform:
				sparsingTransforms.Add ((SparsingTransformElement)element);
				element.index = sparsingTransforms.Count - 1;
				break;
			case PipelineElement.ClassType.LengthTransform:
				lengthTransforms.Add ((LengthTransformElement)element);
				element.index = lengthTransforms.Count - 1;
				break;
			case PipelineElement.ClassType.BranchMapper:
				barkMappers.Add ((BranchMapperElement)element);
				element.index = barkMappers.Count - 1;
				break;
			case PipelineElement.ClassType.ProceduralBranchMapper:
				proceduralBarkMappers.Add ((ProceduralBranchMapperElement)element);
				element.index = proceduralBarkMappers.Count - 1;
				break;
			case PipelineElement.ClassType.SproutGenerator:
				sproutGenerators.Add ((SproutGeneratorElement)element);
				element.index = sproutGenerators.Count - 1;
				break;
			case PipelineElement.ClassType.SproutMeshGenerator:
				sproutMeshGenerators.Add ((SproutMeshGeneratorElement)element);
				element.index = sproutMeshGenerators.Count - 1;
				break;
			case PipelineElement.ClassType.SproutMapper:
				sproutMappers.Add ((SproutMapperElement)element);
				element.index = sproutMappers.Count - 1;
				break;
			case PipelineElement.ClassType.WindEffect:
				windEffects.Add ((WindEffectElement)element);
				element.index = windEffects.Count - 1;
				break;
			case PipelineElement.ClassType.Positioner:
				positioners.Add ((PositionerElement)element);
				element.index = positioners.Count - 1;
				break;
			case PipelineElement.ClassType.Baker:
				bakers.Add ((BakerElement)element);
				element.index = bakers.Count - 1;
				break;
			default:
				element.index = -1;
				break;
			}
		}
		/// <summary>
		/// Updates connerction references on a pipeline element.
		/// </summary>
		/// <param name="element">Element.</param>
		public void UpdateConnections (PipelineElement element) {
			if (element.srcElement == null) {
				element.srcElementIndex = -1;
			} else {
				element.srcElementIndex = element.srcElement.index;
				element.srcElementClassType = element.srcElement.classType;
			}
			if (element.sinkElement == null) {
				element.sinkElementIndex = -1;
			} else {
				element.sinkElementIndex = element.sinkElement.index;
				element.sinkElementClassType = element.sinkElement.classType;
			}
		}
		/// <summary>
		/// Gets an element according to its class type.
		/// </summary>
		/// <returns>The element.</returns>
		/// <param name="index">Index of the element.</param>
		/// <param name="classType">Class type.</param>
		public PipelineElement GetElement(int index, PipelineElement.ClassType classType) {
			if (index >= 0) {
				switch (classType) {
				case PipelineElement.ClassType.Base:
					return null;
				case PipelineElement.ClassType.StructureGenerator:
					return (index < structureGenerators.Count ? structureGenerators [index] : null);
				case PipelineElement.ClassType.LSystem:
					return (index < lSystemElements.Count ? lSystemElements [index] : null);
				case PipelineElement.ClassType.BranchMeshGenerator:
					return (index < branchMeshGenerators.Count ? branchMeshGenerators [index] : null);
				case PipelineElement.ClassType.TrunkMeshGenerator:
					return (index < trunkMeshGenerators.Count ? trunkMeshGenerators [index] : null);
				case PipelineElement.ClassType.GirthTransform:
					return (index < girthTransforms.Count ? girthTransforms [index] : null);
				case PipelineElement.ClassType.BranchBender:
					return (index < branchBenders.Count ? branchBenders [index] : null);
				case PipelineElement.ClassType.SparsingTransform:
					return (index < sparsingTransforms.Count ? sparsingTransforms [index] : null);
				case PipelineElement.ClassType.LengthTransform:
					return (index < lengthTransforms.Count ? lengthTransforms [index] : null);
				case PipelineElement.ClassType.BranchMapper:
					return (index < barkMappers.Count ? barkMappers [index] : null);
				case PipelineElement.ClassType.ProceduralBranchMapper:
					return (index < proceduralBarkMappers.Count ? proceduralBarkMappers [index] : null);
				case PipelineElement.ClassType.SproutGenerator:
					return (index < sproutGenerators.Count ? sproutGenerators [index] : null);
				case PipelineElement.ClassType.SproutMeshGenerator:
					return (index < sproutMeshGenerators.Count ? sproutMeshGenerators [index] : null);
				case PipelineElement.ClassType.SproutMapper:
					return (index < sproutMappers.Count ? sproutMappers [index] : null);
				case PipelineElement.ClassType.WindEffect:
					return (index < windEffects.Count ? windEffects [index] : null);
				case PipelineElement.ClassType.Positioner:
					return (index < positioners.Count ? positioners [index] : null);
				case PipelineElement.ClassType.Baker:
					return (index < bakers.Count ? bakers [index] : null);
				}
			}
			return null;
		}
		/// <summary>
		/// Gets the elements on the pipeline.
		/// </summary>
		/// <returns>The elements.</returns>
		public IEnumerable<PipelineElement> GetElements() {
			int i;
			for (i = 0; i < structureGenerators.Count; i++) {
				yield return structureGenerators[i];
			}
			for (i = 0; i < lSystemElements.Count; i++) {
				yield return lSystemElements[i];
			}
			for (i = 0; i < branchMeshGenerators.Count; i++) {
				yield return branchMeshGenerators[i];
			}
			for (i = 0; i < trunkMeshGenerators.Count; i++) {
				yield return trunkMeshGenerators[i];
			}
			for (i = 0; i < girthTransforms.Count; i++) {
				yield return girthTransforms[i];
			}
			for (i = 0; i < branchBenders.Count; i++) {
				yield return branchBenders[i];
			}
			for (i = 0; i < sparsingTransforms.Count; i++) {
				yield return sparsingTransforms[i];
			}
			for (i = 0; i < lengthTransforms.Count; i++) {
				yield return lengthTransforms[i];
			}
			for (i = 0; i < barkMappers.Count; i++) {
				yield return barkMappers[i];
			}
			for (i = 0; i < proceduralBarkMappers.Count; i++) {
				yield return proceduralBarkMappers[i];
			}
			for (i = 0; i < sproutGenerators.Count; i++) {
				yield return sproutGenerators[i];
			}
			for (i = 0; i < sproutMeshGenerators.Count; i++) {
				yield return sproutMeshGenerators[i];
			}
			for (i = 0; i < sproutMappers.Count; i++) {
				yield return sproutMappers[i];
			}
			for (i = 0; i < windEffects.Count; i++) {
				yield return windEffects[i];
			}
			for (i = 0; i < positioners.Count; i++) {
				yield return positioners[i];
			}
			for (i = 0; i < bakers.Count; i++) {
				yield return bakers[i];
			}
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear () {
			structureGenerators.Clear ();
			lSystemElements.Clear ();
			branchMeshGenerators.Clear ();
			trunkMeshGenerators.Clear ();
			girthTransforms.Clear ();
			branchBenders.Clear ();
			sparsingTransforms.Clear ();
			lengthTransforms.Clear ();
			barkMappers.Clear ();
			proceduralBarkMappers.Clear ();
			sproutMappers.Clear ();
			sproutGenerators.Clear ();
			sproutMeshGenerators.Clear ();
			windEffects.Clear ();
			positioners.Clear ();
			bakers.Clear ();
		}
		#endregion
	}
}