using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Broccoli.Pipe;
using Broccoli.NodeEditorFramework.Utilities;

namespace Broccoli.BroccoEditor
{
	/// <summary>
	/// Draws a canvas to select a cropping area on an image.
	/// </summary>
	public class SproutAreaCanvasEditor {
		#region Vars
		/// <summary>
		/// Canvas edition modes.
		/// </summary>
		private enum CanvasEditionMode {
			None,
			Selection,
			MarginTopRight,
			Pivot
		}
		/// <summary>
		/// The color of the sprout area border line.
		/// </summary>
		private static Color sproutAreaBorderLineColor = Color.Lerp (Color.yellow, Color.red, 0.4f);
		/// <summary>
		/// The color of the sprout area pivot.
		/// </summary>
		private static Color sproutAreaPivotColor = Color.Lerp (Color.white, Color.blue, 0.3f);
		/// <summary>
		/// The length of the selection.
		/// </summary>
		private static int selectionLength = 5;
		/// <summary>
		/// The width of the line.
		/// </summary>
		private static int lineWidth = 4;

		/// <summary>
		/// The point draw offset.
		/// </summary>
		Vector2 pointDrawOffset;
		/// <summary>
		/// Rect for the whole canvas.
		/// </summary>
		Rect canvasFullArea;
		/// <summary>
		/// Rect for the canvas display area.
		/// </summary>
		Rect canvasArea;

		/// <summary>
		/// The selection area rect.
		/// </summary>
		Rect selectionAreaRect;
		/// <summary>
		/// The top right limit rect.
		/// </summary>
		Rect topRightLimitRect;
		/// <summary>
		/// The pivot rect.
		/// </summary>
		Rect pivotRect;

		/// <summary>
		/// Width of the selection.
		/// </summary>
		float width = 1f;
		/// <summary>
		/// Height of the selection.
		/// </summary>
		float height = 1f;
		/// <summary>
		/// The x minimum position of the selection.
		/// </summary>
		float xMinPos = 0f;
		/// <summary>
		/// The x max position of the selection.
		/// </summary>
		float xMaxPos = 0f;
		/// <summary>
		/// The y minimum position of the selection.
		/// </summary>
		float yMinPos = 1f;
		/// <summary>
		/// The y max position of the selection.
		/// </summary>
		float yMaxPos = 1f;

		/// <summary>
		/// Selection bottom left position.
		/// </summary>
		Vector2 posBottomLeft;
		/// <summary>
		/// Selection bottom right position.
		/// </summary>
		Vector2 posBottomRight;
		/// <summary>
		/// Selection top right position.
		/// </summary>
		Vector2 posTopRight;
		/// <summary>
		/// Selection top left position.
		/// </summary>
		Vector2 posTopLeft;
		/// <summary>
		/// Selection pivot position.
		/// </summary>
		Vector2 posPivot;

		/// <summary>
		/// The initial position when dragging the selection.
		/// </summary>
		Vector2 initialPos = Vector2.zero;
		/// <summary>
		/// Offset when dragging the selection.
		/// </summary>
		Vector2 offset = Vector2.zero;
		/// <summary>
		/// The top right offset.
		/// </summary>
		Vector2 topRightOffset = Vector2.zero;
		/// <summary>
		/// Offset when dragging the pivot point.
		/// </summary>
		Vector2 pivotOffset = Vector2.zero;

		/// <summary>
		/// The canvas edition mode.
		/// </summary>
		private CanvasEditionMode canvasEditionMode = CanvasEditionMode.None;
		/// <summary>
		/// True if the selection has changed and required redrawing.
		/// </summary>
		bool changed = false;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.TreeNodeEditor.SproutAreaCanvasEditor"/> class.
		/// </summary>
		public SproutAreaCanvasEditor () {
			pointDrawOffset = new Vector2 (lineWidth, 0f);
		}
		#endregion

		#region Draw Methods
		/// <summary>
		/// Draws the canvas.
		/// </summary>
		/// <param name="area">Area used for drawing.</param>
		/// <param name="texture">Texture to display.</param>
		/// <param name="sproutArea">Sprout area for the selection.</param>
		public void DrawCanvas (Rect area, Texture2D texture, SproutMap.SproutMapArea sproutArea) {
			canvasFullArea = area;
			EditorGUI.DrawRect (area, Color.gray);
			int xBorder = 0;
			if (area.width > area.height) {
				xBorder = (int)((area.width - area.height) / 2f);
			}
			float ratio = 1f;
			int yBorder = 0;
			if (area.height > area.width) {
				ratio = area.width / area.height;
				yBorder = (int)((area.height - area.width) / 2f);
			}
			Rect canvas = new Rect ((area.x + xBorder) * ratio, 
				(area.y + yBorder) * ratio, 
				area.width - xBorder * 2, 
				area.height - yBorder * 2);
			EditorGUI.DrawTextureTransparent (canvas, texture, ScaleMode.ScaleToFit);
			canvasArea = new Rect(canvas);
			float textureAspect = texture.width / (float)texture.height;
			if (textureAspect <= 1) { // Higher than wider.
				canvasArea.width = canvasArea.height * textureAspect;
				canvasArea.x += (canvasArea.height - canvasArea.width) / 2f;
			} else { // Wider than higher.
				canvasArea.height = canvasArea.width / textureAspect;
				canvasArea.y += (canvasArea.width - canvasArea.height) / 2f;
			}
			DrawSproutArea (sproutArea, ratio);
		}
		/// <summary>
		/// Draws the sprout area for the selection with controls to modify its dimensions.
		/// </summary>
		/// <param name="sproutArea">Sprout area.</param>
		public void DrawSproutArea (SproutMap.SproutMapArea sproutArea, float ratio) {
			//EditorGUI.DrawRect (canvasArea, Color.red);
			// Draw bounding box.
			width = canvasArea.xMax - canvasArea.xMin;
			height = canvasArea.yMax - canvasArea.yMin;
			xMinPos = canvasArea.xMin + (width * sproutArea.x);
			xMaxPos = xMinPos + (width * sproutArea.width);
			yMinPos = canvasArea.yMax - (height * sproutArea.y);
			yMaxPos = yMinPos - (height * sproutArea.height);
			posBottomLeft = new Vector2 (xMinPos, yMinPos);
			posBottomRight = new Vector2 (xMaxPos, yMinPos);
			posTopRight = new Vector2 (xMaxPos, yMaxPos);
			posTopLeft = new Vector3 (xMinPos, yMaxPos);
			posPivot = new Vector2 (Mathf.Lerp (xMinPos, xMaxPos, sproutArea.pivotX),
				Mathf.Lerp (yMinPos, yMaxPos, sproutArea.pivotY));
			SetCtrlRects ();
			DrawSelection ();

			// Draw bounding box controls.
			DrawPoint (posTopRight + offset + topRightOffset, sproutAreaBorderLineColor);

			// Draw pivot point.
			DrawPoint (posPivot + offset + pivotOffset, sproutAreaPivotColor);
			SetCursorRects ();
			if (Event.current.type == EventType.MouseDrag ||
			    Event.current.type == EventType.MouseDown ||
			    Event.current.type == EventType.MouseUp) {
				ReceiveEvent (Event.current);
			}
		}
		/// <summary>
		/// Draws the selection.
		/// </summary>
		private void DrawSelection () {
			Vector2 bottomLeft = posBottomLeft + offset;
			Vector2 bottomRight = new Vector2 (posBottomRight.x + offset.x + topRightOffset.x, posBottomRight.y + offset.y);
			Vector2 topRight = posTopRight + offset + topRightOffset;
			Vector2 topLeft = new Vector2 (posTopLeft.x + offset.x, posTopLeft.y + offset.y + topRightOffset.y);
			RTEditorGUI.DrawLine (bottomLeft, bottomRight, sproutAreaBorderLineColor, null, 2);
			RTEditorGUI.DrawLine (bottomRight, topRight, sproutAreaBorderLineColor, null, 2);
			RTEditorGUI.DrawLine (topRight, topLeft, sproutAreaBorderLineColor, null, 2);
			RTEditorGUI.DrawLine (topLeft, bottomLeft, sproutAreaBorderLineColor, null, 2);
		}
		/// <summary>
		/// Draws a point.
		/// </summary>
		/// <param name="pointCenter">Point center.</param>
		/// <param name="color">Color.</param>
		private void DrawPoint (Vector2 pointCenter, Color color) {
			RTEditorGUI.DrawLine (pointCenter - pointDrawOffset, pointCenter + pointDrawOffset, color, null, lineWidth * 2);
		}
		#endregion

		#region Ops
		/// <summary>
		/// Receives the event.
		/// </summary>
		/// <param name="curEvent">Current event.</param>
		private void ReceiveEvent (Event curEvent) {
			if (canvasFullArea.Contains(curEvent.mousePosition)) {
				if (curEvent.type == EventType.MouseDown) {
					if (topRightLimitRect.Contains (curEvent.mousePosition)) {
						canvasEditionMode = CanvasEditionMode.MarginTopRight;
					} else if (pivotRect.Contains (curEvent.mousePosition)) {
						canvasEditionMode = CanvasEditionMode.Pivot;
					} else if (selectionAreaRect.Contains (curEvent.mousePosition)) {
						canvasEditionMode = CanvasEditionMode.Selection;
					} else {
						canvasEditionMode = CanvasEditionMode.None;
					}
					initialPos = curEvent.mousePosition;
					offset = Vector3.zero;
				} else if (curEvent.type == EventType.MouseUp) {
					if ((canvasEditionMode == CanvasEditionMode.Selection && offset != Vector2.zero) || 
						(canvasEditionMode == CanvasEditionMode.MarginTopRight && topRightOffset != Vector2.zero) ||
						(canvasEditionMode == CanvasEditionMode.Pivot && pivotOffset != Vector2.zero)) {
						changed = true;
					}
				} else if (curEvent.type == EventType.MouseDrag) {
					if (canvasEditionMode == CanvasEditionMode.Selection) {
						offset = curEvent.mousePosition - initialPos;
						if (posTopRight.x + offset.x > canvasArea.xMax) {
							offset.x = canvasArea.xMax - posTopRight.x;
						}
						if (posBottomLeft.x + offset.x < canvasArea.x) {
							offset.x = canvasArea.x - posBottomLeft.x;
						}
						if (posTopRight.y + offset.y < canvasArea.y) {
							offset.y = canvasArea.y - posTopRight.y;
						}
						if (posBottomLeft.y + offset.y > canvasArea.yMax) {
							offset.y = canvasArea.yMax - posBottomLeft.y;
						}
					} else if (canvasEditionMode == CanvasEditionMode.MarginTopRight) {
						topRightOffset = curEvent.mousePosition - initialPos;
						if (posTopRight.x + topRightOffset.x > canvasArea.xMax) {
							topRightOffset.x = canvasArea.xMax - posTopRight.x;
						}
						if (posTopRight.x + topRightOffset.x < posBottomLeft.x) {
							topRightOffset.x = posBottomLeft.x - posTopRight.x;
						}
						if (posTopRight.y + topRightOffset.y < canvasArea.y) {
							topRightOffset.y = canvasArea.y - posTopRight.y;
						}
						if (posTopRight.y + topRightOffset.y > posBottomLeft.y) {
							topRightOffset.y = posBottomLeft.y - posTopRight.y;
						}
					} else if (canvasEditionMode == CanvasEditionMode.Pivot) {
						pivotOffset = curEvent.mousePosition - initialPos;
						if (posPivot.x + pivotOffset.x > posTopRight.x) {
							pivotOffset.x = posTopRight.x - posPivot.x;
						}
						if (posPivot.x + pivotOffset.x < posTopLeft.x) {
							pivotOffset.x = posTopLeft.x - posPivot.x;
						}
						if (posPivot.y + pivotOffset.y < posTopRight.y) {
							pivotOffset.y = posTopRight.y - posPivot.y;
						}
						if (posPivot.y + pivotOffset.y > posBottomRight.y) {
							pivotOffset.y = posBottomRight.y - posPivot.y;
						}
					}
				}
			}
		}
		/// <summary>
		/// Applies the changes made on the canvas to a sprout area.
		/// </summary>
		/// <param name="sproutArea">Sprout area.</param>
		public void ApplyChanges (SproutMap.SproutMapArea sproutArea) {
			if (canvasEditionMode == CanvasEditionMode.Selection && offset != Vector2.zero) {
				float x = (posTopLeft.x + offset.x - canvasArea.x) / canvasArea.width;
				sproutArea.x = x;
				float y = 1 - ((posBottomLeft.y + offset.y - canvasArea.y) / canvasArea.height);
				sproutArea.y = y;
			} else if (canvasEditionMode == CanvasEditionMode.MarginTopRight && topRightOffset != Vector2.zero) {
				float width = (posTopRight.x + topRightOffset.x - posTopLeft.x) / canvasArea.width;
				sproutArea.width = width;
				float height = (posBottomRight.y - posTopRight.y - topRightOffset.y) / canvasArea.height;
				sproutArea.height = height;
			} else if (canvasEditionMode == CanvasEditionMode.Pivot && pivotOffset != Vector2.zero) {
				float pivotX = 1 - ((posPivot.x + pivotOffset.x - posTopRight.x) / (posTopLeft.x - posTopRight.x));
				sproutArea.pivotX = pivotX;
				float pivotY = 1 - ((posPivot.y + pivotOffset.y - posTopRight.y) / (posBottomRight.y - posTopRight.y));
				sproutArea.pivotY = pivotY;
			}
			Reset ();
		}
		/// <summary>
		/// Determines whether the canvas selection has changed since the last ApplyChanges action.
		/// </summary>
		/// <returns><c>true</c> if selection has changed; otherwise, <c>false</c>.</returns>
		public bool HasChanged () {
			return changed;
		}
		/// <summary>
		/// Reset the canvas.
		/// </summary>
		private void Reset () {
			canvasEditionMode = CanvasEditionMode.None;
			offset = Vector2.zero;
			topRightOffset = Vector2.zero;
			pivotOffset = Vector2.zero;
			changed = false;
		}
		/// <summary>
		/// Sets the ctrl rects, used to modify the selection using the mouse.
		/// </summary>
		private void SetCtrlRects () {
			selectionAreaRect = new Rect (posTopLeft.x, posTopLeft.y, 
				posTopRight.x - posTopLeft.x, posBottomLeft.y - posTopLeft.y);
			topRightLimitRect = new Rect (posTopRight.x - selectionLength, 
				posTopRight.y - selectionLength, selectionLength * 2, selectionLength * 2);
			pivotRect = new Rect (posPivot.x - selectionLength, 
				posPivot.y - selectionLength, selectionLength * 2, selectionLength * 2);
		}
		/// <summary>
		/// Sets the cursor used in the controls on the canvas.
		/// </summary>
		private void SetCursorRects () {
			if (canvasEditionMode == CanvasEditionMode.None) {
				EditorGUIUtility.AddCursorRect (topRightLimitRect, MouseCursor.ResizeUpRight);
				EditorGUIUtility.AddCursorRect (pivotRect, MouseCursor.ScaleArrow);
				EditorGUIUtility.AddCursorRect (selectionAreaRect, MouseCursor.MoveArrow);
			} else if (canvasEditionMode == CanvasEditionMode.Selection) {
				EditorGUIUtility.AddCursorRect (canvasFullArea, MouseCursor.MoveArrow);
			} else if (canvasEditionMode == CanvasEditionMode.MarginTopRight) {
				EditorGUIUtility.AddCursorRect (canvasFullArea, MouseCursor.ResizeUpRight);
			} else if (canvasEditionMode == CanvasEditionMode.Pivot) {
				EditorGUIUtility.AddCursorRect (canvasFullArea, MouseCursor.ScaleArrow);
			}
		}
		#endregion
	}
}