using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Broccoli.Utils
{
	public class EditorGUISplitView
	{
		#region SplitDef class
		/// <summary>
		/// Definition for a split on a window.
		/// </summary>
		public class SplitDef {
			/// <summary>
			/// Modes for splits.
			/// </summary>
			public enum Mode {
				Fixed,
				Dynamic
			}
			/// <summary>
			/// Direction of the split handled by this SplitView.
			/// </summary>
			public Direction direction = Direction.Horizontal;
			/// <summary>
			/// Mode of the split.
			/// </summary>
			public Mode mode = Mode.Dynamic;
			/// <summary>
			/// Relative size of the split. How much space it takes from
			/// the available area. From 0 to 1.
			/// </summary>
			public float size = 0.5f;
			/// <summary>
			/// Size in pixels used on fixed mode.
			/// </summary>
			public int sizePx = 0;
			/// <summary>
			/// Minimum pixel size.
			/// </summary>
			public int minSizePx = 0;
			/// <summary>
			/// Temporary scroll position.
			/// </summary>
			public Vector2 tempScrollPosition = Vector2.zero;
		}
		#endregion

		#region Vars
		/// <summary>
		/// Directions available for the splits.
		/// </summary>
		public enum Direction {
			Horizontal,
			Vertical
		}
		/// <summary>
		/// Flag to check if a split has change size.
		/// </summary>
		public bool splitChanged = false;
		/// <summary>
		/// Direction used for all the splits used on this split view.
		/// </summary>
		Direction splitDirection = Direction.Horizontal;
		/// <summary>
		/// Splits managed by this split view.
		/// </summary>
		/// <typeparam name="SplitDef">Split definition.</typeparam>
		/// <returns>List of split definitions.</returns>
		List<SplitDef> splits = new List<SplitDef> ();
		/// <summary>
		/// Rect to be filled by the splits.
		/// </summary>
		Rect availableRect;
		/// <summary>
		/// Index of the split being drawn.
		/// </summary>
		int currentSplitIndex = 0;
		/// <summary>
		/// Relative available size.
		/// </summary>
		float availableSize = 1f;
		/// <summary>
		/// Index of the split being resized.
		/// </summary>
		int resizeIndex = -1;
		/// <summary>
		/// Parent window of this split view.
		/// </summary>
		EditorWindow parentWindow = null;
		/// <summary>
		/// Parent split view of this split view.
		/// </summary>
		EditorGUISplitView parentSplit = null;
		/// <summary>
		/// If <c>true</c> the parent of this split view is a window.
		/// </summary>
		bool parentIsWindow = false;
		/// <summary>
		/// Relative limits where the splits are dragged to be resized.
		/// </summary>
		/// <typeparam name="float">Relative position of the limit.</typeparam>
		/// <returns>Limits.</returns>
		List<float> limits = new List<float> ();
		/// <summary>
		/// Calculated sizes for every split handled by this split view.
		/// </summary>
		/// <typeparam name="int">Size in pixels.</typeparam>
		/// <returns>Sizes.</returns>
		List<int> sizes = new List<int> ();
		/// <summary>
		/// If <c>true</c> the sizes of the splits do not require recalculation.
		/// </summary>
		bool isCalculated = false;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for this split view whose parent is a window.
		/// </summary>
		/// <param name="splitDirection">Direction of the splits.</param>
		/// <param name="parentWindow">Parent window.</param>
		public EditorGUISplitView (Direction splitDirection, EditorWindow parentWindow) {
			this.parentWindow = parentWindow;
			this.splitDirection = splitDirection;
			parentIsWindow = true;
		}
		/// <summary>
		/// Constructor for this split view whose parent is a split view.
		/// </summary>
		/// <param name="splitDirection">Direction of the splits.</param>
		/// <param name="parentSplit">Parent split view.</param>
		public EditorGUISplitView (Direction splitDirection, EditorGUISplitView parentSplit) {
			this.parentSplit = parentSplit;
			this.splitDirection = splitDirection;
			parentIsWindow = false;
		}
		#endregion

		#region Split CRUD
		/// <summary>
		/// Checks if the parent rect is a window.
		/// </summary>
		/// <returns><c>True</c> if the containing rect is a window.</returns>
		public bool ParentIsWindow () {
			return parentIsWindow;
		}
		/// <summary>
		/// Gets the parent window that contains this split view.
		/// </summary>
		/// <returns>Parent editor window or null.</returns>
		public EditorWindow GetParentWindow () {
			return parentWindow;
		}
		/// <summary>
		/// Gets the parent split view that contains this split view.
		/// </summary>
		/// <returns>Parent split view or null.</returns>
		public EditorGUISplitView GetParentView () {
			return parentSplit;
		}
		/// <summary>
		/// Adds a split with a fixed size.
		/// </summary>
		/// <param name="size">Size in pixels.</param>
		/// <returns><c>True</c> if the split was added.</returns>
		public bool AddFixedSplit (int size) {
			if (size <= 0) {
				return false;
			}
			if (size < 50) {
				size = 50;
			}
			SplitDef splitDef = new SplitDef ();
			splitDef.mode = SplitDef.Mode.Fixed;
			splitDef.sizePx = size;
			splitDef.minSizePx = size;
			splits.Add (splitDef);
			limits.Add (0f);
			sizes.Add (0);
			return true;
		}
		/// <summary>
		/// Adds a split with resizable capabilities.
		/// </summary>
		/// <param name="size">Size in pixels.</param>
		/// <param name="minSizePx">Minimum size in pixels.</param>
		/// <returns><c>True</c> if the split was added.</returns>
		public bool AddDynamicSplit (float size = -1f, int minSizePx = 100) {
			if (size < 0f) {
				size = availableSize;
				availableSize = 0f;
			} else {
				if (availableSize - size < 0f) {
					return false;
				} else {
					availableSize -= size;
				}
			}
			if (minSizePx < 100) {
				minSizePx = 100;
			}
			SplitDef splitDef = new SplitDef ();
			splitDef.mode = SplitDef.Mode.Dynamic;
			splitDef.size = size;
			splitDef.minSizePx = minSizePx;
			splits.Add (splitDef);
			limits.Add (0f);
			sizes.Add (0);
			return true;
		}
		/// <summary>
		/// Gets a split from this split view.
		/// </summary>
		/// <param name="index">Index to the split.</param>
		/// <returns>Split definition.</returns>
		public SplitDef GetSplit (int index) {
			if (index < splits.Count) {
				return splits [index];
			}
			return null;
		}
		/// <summary>
		/// Gets the size in pixels of the split size being drawn.
		/// </summary>
		/// <returns>Width in pixels for horizontal mode and height in pixels for vertical mode.</returns>
		public int GetCurrentSplitSize () {
			if (currentSplitIndex >= 0) {
				return sizes [currentSplitIndex];
			}
			return 0;
		}
		/// <summary>
		/// Gets the number of splits handled by this split view.
		/// </summary>
		/// <returns>Number of splits.</returns>
		public int SplitsCount () {
			return splits.Count;
		}
		/// <summary>
		/// Clears this instance.
		/// </summary>
		public void Clear () {
			splits.Clear ();
			sizes.Clear ();
			limits.Clear ();
			isCalculated = false;
			currentSplitIndex = 0;
			availableSize = 1f;
		}
		#endregion
		
		#region Split Draw
		/// <summary>
		/// Begins drawing the splits.
		/// </summary>
		public void BeginSplitView () {
			if (splits.Count == 0)
				return;
			Rect tempRect;
			float availablePxSize = 0f;
			if (splitDirection == Direction.Horizontal) {
				tempRect = EditorGUILayout.BeginHorizontal (GUILayout.ExpandWidth (true));
				availablePxSize = tempRect.width;
			} else {
				tempRect = EditorGUILayout.BeginVertical (GUILayout.ExpandHeight (true));
				availablePxSize = tempRect.height;
			}
			if (tempRect.width > 0.0f) {
				if (tempRect != availableRect) {
					availableRect = tempRect;
					isCalculated = false;
				}
			}
			// Calculate widths.
			CalculateSizesAndLimits (availablePxSize);
			currentSplitIndex = 0;
			BeginSplit ();
		}
		/// <summary>
		/// Calculates the sizes and limits used for each split.
		/// </summary>
		/// <param name="availablePxSize">Number of pixels available.</param>
		void CalculateSizesAndLimits (float availablePxSize) {
			if (!isCalculated && availablePxSize > 0 && Event.current.type == EventType.Repaint) {
				isCalculated = true;
				float limit = 0;
				float calcSize = 0f;
				float calcPxSize = 0;
				float accumSize = 0f;
				SplitDef splitDef;
				for (int i = 0; i < splits.Count; i++) {
					splitDef = splits [i];
					if (splitDef.mode == SplitDef.Mode.Fixed) {
						calcPxSize = splitDef.sizePx;
						calcSize = calcPxSize / availablePxSize;
						accumSize += calcSize;
					} else {
						calcSize = splitDef.size;
						calcPxSize = availablePxSize * calcSize;
						if (calcPxSize < splitDef.minSizePx) {
							calcPxSize = splitDef.minSizePx;
							calcSize = calcPxSize / availablePxSize;
							splitDef.size = calcSize;
						}
						if (accumSize + calcSize > 1f) {
							calcSize = 1f - accumSize;
							splitDef.size = calcSize;
							calcPxSize = availablePxSize * calcSize;
						}
						accumSize += calcSize;
					}
					limit += calcPxSize / (float)availablePxSize;
					sizes [i] = (int)calcPxSize;
					limits [i] = limit;
				}
			}
		}
		/// <summary>
		/// Applies a new limit to a split, validating and recalculating space and limits for
		/// the rest of the splits.
		/// </summary>
		/// <param name="splitIndex">Index of the split to apply the new limit to.</param>
		/// <param name="newLimit">Limit to apply, relative from 0 to 1.</param>
		/// <returns><c>True</c> if the limit could be applied.</returns>
		bool ApplyLimit (int splitIndex, float newLimit) {
			if (splitIndex < 0 || splitIndex > splits.Count) {
				return false;
			}
			bool canApplyLimit = false;
			int minSizePx = 0;

			// Check available space upstream.
			for (int i = 0; i <= splitIndex; i++) {
				minSizePx += splits [i].minSizePx;
			}
			float availablePxSize = GetAvailableSize ();
			if (minSizePx <= availablePxSize * newLimit) {
				canApplyLimit = true;
			}
			if (!canApplyLimit) {
				return false;
			}

			// Check available space downstream.
			minSizePx = 0;
			for (int i = splitIndex + 1; i < splits.Count; i++) {
				minSizePx += splits [i].minSizePx;
			}
			if (minSizePx <= availablePxSize * (1f - newLimit)) {
				canApplyLimit = true;
			}

			// Validate with available size.
			if (minSizePx + (newLimit * availablePxSize) >= availablePxSize) {
				canApplyLimit = false;
			}
			
			if (!canApplyLimit) {
				return false;
			}

			//Apply new limits and recalculate sizes upstream.
			float offset = newLimit - limits [splitIndex];
			float pxOffset = offset * availablePxSize;
			for (int i = splitIndex; i >= 0; i--) {
				SplitDef splitDef = splits[i];
				if ((float)sizes[i] + pxOffset > splitDef.minSizePx) {
					sizes [i] = sizes [i] + (int)pxOffset;
					limits [i] = newLimit;
					splitDef.size += offset;
					break;
				}
			}
			// Apply new limits and recalculate sizes downstream.
			for (int i = splitIndex + 1; i < splits.Count; i++) {
				SplitDef splitDef = splits[i];
				if ((float)sizes[i] - pxOffset > splitDef.minSizePx) {
					sizes [i] = sizes [i] - (int)pxOffset;
					limits [i] -= offset;
					splitDef.size -= offset;
					break;
				}
			}

			isCalculated = false;
			CalculateSizesAndLimits (availablePxSize);
			return true;
		}
		/// <summary>
		/// Internal function to begin a new split area on the GUI.
		/// </summary>
		void BeginSplit () {
			SplitDef splitDef = splits[currentSplitIndex];
			if(splitDirection == Direction.Horizontal) {
				splitDef.tempScrollPosition = 
					GUILayout.BeginScrollView (splitDef.tempScrollPosition, 
						GUILayout.Width (sizes[currentSplitIndex]));
			} else {
				splitDef.tempScrollPosition = 
					GUILayout.BeginScrollView (splitDef.tempScrollPosition, 
						GUILayout.Height (sizes[currentSplitIndex]));
			}
		}
		/// <summary>
		/// Closes the current split to begin a new one.
		/// </summary>
		public void Split () {
			SplitDef splitDef = splits [currentSplitIndex];
			GUILayout.EndScrollView ();
			ResizeSplit ();
			currentSplitIndex ++;
			if (currentSplitIndex < splits.Count) {
				BeginSplit ();
			}
		}
		/// <summary>
		/// Ends the split view drawing.
		/// </summary>
		public void EndSplitView() {
			if (currentSplitIndex < splits.Count) {
				SplitDef splitDef= splits [currentSplitIndex];
				GUILayout.EndScrollView ();
			}
			if(splitDirection == Direction.Horizontal)
				EditorGUILayout.EndHorizontal ();
			else 
				EditorGUILayout.EndVertical ();
		}
		/// <summary>
		/// Gets the available size from the rect this split view display its content.
		/// </summary>
		/// <returns>Number of pixels available. If horizontal then width; if vertical, height.</returns>
		private float GetAvailableSize () {
			if (splitDirection == Direction.Horizontal) {
				return availableRect.width;
			} else {
				return availableRect.height;
			}
		}
		/// <summary>
		/// Draw the handles and manages resizing from the user input.
		/// </summary>
		private void ResizeSplit() {
			SplitDef splitDef = splits [currentSplitIndex];
			if (splitDef.mode == SplitDef.Mode.Fixed) {
				return;
			}
			Rect resizeHandleRect;
			if (splitDirection == Direction.Horizontal) {
				resizeHandleRect = new Rect (limits[currentSplitIndex] * availableRect.width, availableRect.y, 4f, availableRect.height);
				resizeHandleRect.x -= 3;
			} else {
				resizeHandleRect = new Rect (availableRect.x, limits[currentSplitIndex] * availableRect.height, availableRect.width, 4f);
				resizeHandleRect.y -= 3;
			}

			if (splitDirection == Direction.Horizontal)
				EditorGUIUtility.AddCursorRect (resizeHandleRect, MouseCursor.ResizeHorizontal);
			else
				EditorGUIUtility.AddCursorRect (resizeHandleRect, MouseCursor.ResizeVertical);

			if (Event.current.type == EventType.MouseDown && resizeHandleRect.Contains(Event.current.mousePosition)){
				resizeIndex = currentSplitIndex;
			}
			if (resizeIndex == currentSplitIndex && Event.current.type == EventType.Repaint) {
				float newLimit;
				if (splitDirection == Direction.Horizontal) {
					newLimit = Event.current.mousePosition.x / availableRect.width;
				} else {
					newLimit = Event.current.mousePosition.y / availableRect.height;
				}
				splitChanged = ApplyLimit (currentSplitIndex, newLimit);
			}
			if (Event.current.type == EventType.MouseUp)
				resizeIndex = -1;

			if (splitDirection == Direction.Horizontal) {
				resizeHandleRect.width -= 3;
			} else {
				resizeHandleRect.height -= 3;
			}
			GUI.Box (resizeHandleRect, "");
			//GUI.DrawTexture (resizeHandleRect, EditorGUIUtility.whiteTexture);
		}
		#endregion
	}
}

