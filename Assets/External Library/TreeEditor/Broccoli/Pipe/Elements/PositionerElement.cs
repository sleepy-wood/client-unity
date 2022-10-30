using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Element to position trees created by the factory.
	/// </summary>
	[System.Serializable]
	public class PositionerElement : PipelineElement {
		#region Vars
		/// <summary>
		/// Gets the type of the connection.
		/// </summary>
		/// <value>The type of the connection.</value>
		public override ConnectionType connectionType {
			get { return PipelineElement.ConnectionType.Transform; }
		}
		/// <summary>
		/// Gets the type of the element.
		/// </summary>
		/// <value>The type of the element.</value>
		public override ElementType elementType {
			get { return PipelineElement.ElementType.Positioner; }
		}
		/// <summary>
		/// Gets unique class type of the element.
		/// </summary>
		/// <value>The type of the class.</value>
		public override ClassType classType {
			get { return PipelineElement.ClassType.Positioner; }
		}
		/// <summary>
		/// Value used to position elements in the pipeline. The greater the more towards the end of the pipeline.
		/// </summary>
		/// <value>The position weight.</value>
		public override int positionWeight {
			get { return PipelineElement.effectWeight + 10; }
		}
		/// <summary>
		/// If true then a list of positions is used for the trees.
		/// </summary>
		public bool useCustomPositions = false;
		/// <summary>
		/// List of positions.
		/// </summary>
		public List<Position> positions = new List<Position> ();
		/// <summary>
		/// The index on of the selected position.
		/// </summary>
		//[System.NonSerialized]
		public int selectedPositionIndex = -1;
		/// <summary>
		/// The default position.
		/// </summary>
		static Position defaultPosition = new Position ();
		/// <summary>
		/// Temp variable to save enabled positions when requesting one.
		/// </summary>
		List<Position> enabledPositions = new List<Position> ();
		/// <summary>
		/// Option to add a collision object at trunk level.
		/// </summary>
		public bool addCollisionObjectAtTrunk = false;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.PositionerElement"/> class.
		/// </summary>
		public PositionerElement () {}
		#endregion

		#region Validation
		/// <summary>
		/// Validate this instance.
		/// </summary>
		public override bool Validate () {
			log.Clear ();
			if (useCustomPositions) {
				if (useCustomPositions && positions.Count == 0) {
					log.Enqueue (LogItem.GetWarnItem ("Custom positions is enabled but the list of positions is empty."));
				} else {
					bool allDisabled = true;
					for (int i = 0; i < positions.Count; i++) {
						if (positions[i].enabled) {
							allDisabled = false;
							break;
						}
					}
					if (allDisabled) {
						log.Enqueue (LogItem.GetWarnItem ("Custom positions is enabled but all positions on the list are disabled."));
					}
				}
			}
			return true;
		}
		/// <summary>
		/// Determines whether this instance has any valid position.
		/// </summary>
		/// <returns><c>true</c> if this instance has any valid position; otherwise, <c>false</c>.</returns>
		public bool HasValidPosition () {
			for (int i = 0; i < positions.Count; i++) {
				if (positions[i].enabled)
					return true;
			}
			return false;
		}
		#endregion

		#region Position
		/// <summary>
		/// Gets a position either from a list of custom positions or a default one.
		/// </summary>
		/// <returns>The position.</returns>
		public Position GetPosition () {
			Position position;
			if (useCustomPositions) {
				enabledPositions.Clear ();
				for (int i = 0; i < positions.Count; i++) {
					if (positions [i].enabled)
						enabledPositions.Add (positions [i]);
				}
			}
			if (enabledPositions.Count > 0) {
				position = enabledPositions [Random.Range(0, enabledPositions.Count)];
				enabledPositions.Clear ();
			} else {
				position = defaultPosition;
			}
			return position;
		}
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		override public PipelineElement Clone() {
			PositionerElement clone = ScriptableObject.CreateInstance<PositionerElement> ();
			SetCloneProperties (clone);
			clone.useCustomPositions = true;
			for (int i = 0; i < positions.Count; i++) {
				clone.positions.Add (positions[i].Clone ());
			}
			clone.selectedPositionIndex = selectedPositionIndex;
			clone.addCollisionObjectAtTrunk = addCollisionObjectAtTrunk;
			return clone;
		}
		#endregion
	}
}