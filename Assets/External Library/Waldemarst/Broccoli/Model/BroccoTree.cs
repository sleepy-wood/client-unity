using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Broccoli.Base;
using Broccoli.Utils;

/// <summary>
/// Classes for modeling entities composing a tree and the tree itself.
/// </summary>
namespace Broccoli.Model {
	/// <summary>
	/// Class representing a biological tree.
	/// </summary>
	[System.Serializable]
	public class BroccoTree {
		#region Class Sprout
		/// <summary>
		/// Class representing a sprout.
		/// </summary>
		[System.Serializable]
		public class Sprout {
			#region Vars
			/// <summary>
			/// Belonging group.
			/// </summary>
			public int groupId = 0;
			/// <summary>
			/// The index of the snapshot or area map.
			/// </summary>
			[System.NonSerialized]
			public int subgroupId = -1;
			/// <summary>
			/// Relative position on the parent branch. From 0 to 1.
			/// </summary>
			public float position = 1f;
			/// <summary>
			/// Position on the tree hierarchy. From 0 (base) to 1 (top branch tip).
			/// </summary>
			public float hierarchyPosition = 1f;
			/// <summary>
			/// Flag to mark the preference of using hierarchy position instead or in branch positioning.
			/// </summary>
			public bool preferHierarchyPosition = false;
			/// <summary>
			/// Gets the prefered position for the sprout. Either in-branch or in full hierarchy, depending
			/// on the preferHierarchyPosition flag value.
			/// </summary>
			/// <value>The prefered position.</value>
			public float preferedPosition {
				get { 
					if (preferHierarchyPosition)
						return hierarchyPosition;
					else
						return position;
				}
			}
			/// <summary>
			/// Angle around the branch circumference.
			/// </summary>
			public float rollAngle = 0f;
			/// <summary>
			/// Angle between the parent branch and the sprout.
			/// </summary>
			public float branchAlignAngle = 0f;
			/// <summary>
			/// Horizontal alignment relative to gravity.
			/// </summary>
			public float horizontalAlign = 0f;
			/// <summary>
			/// Alignment to look up againts the gravity vector.
			/// </summary>
			public float gravityAlign = 0f;
			/// <summary>
			/// Perpendicular alignment relative to branch direction.
			/// </summary>
			public float perpendicularAlign = 0f;
			/// <summary>
			/// Flip aligment towards a directional vector.
			/// </summary>
			public float flipAlign = 0f;
			/// <summary>
			/// Flip alignment direction.
			/// </summary>
			public Vector3 flipDirection = Vector3.up;
			/// <summary>
			/// If true the sprout comes from the center of the branch,
			/// instead of the surface of it.
			/// </summary>
			public bool fromBranchCenter = false;
			/// <summary>
			/// Parent branch.
			/// </summary>
			[System.NonSerialized]
			public Branch parentBranch;
			/// <summary>
			/// Position vector on the line of the branch..
			/// </summary>
			[System.NonSerialized]
			public Vector3 inBranchPosition = Vector3.zero;
			/// <summary>
			/// Position vector on the surface of the branch mesh.
			/// </summary>
			[System.NonSerialized]
			public Vector3 inGirthPosition = Vector3.zero;
			/// <summary>
			/// The sprout direction.
			/// </summary>
			[System.NonSerialized]
			public Vector3 sproutDirection = Vector3.zero;
			/// <summary>
			/// The sprout normal.
			/// </summary>
			[System.NonSerialized]
			public Vector3 sproutNormal = Vector3.zero;
			/// <summary>
			/// The sprout forward.
			/// </summary>
			public Vector3 forward = Vector3.zero;
			/// <summary>
			/// Offset of the branch from its center.
			/// </summary>
			public Vector3 positionOffset = Vector3.zero;
			/// <summary>
			/// Saves the id of te structure level that generated this sprout.
			/// </summary>
			[System.NonSerialized]
			public int helperStructureLevelId = -1;
			/// <summary>
			/// Saves the id of the seed that generated this sprout.
			/// </summary>
			[System.NonSerialized]
			public int helperSeedId = -1;
			/// <summary>
			/// Temporal id for the sprout, assigned when requested by process
			/// that need to match each sprout with a particular data structure.
			/// Note this id is non persistent or may not 
			/// be set depending on the context.
			/// </summary>
			[System.NonSerialized]
			public int helperSproutId = -1;
			/// <summary>
			/// The mesh length for this sprout, if it has no mesh then 0.
			/// </summary>
			[System.NonSerialized]
			public float meshHeight = 0f;
			#endregion

			#region Ops
			/// <summary>
			/// Calculates positions relative to the parent branch.
			/// </summary>
			public void CalculateVectors (Branch referenceBranch = null, bool isBranch = false) {
				if (referenceBranch == null)
					referenceBranch = parentBranch;
				if (referenceBranch != null) {
					// Set inBranchPosition.
					inBranchPosition = referenceBranch.GetPointAtPosition (position);

					// Set inGirthPosition.
					if (fromBranchCenter) {
						inGirthPosition = inBranchPosition;
					} else {
						//float girthAtPosition = referenceBranch.GetGirthAtPosition (position);
						/*
						Vector3 toGirthNormal = Quaternion.AngleAxis (rollAngle * Mathf.Rad2Deg + 90f, referenceBranch.GetDirectionAtPosition (position)) *
						                        referenceBranch.GetNormalAtPosition (position);
												*/
						//inGirthPosition = inBranchPosition + (toGirthNormal.normalized * girthAtPosition);
						inGirthPosition = inBranchPosition + positionOffset * 0.8f;
					}

					Vector3 referenceBranchNormal;
					Vector3 referenceBranchDirection;

					//if (isBranch) {
						referenceBranchNormal = referenceBranch.GetNormalAtPosition (position);
						referenceBranchDirection = referenceBranch.GetDirectionAtPosition (position);
					//}

					if (referenceBranch != null && referenceBranch.parentTree != null) {
						hierarchyPosition = (referenceBranch.GetHierarchyLevel () + position) / referenceBranch.parentTree.GetOffspringLevel ();
					}

					// Direction
					Vector3 result;
					if (isBranch) {
						result = Quaternion.AngleAxis(branchAlignAngle * Mathf.Rad2Deg, Vector3.left) * Vector3.forward;
					} else {
						result = Quaternion.AngleAxis(branchAlignAngle * Mathf.Rad2Deg, Vector3.forward) * Vector3.right;	
					}
					//Vector3 result = Quaternion.AngleAxis(branchAlignAngle * Mathf.Rad2Deg, Vector3.forward) * Vector3.right;
					//Vector3 result = Quaternion.AngleAxis(branchAlignAngle * Mathf.Rad2Deg, Vector3.left) * Vector3.forward;
					result = Quaternion.AngleAxis(rollAngle * Mathf.Rad2Deg, GlobalSettings.againstGravityDirection) * result;
					Quaternion rotation;
					if (referenceBranchNormal != Vector3.zero) {
						rotation = Quaternion.LookRotation (referenceBranchNormal, referenceBranchDirection);
					} else {
						rotation = Quaternion.Euler (Vector3.zero);
					}
					sproutDirection = (rotation * result).normalized;

					// Normal
					Vector3 resultN = Quaternion.AngleAxis(branchAlignAngle * Mathf.Rad2Deg, Vector3.forward) * GlobalSettings.againstGravityDirection;
					resultN = Quaternion.AngleAxis (rollAngle * Mathf.Rad2Deg, GlobalSettings.againstGravityDirection) * resultN;
					if (referenceBranchNormal != Vector3.zero) {
						Quaternion rotationN;
						if (referenceBranchNormal != Vector3.zero) {
							rotationN = Quaternion.LookRotation (referenceBranchNormal, referenceBranchDirection);
						} else {
							rotationN = Quaternion.Euler (Vector3.zero);
						}
						sproutNormal = rotationN * resultN;
					}
					sproutNormal = sproutNormal.normalized;

					// Forward
					//sproutForward = Quaternion.AngleAxis (rollAngle, referenceBranchDirection) * referenceBranchNormal;
					forward = Quaternion.AngleAxis (rollAngle * Mathf.Rad2Deg, referenceBranchDirection) * referenceBranchNormal;
					// TODO RE: simplify vector creation.

					// Horizontal align
					if (horizontalAlign > 0) {
						Vector3 horizontalDirection = Vector3.ProjectOnPlane (sproutDirection, GlobalSettings.againstGravityDirection);
						if (horizontalDirection.magnitude == 0) {
							// TODO
						}
						Vector3 newSsproutDirection = Vector3.Lerp (sproutDirection, horizontalDirection, horizontalAlign);
						sproutNormal = Vector3.Lerp (sproutNormal, GlobalSettings.againstGravityDirection, horizontalAlign);
						sproutDirection = newSsproutDirection;
					}

					// Flip sprout align
					if (flipAlign > 0 && !isBranch) {
						Vector3 _flipDirection = Vector3.ProjectOnPlane (sproutDirection, flipDirection);
						if (_flipDirection.magnitude == 0) {
							// TODO
						}
						Vector3 newSsproutDirection = Vector3.Lerp (sproutDirection, _flipDirection, flipAlign);
						sproutNormal = Vector3.Lerp (sproutNormal, flipDirection, flipAlign);
						sproutDirection = newSsproutDirection;
					}

					// Gravity Align
					if (gravityAlign != 0) {
						Vector3 newSproutDirection = Vector3.Lerp (
							sproutDirection, 
							(gravityAlign > 0?GlobalSettings.againstGravityDirection:GlobalSettings.gravityDirection), 
							gravityAlign>0?gravityAlign:-gravityAlign);
						Quaternion gravityRotation = Quaternion.FromToRotation (sproutDirection, newSproutDirection);
						sproutNormal = gravityRotation * sproutNormal;
						sproutDirection = newSproutDirection;
					}
				}
			}
			#endregion

			#region Clone
			/// <summary>
			/// Clone this instance.
			/// </summary>
			public Sprout Clone () {
				Sprout clone = new Sprout ();
				clone.groupId = groupId;
				clone.position = position;
				clone.rollAngle = rollAngle;
				clone.branchAlignAngle = branchAlignAngle;
				clone.horizontalAlign = horizontalAlign;
				clone.gravityAlign = gravityAlign;
				clone.perpendicularAlign = perpendicularAlign;
				clone.flipAlign = flipAlign;
				clone.flipDirection = flipDirection;
				clone.fromBranchCenter = fromBranchCenter;
				return clone;
			}
			#endregion
		}
		#endregion

		#region Class Branch
		/// <summary>
		/// Class representing a branch.
		/// </summary>
		[System.Serializable]
		public class Branch {
			#region Vars
			/// <summary>
			/// Id for the branch.
			/// </summary>
			public int id = 0;
			/// <summary>
			/// Guid for the branch.
			/// </summary>
			public System.Guid guid {
				get { return curve.guid; }
			}
			/// <summary>
			/// Direction the branch is growing.
			/// </summary>
			public Vector3 direction = GlobalSettings.againstGravityDirection;
			public Vector3 forward = Vector3.forward;
			/// <summary>
			/// Position of the branch relative to its parent length.
			/// When at base position is 0, at tip is 1.
			/// </summary>
			[SerializeField]
			float _position = 0;
			/// <summary>
			/// Adds offset to the branch origin position.
			/// </summary>
			[SerializeField]
			Vector3 _positionOffset = Vector3.zero;
			/// <summary>
			/// Branch origin position in space relative to its parent branch or root.
			/// </summary>
			public Vector3 positionFromBranch = Vector3.zero;
			/// <summary>
			/// Branch origin position in space taking the tree root as reference origin.
			/// </summary>
			public Vector3 positionFromRoot = Vector3.zero;
			/// <summary>
			/// True for branches instances representing tree roots.
			/// </summary>
			public bool isRoot = false;
			/// <summary>
			/// True if this branch has a break point.false The branch does not generate offspring nor it is meshed after this point.
			/// </summary>
			public bool isBroken = false;
			/// <summary>
			/// The position for the break point if this branch is broken.
			/// </summary>
			public float breakPosition = 0.5f;
			#endregion

			#region Length Vars
			/// <summary>
			/// Saves the length of the branch at current time.
			/// </summary>
			float _length = 1f; // TODO: remove
			/// <summary>
			/// Flag to process length only once after the branch gets created.
			/// </summary>
			[SerializeField]
			bool _lengthProcessed = false;
			/// <summary>
			/// Expected length for the branch when it reaches max age.
			/// </summary>
			[SerializeField]
			float _maxLength = 1f;
			/// <summary>
			/// The minimum length factor used to get the actual branch length.
			/// </summary>
			[SerializeField]
			float _lengthFactor = 1f;
			#endregion

			#region Girth Vars
			/// <summary>
			/// Maximum girth expected at this branch.
			/// </summary>
			[SerializeField]
			float _maxGirth = 0.25f;
			/// <summary>
			/// Minimum girth expected at this branch.
			/// </summary>
			[SerializeField]
			float _minGirth = 0.05f;
			/// <summary>
			/// Multiplies the girth to this factor at the branch base.
			/// </summary>
			float _girthAtBaseFactor = 0;
			/// Multiplies the girth to this factor at the branch tip.
			float _girthAtTopFactor = 0;
			/// <summary>
			/// Curve for values between min and max girth.
			/// </summary>
			//public AnimationCurve girthCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
			public AnimationCurve girthCurve = null;
			/// <summary>
			/// Scale factor to multiply the final girth.
			/// </summary>
			public float girthScale = 1f;
			#endregion

			#region Object Vars
			/// <summary>
			/// Structural bezier curve for this branch.
			/// </summary>
			/// <returns></returns>
			public BezierCurve curve = new BezierCurve ();
			/// <summary>
			/// Reference to a branch continuing this one.
			/// </summary>
			[System.NonSerialized]
			public Branch followUp = null;
			/// <summary>
			/// Children branches.
			/// </summary>
			[System.NonSerialized]
			List<Branch> _branches = new List<Branch> ();
			/// <summary>
			/// Children leaves.
			/// </summary>
			List<Sprout> _sprouts = new List<Sprout> ();
			/// <summary>
			/// The level length growth factor.
			/// </summary>
			[SerializeField]
			float _levelLengthGrowthFactor = 0.9f; //TODO: remove
			/// <summary>
			/// Branch level on the tree offspring.
			/// </summary>
			int _level = 0;
			/// <summary>
			/// Branch hierarchy on the tree offspring.
			/// </summary>
			float _hierarchy = -1;
			/// <summary>
			/// Levels of offspring after this branch.
			/// </summary>
			int _offspringLevels= 0;
			[System.NonSerialized]
			public BroccoTree parentTree = null;
			/// <summary>
			/// Parent branch.
			/// </summary>
			[System.NonSerialized]
			Branch _parent = null;
			/// <summary>
			/// Id to the parent branch, if set, used to flat serialization.
			/// </summary>
			public int parentBranchId = -1;
			/// <summary>
			/// Flag to mark that this branch has been manually modified.
			/// </summary>
			public bool isTuned = false;
			/// <summary>
			/// True to branches belonging to the tree trunk.
			/// </summary>
			public bool isTrunk = false;
			public float rollAngle = 0f;
			#endregion

			#region Helper Vars
			public int helperStructureLevelId = -1;
			/// <summary>
			/// True when the branch requires length update.
			/// </summary>
			bool _lengthDirty = false;
			/// <summary>
			/// True when the branch requires position update.
			/// </summary>
			bool _positionDirty = false;
			/// <summary>
			/// True when the branch requires girth update.
			/// </summary>
			bool _girthDirty = false;
			/// <summary>
			/// True when new sprouts have been added and recalculation
			/// in needed.
			/// </summary>
			bool _sproutsDirty = false;
			#endregion

			#region Getters and Setters
			/// <summary>
			/// Girth expected when branch reaches age 1.
			/// </summary>
			/// <value>The max girth the branch reaches.</value>
			public float maxGirth {
				get { return this._maxGirth; }
				set {
					this._maxGirth = value;
					this._girthDirty = true;
				}
			}
			/// <summary>
			/// Girth expected when branch age is 0.
			/// </summary>
			/// <value>Minimal girth for the branch.</value>
			public float minGirth {
				get { return this._minGirth; }
				set {
					this._minGirth = value;
					this._girthDirty = true;
				}
			}
			/// <summary>
			/// Gets the median girth.
			/// </summary>
			/// <value>The median girth.</value>
			public float medianGirth {
				get { return (this._minGirth + this._maxGirth) / 2f; }
			}
			/*
			 * Current length of the branch.
			 */
			/// <summary>
			/// Length of the branch at current age.
			/// </summary>
			/// <value>Length of the branch.</value>
			public float length {
				get { return this.curve.length; }
			}
			/// <summary>
			/// Expected length of the branch at age 1.
			/// </summary>
			/// <value>Max length of branch.</value>
			public float maxLength {
				get { return _maxLength; }
				set {
					_maxLength = value;
					this._lengthDirty = true;
					this._positionDirty = true;
				}
			}
			/// <summary>
			/// Minimum length factor used to calculate branch length.
			/// </summary>
			/// <value>The minimum length factor.</value>
			public float lengthFactor {
				get { return _lengthFactor; }
				set {
					_lengthFactor = value;
					this._lengthDirty = true;
					this._positionDirty = true;
				}
			}
			/// <summary>
			/// Position within its parent branch, 0 is at root, 1 at the end.
			/// </summary>
			/// <value>The position of this branch relative to its parent.</value>
			public float position {
				get { return this._position; }
				set {
					this._position = value;
					if (_parent != null && _parent.followUp == this) {
						if (_position != 1f) {
							_parent.followUp = null;
							isTrunk = false;
							for (int i = 0; i < parentTree.branches.Count; i++) {
								if (_parent.branches[i].position == 1) {
									_parent.followUp = _parent.branches[i];
									branches[i].isTrunk = _parent.isTrunk;
									break;
								}
							}
						}
					}
					this._positionDirty = true;
				}
			}
			/// <summary>
			/// 
			/// </summary>
			/// <value></value>
			public Vector3 positionOffset {
				get { return this._positionOffset; }
				set {
					this._positionOffset = value;
					this._positionDirty = true;
				}
			}
			/// <summary>
			/// Accumulative length factor per branch level.
			/// Affects branch length only.
			/// </summary>
			/// <value>The level length growth factor.</value>
			public float levelLengthGrowthFactor {
				get { return this._levelLengthGrowthFactor; }
				set {
					this._levelLengthGrowthFactor = value;
				}
			}
			/// <summary>
			/// Children branches.
			/// </summary>
			/// <value>The branches.</value>
			public List<Branch> branches {
				get { return this._branches; }
			}
			/// <summary>
			/// Children leaves.
			/// </summary>
			/// <value>The leaves.</value>
			public List<Sprout> sprouts {
				get { return this._sprouts; }
			}
			/// <summary>
			/// Parent branch.
			/// </summary>
			/// <value>The parent of this branch.</value>
			public Branch parent {
				get { return _parent; }
			}
			/// <summary>
			/// Number of branch levels after this branch.
			/// </summary>
			/// <value>The offspring levels.</value>
			public int offspringLevels {
				get { return _offspringLevels; }
			}
			/// <summary>
			/// World position of the starting point of the branch.
			/// </summary>
			/// <value>The origin.</value>
			public Vector3 origin {
				get {
					//return obj.transform.position;
					//return obj.transform.localPosition;
					//return obj.transform.position - parentTree.obj.transform.position; // TODO remove bz
					return GetPointAtPosition (0);
				}
			}
			/// <summary>
			/// Wold position of the ending point of the branch.
			/// </summary>
			/// <value>The destination.</value>
			public Vector3 destination {
				get {
					return GetPointAtPosition (1.0f);
				}
			}
			public bool IsFollowUp () {
				if (parent != null && parent.followUp == this)
					return true;
				return false;
			}
			#endregion

			#region Constructor
			/// <summary>
			/// Class constructor.
			/// </summary>
			public Branch () {
				curve.resolution = 2;
				BezierNode nodeA = new BezierNode (Vector3.zero);
				BezierNode nodeB = new BezierNode (Vector3.one * 2);
				nodeA.handleStyle = BezierNode.HandleStyle.Auto;
				nodeB.handleStyle = BezierNode.HandleStyle.Auto;
				curve.AddNode (nodeA, false);
				curve.AddNode (nodeB, false);
				curve.Process ();
			}
			#endregion
			
			#region Events
			/// <summary>
			/// Raises the destroy event.
			/// </summary>
			public void OnDestroy () { }
			#endregion

			#region Length Methods
			/// <summary>
			/// Updates the length according to age and modifiers.
			/// </summary>
			public void UpdateLength (bool recursive = false) {
				if (!_lengthProcessed) {
					this._length = _maxLength * lengthFactor;
					this.curve.Last().position = curve.First().position + (direction.normalized * this._length);
					//if (isTuned) {
						_lengthProcessed = true;
					//}
				}
				//this.curve.ProcessAfterCurveChanged ();
				this.curve.Process ();
				if (recursive) {
					for (int i = 0; i < _branches.Count; i++) {
						_branches[i].UpdateLength (recursive);
					}
				}
			}
			#endregion

			#region Position Methods
			/// <summary>
			/// Gets a vector in that space between origin and destination, in local tree space or absolute space.
			/// </summary>
			/// <returns>Vector at requested position.</returns>
			/// <param name="position">Position between 0 and 1.</param>
			/// <param name="absolutePosition">If set to <c>true</c> absolute position.</param>
			/// <summary>
			public Vector3 GetPointAtPosition (float position, bool absolutePosition = false) {
				if (parentTree != null && absolutePosition) {
					return curve.GetPointAt (position).position + positionFromRoot - parentTree.obj.transform.position;
				} else {
					return curve.GetPointAt (position).position + positionFromRoot;
				}
				// TODO remove bz
				/*
				Vector3 point;
				if (_bendPoints.Count == 0) {
					point = obj.transform.position + (direction.normalized * _length * position);
				} else {
					Vector3 directionOffset = direction.normalized;
					//Vector3 pointOffset = obj.transform.position;
					Vector3 pointOffset = obj.transform.position;
					float consumedPosition = 0f;
					for (int i = 0; i < _bendPoints.Count; i++) {
						if (position < _bendPoints[i].position) {
							break;
						}
						pointOffset += (directionOffset.normalized * _length * (_bendPoints[i].position - consumedPosition));
						consumedPosition = _bendPoints[i].position;
						directionOffset = _bendPoints[i].direction;
					}
					point = pointOffset + (directionOffset.normalized * length * (position - consumedPosition));
				}
				if (parentTree == null || absolutePosition) {
					return point;
				} else {
					return point - parentTree.obj.transform.position;
				}
				*/
			}
			/// <summary>
			/// Gets a vector at requested length at origin.
			/// </summary>
			/// <returns>Vector at the requested length.</returns>
			/// <param name="length">Length from origin.</param>
			public Vector3 GetPointAtLength (float length) {
				if (this.length > 0)
					return GetPointAtPosition (length / this.length);
				else
					return this.origin;
			}
			/// <summary>
			/// Set the transform position for all children branches.
			/// </summary>
			public void UpdatePosition (Branch referenceBranch = null) {
				if (_parent != null) {
					if (_parent.followUp == this) {
						// If this branch is a follow up.
						Vector3 parentPosition = _parent.curve.GetPointAt(position).position;
						Vector3 branchPosition = curve.GetPointAt(0f).position;
						positionFromBranch = parentPosition - branchPosition;
						curve.spareNoiseOffsetAtFirstPoint = false;
					} else {
						// this branch is not a follow up.
						positionFromBranch = _parent.curve.GetPointAt(position).position - curve.GetPointAt(0f).position + _positionOffset;
					}
					positionFromRoot = _parent.positionFromRoot + positionFromBranch;
				} else if (referenceBranch != null) {
					positionFromBranch = referenceBranch.curve.GetPointAt(position).position + _positionOffset;
					positionFromRoot = referenceBranch.positionFromRoot + positionFromBranch;
				}
				for (int i = 0; i < _branches.Count; i++) {
					_branches[i].UpdatePosition ();
				}
			}
			#endregion

			#region Girth Methods
			/// <summary>
			/// Update the girth factors used to calculate values across the branch.
			/// </summary>
			public void UpdateGirth (bool recursive = false) {
				if (_parent == null) {
					int treeLevels = _level + _offspringLevels + 1;
					_girthAtBaseFactor = 0;
					_girthAtTopFactor = (1 / (float)treeLevels) * (_level + 1);
				} else {
					_girthAtBaseFactor = _parent.GetGirthFactorAt (_position);
					_girthAtTopFactor = ((1 - _girthAtBaseFactor) / (float)(_offspringLevels + 1)) + _girthAtBaseFactor;
					if (IsFollowUp ()) girthScale = parent.girthScale;
				}
				if (recursive) {
					for (int i = 0; i < _branches.Count; i++) {
						_branches[i].UpdateGirth (recursive);
					}
				}
			}
			/// <summary>
			/// Get the factor used to calculate girth.
			/// </summary>
			/// <returns>The <see cref="System.Single"/>Girth factor at position.</returns>
			/// <param name="position">Position on the branch between 0 and 1.</param>
			public float GetGirthFactorAt (float position) {
				return (_girthAtTopFactor - _girthAtBaseFactor) * position + _girthAtBaseFactor;
			}
			/// <summary>
			/// Gets the girth value at a given position.
			/// </summary>
			/// <returns>The girth at position.</returns>
			/// <param name="position">Position between 0 a 1.</param>
			public float GetGirthAtPosition (float position) {
				float girth = (_maxGirth - _minGirth) * GetGirthCurve().Evaluate(1f - GetGirthFactorAt (position)) + _minGirth;
				//float ageFactor = Mathf.Clamp(age, 0, _level + _offspringLevels);
				//return girth * ageFactor;
				return Mathf.Clamp (girth * (IsFollowUp()?parent.girthScale:girthScale), _minGirth, _maxGirth);
			}
			/// <summary>
			/// Gets the girth value at a given length from branch origin.
			/// </summary>
			/// <returns>The girth of the branch at a given length.</returns>
			/// <param name="length">Length.</param>
			public float GetGirthAtLength (float length) {
				if (this.length > 0)
					return GetGirthAtPosition (length / this.length);
				else
					return Mathf.Clamp (_girthAtBaseFactor * _maxGirth * (IsFollowUp()?parent.girthScale:girthScale), _minGirth, _maxGirth);
			}
			/// <summary>
			/// Get the girth curve used to interpolate between min and max girth.
			/// </summary>
			/// <returns>The girth curve.</returns>
			public AnimationCurve GetGirthCurve () {
				if (girthCurve == null) {
					if (_parent != null) {
						return _parent.GetGirthCurve ();
					} else {
						return AnimationCurve.Linear(0f, 0f, 1f, 1f);
					}
				} else {
					return girthCurve;
				}
			}
			#endregion

			#region Level Methods
			/// <summary>
			/// Gets the level of the branch at the tree. Zero means the branch has
			/// no parent, 1 means first branch and so on.
			/// </summary>
			/// <returns>The level.</returns>
			/// <param name="recalculate">If set to <c>true</c> recalculate.</param>
			public int GetLevel (bool recalculate = false) {
				if (recalculate || _level < 0) {
					if (_parent == null)
						_level = 0;
					else
						_level = _parent.GetLevel () + 1;
				}
				return _level;
			}
			/// <summary>
			/// Gets the hierarchy level (level + position).
			/// </summary>
			/// <returns>The hierarchy level.</returns>
			/// <param name="recalculate">If set to <c>true</c> recalculate.</param>
			public float GetHierarchyLevel (bool recalculate = false) {
				if (_hierarchy == -1) {
					recalculate = true;
				}
				if (recalculate) {
					if (_parent == null) {
						_hierarchy = 0f;
					} else {
						_hierarchy = _position + _parent.GetHierarchyLevel (recalculate);
					}
				}
				return _hierarchy;
			}
			/// <summary>
			/// Updates the follow up branch.
			/// </summary>
			/// <param name="recursive">If set to <c>true</c> recursive.</param>
			public void UpdateFollowUps (bool recursive = false) {
				followUp = null;
				for (int i = 0; i < _branches.Count; i++) {
					if (_branches[i].position == 1 && 
						(followUp == null || _branches[i].offspringLevels > followUp.offspringLevels)) {
						followUp = _branches[i];
						_branches[i].isTrunk = isTrunk;
					}
				}
				if (recursive) {
					for (int i = 0; i < _branches.Count; i++) {
						_branches[i].UpdateFollowUps (true);
					}
				}
			}
			#endregion

			#region Structure Methods
			/// <summary>
			/// Structural update this instance.
			/// </summary>
			/// <param name="force">If set to <c>true</c> force the update.</param>
			public void Update (bool force = false) {
				if (_lengthDirty || force) {
					UpdateLength ();
					_lengthDirty = false;
				}
				if (_positionDirty || force) {
					UpdatePosition ();
					_lengthDirty = false;
				}
				if (_girthDirty || force) {
					UpdateGirth ();
					_girthDirty = false;
				}
				for (int i = 0; i < _branches.Count; i++) {
					_branches[i].Update (force);
				}
				// Curve updating.
			}
			public void ResetDirection (Vector3 newDirection, bool recursive = false) {
				direction = newDirection;
				this._length = _maxLength * lengthFactor;
				this.curve.Last().position = curve.First().position + (direction.normalized * this._length);
				this.curve.Process ();
				if (recursive) {
					for (int i = 0; i < _branches.Count; i++) {
						_branches[i].Update (true);
					}
				}
			}
			/// <summary>
			/// Attaches a branch to the current branch.
			/// </summary>
			/// <param name="branch">Branch to attach.</param>
			public void AddBranch (BroccoTree.Branch branch) {
				branch.parentTree = parentTree;
				_branches.Add (branch);
				branch._parent = this;
				branch.UpdatePosition ();
				// Set followUp
				if (this.followUp == null && branch.position == 1) {
					this.followUp = branch;
					branch.isTrunk = isTrunk;
				}
				/*
				// Parent to transform
				if (branch.obj == null) {
					branch.obj = new GameObject ("branch"); // TODO bz: remove.
				}
				branch.obj.transform.SetParent (this.obj.transform);
				*/
				branch.GetLevel(true);

				_InternalOffspringReceivedBranch (branch, branch.offspringLevels + 1);
			}
			public void UpdateResolution (int resolutionSteps, bool recursive = false) {
				curve.resolution = resolutionSteps;
				curve.ComputeSamples ();
				if (recursive) {
					for (int i = 0; i < branches.Count; i++) {
						branches [i].UpdateResolution (resolutionSteps, true);
					}
				}
			}
			public void ClearSprouts () {
				_sprouts.Clear ();
			}
			/// <summary>
			/// Internal function to inform when a new branch is received.
			/// Updates offspringLevels.
			/// </summary>
			/// <param name="branch">Branch.</param>
			/// <param name="offspringLevel">Offspring level.</param>
			public void _InternalOffspringReceivedBranch (Branch branch, int offspringLevels) {
				//GetMaxChildrenLevel (true); //TODO: update offspring level
				if (offspringLevels > this._offspringLevels) {
					this._offspringLevels = offspringLevels;
				}
				if (this._parent != null) {
					this._parent._InternalOffspringReceivedBranch (branch, this._offspringLevels + 1);
				}
				OnOffspringReceivedBranch (branch);
			}
			/// <summary>
			/// Event to call when offspring branches receive a new branch.
			/// </summary>
			/// <param name="branch">Branch received.</param>
			public virtual void OnOffspringReceivedBranch (Branch branch) {}
			/// <summary>
			/// Return all the branches attached to this particular branch
			/// at any level.
			/// </summary>
			/// <returns>The descendant branches.</returns>
			public List<Branch> GetDescendantBranches () {
				List<Branch> children = new List<Branch> (this._branches);
				for (int i = 0; i < _branches.Count; i++) {
					children.AddRange (_branches[i].GetDescendantBranches ());
				}
				return children;
			}
			/// <summary>
			/// Recalculates the normals.
			/// </summary>
			public void RecalculateNormals () {
				RecalculateNormals (Vector3.zero, Vector3.zero);
			}
			/// <summary>
			/// Recalculates the normals.
			/// </summary>
			/// <param name="prevDirection">Previous direction.</param>
			/// <param name="prevNormal">Previous normal.</param>
			public void RecalculateNormals (Vector3 prevDirection, Vector3 prevNormal) {
				if (isTrunk) {
					curve.normalMode = BezierCurve.NormalMode.ReferenceVector;
					curve.referenceNormal = Vector3.forward;
				} else {
					curve.normalMode = BezierCurve.NormalMode.ReferenceVector;
					if (IsFollowUp ()) {
						CurvePoint parentLastPoint = _parent.curve.GetPointAt (1f, true);
						curve.referenceNormal = parentLastPoint.normal;
						curve.referenceForward = parentLastPoint.forward;
					} else {
						Vector3 forwardAtBase = curve.GetPointAt (0f, true).forward;
						curve.referenceNormal = Vector3.ProjectOnPlane (forwardAtBase, curve.referenceForward).normalized;
					}
					if (curve.referenceNormal == Vector3.zero) {
						curve.referenceNormal = Vector3.forward;
					}
				}
				curve.RecalculateNormals ();

				/* OLD CODE
				if (prevDirection == Vector3.zero) {
					// Get perpendicular vector to this direction to begin with.
					//float angle = Random.Range (0, Mathf.PI * 2f);
					float angle = segmentAngle * Mathf.Deg2Rad;
					// Generate a uniformly-distributed unit vector in the XY plane.
					Vector3 inPlane = new Vector3 (Mathf.Cos (angle), Mathf.Sin (angle), 0f);
					// Rotate the vector into the plane perpendicular to direction and return it.
					normal = (Quaternion.LookRotation (direction) * inPlane).normalized;
					//normal = Vector3.forward;
				} else {
					normal = Quaternion.FromToRotation(prevDirection, direction) * prevNormal;
				}
				prevDirection = direction;
				prevNormal = normal;
				if (_bendPoints.Count > 0) {
					for (int i = 0; i < _bendPoints.Count; i++) {
						_bendPoints[i].normal = Quaternion.FromToRotation(prevDirection, _bendPoints[i].direction) * prevNormal;
						prevDirection = _bendPoints[i].direction;
						prevNormal = _bendPoints[i].normal;
					}
				}
				*/
				
				// ProcessBendPoints.
				for (int i = 0; i < _branches.Count; i++) {
					if (_branches[i] == followUp) {
						_branches[i].RecalculateNormals (prevDirection, prevNormal);
					} else {
						_branches[i].RecalculateNormals (Vector3.zero, Vector3.zero);
					}
				}
			}
			/// <summary>
			/// Get the point origin of this hierarchy of branches from the tree trunk.
			/// </summary>
			/// <param name="positionAtParent"></param>
			/// <returns></returns>
			public Vector3 GetTrunkPoint (float positionAtParent) {
				if (isTrunk || _parent == null) {
					return GetPointAtPosition (positionAtParent);
				} else {
					return parent.GetTrunkPoint (position);
				}
			}
			/// <summary>
			/// Gets a plane at the base of the branch, perpendicular to the parent branch direction.
			/// </summary>
			/// <returns>Plane perpendicular to the parent branch.</returns>
			public Plane GetParentPlane () {
				Plane plane = new Plane ();
				if (_parent != null) {
					Vector3 inNormal = Quaternion.AngleAxis (rollAngle * Mathf.Rad2Deg, _parent.GetDirectionAtPosition (position)) * 
						_parent.GetNormalAtPosition (position);
					plane.SetNormalAndPosition (inNormal, GetPointAtPosition (0f));
				}
				return plane;
			}
			/// <summary>
			/// Attaches a sprout to the current branch.
			/// </summary>
			/// <param name="sprout">Sprout.</param>
			/// <param name="calculateVectors">If set to <c>true</c> calculate vectors for each sprout.</param>
			public void AddSprout (BroccoTree.Sprout sprout, bool calculateVectors = false) {
				_sprouts.Add (sprout);
				sprout.parentBranch = this;
				if (calculateVectors) {
					sprout.CalculateVectors ();
				}
				_sproutsDirty = true;
			}
			/// <summary>
			/// Attaches a list of sprouts to the current branch.
			/// </summary>
			/// <param name="sprouts">Sprouts.</param>
			/// <param name="calculateVectors">If set to <c>true</c> calculate vectors for each sprout.</param>
			public void AddSprouts (List<BroccoTree.Sprout> sprouts, bool calculateVectors = false) {
				for (int i = 0; i < sprouts.Count; i++) {
					AddSprout (sprouts[i], calculateVectors);
				}
			}
			/// <summary>
			/// Calculates the sprouts position and orientation.
			/// </summary>
			/// <param name="recursive">If set to <c>true</c> the calculation is called on all children branches.</param>
			public void UpdateSprouts (bool recursive = true) {
				if (_sproutsDirty) {
					for (int i = 0; i < _sprouts.Count; i++) {
						_sprouts[i].CalculateVectors ();
					}
					_sproutsDirty = false;
				}
				if (recursive) {
					for (int i = 0; i < _branches.Count; i++) {
						_branches[i].UpdateSprouts (recursive);
					}
				}
			}
			/// <summary>
			/// Gets the normal the branch position.
			/// </summary>
			/// <returns>The normal at position.</returns>
			/// <param name="position">Position from 0 to 1.</param>
			public Vector3 GetNormalAtPosition (float position) {
				if (curve != null) {
					return curve.GetPointAt (position).normal;
				}
				return Vector3.forward;
				/*
				if (_bendPoints.Count > 0) {
					Vector3 bendNormal = normal;
					for (int i = 0; i < _bendPoints.Count; i++) {
						if (_bendPoints[i].position > position) {
							break;
						}
						bendNormal = _bendPoints[i].normal;
					}
					return bendNormal;
				} else {
					return normal;
				}
				*/
			}
			/// <summary>
			/// Gets the normal at the branch length.
			/// </summary>
			/// <returns>The normal at length.</returns>
			/// <param name="length">Length.</param>
			public Vector3 GetNormalAtLength (float length) {
				return GetNormalAtPosition (length / this.length);
			}
			public Vector3 GetDirectionAtPosition (float position) {
				CurvePoint p = curve.GetPointAt (position);
				return p.forward;
			}
			public Vector3 GetDirectionAtLength (float length) {
				return GetDirectionAtPosition (length / this.length);
			}
			#endregion

			#region Clone
			/// <summary>
			/// Clones this instance with no extended objects.
			/// </summary>
			/// <returns>The clone instance.</returns>
			public Branch PlainClone (bool createObj = false) {
				Branch clone = new Branch ();
				clone.id = id;
				clone._maxGirth = _maxGirth;
				clone._minGirth = _minGirth;
				clone._girthAtBaseFactor = _girthAtBaseFactor;
				clone._girthAtTopFactor = _girthAtTopFactor;
				if (clone.girthCurve != null) {
					clone.girthCurve = new AnimationCurve (girthCurve.keys);
				}
				clone.girthScale = girthScale;
				clone._length = _length;
				clone._lengthProcessed = _lengthProcessed;
				clone._maxLength = _maxLength;
				clone._lengthFactor = _lengthFactor;
				clone._position = _position;
				clone.direction = direction;
				clone.forward = forward;
				clone._levelLengthGrowthFactor = _levelLengthGrowthFactor;
				clone._level = _level;
				clone._offspringLevels = _offspringLevels;
				clone.positionFromRoot = positionFromRoot;
				clone.positionFromBranch = positionFromBranch;
				clone._positionOffset = _positionOffset;
				clone.rollAngle = rollAngle;
				clone.helperStructureLevelId = helperStructureLevelId;
				clone.curve = curve.Clone ();
				clone.isTuned = isTuned;
				clone.isTrunk = isTrunk;
				clone.isRoot = isRoot;
				clone.isBroken = isBroken;
				clone.breakPosition = breakPosition;
				return clone;
			}
			#endregion
		}
		#endregion

		#region Vars
		/// <summary>
		/// List of branches contained in the tree.
		/// </summary>
		[SerializeField]
		List<Branch> _branches = new List<Branch> ();
		/// <summary>
		/// The branches positions.
		/// </summary>
		[SerializeField]
		List<Vector3> _branchesPositions = new List<Vector3> ();
		/// <summary>
		/// GameObject representing the tree.
		/// </summary>
		public GameObject obj = new GameObject ("root");
		/// <summary>
		/// Minimum girth found on this tree.
		/// </summary>
		public float minGirth = 0.05f; //TODO: get value from traversing branches.
		/// <summary>
		/// Maximum girth found on this tree.
		/// </summary>
		public float maxGirth = 0.25f;  //TODO: get value from traversing branches.
		/// <summary>
		/// Maximum length from the base of the tree to the top branch.
		/// </summary>
		private float _maxLength = -1f;
		#endregion

		#region Getters and Setters
		/// <summary>
		/// Position to spawn the tree.
		/// </summary>
		/// <value>The position.</value>
		public Vector3 position {
			get { return obj.transform.position; }
			set {
				obj.transform.position = value;
			}
		}
		/// <summary>
		/// Branches on this tree.
		/// </summary>
		/// <value>The branches.</value>
		public List<Branch> branches {
			get { return this._branches; }
		}
		/// <summary>
		/// Gets the branches positions.
		/// </summary>
		/// <value>The branches positions.</value>
		public List<Vector3> branchesPositions {
			get { return this._branchesPositions; }
		}
		/// <summary>
		/// Gets the median girth for the branches on the tree.
		/// </summary>
		/// <value>The median girth.</value>
		public float medianGirth {
			get { return (minGirth + maxGirth) / 2f; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="Tree"/> class.
		/// </summary>
		public BroccoTree () { }
		#endregion

		#region Structure Methods
		/// <summary>
		/// Adds the branch.
		/// </summary>
		/// <param name="branch">Branch to add.</param>
		public void AddBranch (BroccoTree.Branch branch) {
			AddBranch (branch, Vector3.zero);
		}
		/// <summary>
		/// Adds the branch.
		/// </summary>
		/// <param name="branch">Branch.</param>
		/// <param name="position">Position.</param>
		public void AddBranch (BroccoTree.Branch branch, Vector3 position) {
			branch.parentTree = this;
			_branches.Add (branch);
			_branchesPositions.Add (position); // TODO bz: remove, will take position from branch.
			branch.positionFromBranch = position;
			branch.positionFromRoot = position;
			branch.isTrunk = true;
			int maxIterations = 30;
			Branch followUpBranch = branch.followUp;
			while (followUpBranch != null && maxIterations > 0) {
				followUpBranch.isTrunk = true;
				followUpBranch = followUpBranch.followUp;
				maxIterations--;
				if (maxIterations == 0) {
					Debug.LogWarning ("Max recursion found while processing tree branches.");
				}
			}
			/*
			if (branch.obj == null) {
				branch.obj = new GameObject ("branch"); // TODO bz: remove.
			}
			branch.obj.transform.SetParent (this.obj.transform); // TODO bz: remove all gameobject.
			branch.obj.transform.localPosition = position;
			*/
		}
		/// <summary>
		/// Update the structure of the tree.
		/// </summary>
		/// <param name="force">If set to <c>true</c> force the update.</param>
		public void Update (bool force = false) {
			for (int i = 0; i < _branches.Count; i++) {
				_branches[i].Update (force);
			}
		}
		/// <summary>
		/// Updates the length.
		/// </summary>
		public void UpdateLength () {
			for (int i = 0; i < _branches.Count; i++) {
				_branches[i].UpdateLength (true);
			}
		}
		/// <summary>
		/// Updates the girth.
		/// </summary>
		public void UpdateGirth () {
			for (int i = 0; i < _branches.Count; i++) {
				_branches[i].UpdateGirth (true);
			}
		}
		/// <summary>
		/// Updates the position.
		/// </summary>
		public void UpdatePosition () {
			for (int i = 0; i < _branches.Count; i++) {
				_branches[i].UpdatePosition ();
			}
		}
		/// <summary>
		/// Updates the follow up branches on the tree.
		/// </summary>
		public void UpdateFollowUps () {
			for (int i = 0; i < _branches.Count; i++) {
				_branches[i].UpdateFollowUps (true);
			}
		}
		/// <summary>
		/// Updates the curve resolution on every branch of the hierarchy.
		/// </summary>
		public void UpdateResolution (int resolutionSteps) {
			for (int i = 0; i < _branches.Count; i++) {
				_branches[i].UpdateResolution (resolutionSteps, true);
			}
		}
		/// <summary>
		/// Recalculates the normals.
		/// </summary>
		public void RecalculateNormals () {
			for (int i = 0; i < _branches.Count; i++) {
				_branches[i].RecalculateNormals ();
			}
		}
		public void CalculateSprouts () {
			for (int i = 0; i < _branches.Count; i++) {
				_branches[i].UpdateSprouts (true);
			}
		}
		/// <summary>
		/// Set the curve that interpolates between min and max girth of
		/// branches in the tree.
		/// </summary>
		/// <param name="curve">Curve to interpolate values.</param>
		public void SetBranchGirthCurve (AnimationCurve curve) {
			for (int i = 0; i < _branches.Count; i++) {
				_branches[i].girthCurve = curve;
			}
		}
		/// <summary>
		/// Sets a unique and non persistent id to each sprout on the tree.
		/// </summary>
		public void SetHelperSproutIds () {
			List<Branch> branches = GetDescendantBranches ();
			int sproutId = 0;
			for (int i = 0; i < branches.Count; i++) {
				for (int j = 0; j < branches[i].sprouts.Count; j++) {
					branches[i].sprouts[j].helperSproutId = sproutId;
					sproutId++;
				}
			}
			branches.Clear ();
		}
		/// <summary>
		/// Traverses the tree and set follow up branches with those with
		/// the greater offspring level if more than one child branch is at
		/// position 1.
		/// </summary>
		public void SetFollowUpBranchesByWeight () {
			List<Branch> branches = GetDescendantBranches ();
			int followUpOffsetLevels = -1;
			for (int i = 0; i < branches.Count; i++) {
				for (int j = 0; j < branches[i].branches.Count; j++) {
					if (branches[i].branches[j].position == 1 && branches[i].branches[j].offspringLevels > followUpOffsetLevels) {
						branches[i].followUp = branches[i].branches[j];
						branches[i].branches[j].isTrunk = branches[i].isTrunk;
						followUpOffsetLevels = branches[i].branches[j].offspringLevels;
					}
				}
			}
			branches.Clear ();
		}
		/// <summary>
		/// Deletes all objects from this tree.
		/// </summary>
		public void Clear() {
			List<Branch> branches = GetDescendantBranches ();
			for (int i = 0; i < branches.Count; i++) {
				branches[i].OnDestroy ();
				/* TODO remove bz
				GameObject.DestroyImmediate (branches[i].obj);
				*/
			}
			_branches.Clear ();
			_branchesPositions.Clear ();
		}
		#endregion

		#region Traversing methods
		/// <summary>
		/// Traverse the tree and returns all branches.
		/// </summary>
		/// <returns>The descendant branches.</returns>
		public List<Branch> GetDescendantBranches () {
			List<Branch> children = new List<Branch> (this._branches);
			foreach (Branch child in _branches) {
				children.AddRange (child.GetDescendantBranches ());
			}
			return children;
		}
		/// <summary>
		/// Traverse the tree and returns branches corresponding to
		/// the desided generation level.
		/// </summary>
		/// <returns>The descendant branches.</returns>
		/// <param name="level">Level of the branches to return.</param>
		public List<Branch> GetDescendantBranches (int level) {
			List<Branch> allChildren = GetDescendantBranches ();
			List<Branch> levelChildren = new List<Branch> ();
			foreach (Branch child in allChildren) {
				if (child.GetLevel () == level)
					levelChildren.Add (child);
			}
			return levelChildren;
		}
		/// <summary>
		/// Get the max offspring level of branches on the tree.
		/// </summary>
		/// <returns>The offspring level.</returns>
		public int GetOffspringLevel () {
			int childrenBranchLevels = 0;
			if (_branches.Count > 0) {
				for (int i = 0; i < _branches.Count; i++) {
					if (_branches[i].offspringLevels > childrenBranchLevels) {
						childrenBranchLevels = _branches[i].offspringLevels;
					}
				}
				childrenBranchLevels += 1;
			}
			return childrenBranchLevels;
		}
		/// <summary>
		/// Get the maximum length of tree branches from the base of the trunk to the tip of the branches or roots.
		/// </summary>
		/// <param name="recalculate">True to recalculate the max length, false to use cached value.</param>
		/// <returns>Max length found from the base of the tree to the last branch tip.</returns>
		public float GetMaxLength (bool recalculate = false) {
			if (recalculate || _maxLength < 0) {
				_maxLength = 0f;
				for (int i = 0; i < branches.Count; i++) {
					GetMaxLengthRecursive (branches[i], 0f, 0);
				}
			}
			return _maxLength;
		}
		/// <summary>
		/// Recursive helper function to get the maximum distance from the root of the tree to the last branch or root.
		/// </summary>
		/// <param name="branch">Branch to inspect for length.</param>
		/// <param name="accumLength">Accumulated length.</param>
		/// <param name="loop">Recursive loop control.</param>
		private void GetMaxLengthRecursive (BroccoTree.Branch branch, float accumLength, int loop) {
			if (accumLength + branch.length > _maxLength) {
				_maxLength = accumLength + branch.length;
			}
			if (loop > 50) {
				Debug.LogWarning ("Probable loop detected with connected branches.");
			}
			for (int i = 0; i < branch.branches.Count; i++) {
				GetMaxLengthRecursive (branch.branches [i], accumLength + (branch.branches[i].position * branch.length), loop + 1);
			}
		}
		/// <summary>
		/// Gets the min & max length found at any branch on this tree.
		/// </summary>
		/// <param name="minLength">Minimum length.</param>
		/// <param name="maxLength">Max length.</param>
		public void GetMinMaxLength (out float minLength, out float maxLength) {
			List<Branch> branches = GetDescendantBranches();
			minLength = (branches.Count > 0?-1f:0f);
			maxLength = 0f;
			//TODO: use cache.
			for (int i = 0; i < branches.Count; i++) {
				if (branches[i].length > maxLength) {
					maxLength = branches[i].length;
				}
				if (branches[i].length < minLength || minLength < 0) {
					minLength = branches[i].length;
				}
			}
		}
		#endregion
	}
}