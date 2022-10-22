using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Broccoli.Pipe {
	/// <summary>
	/// Item for log entries.
	/// </summary>
	public class LogItem {
		/// <summary>
		/// Message type.
		/// </summary>
		public enum MessageType
		{
			Info,
			Warning,
			Error
		}
		/// <summary>
		/// The message.
		/// </summary>
		string _msg;
		/// <summary>
		/// Gets the message.
		/// </summary>
		/// <value>The message.</value>
		public string message { get { return _msg; } private set{ } }
		/// <summary>
		/// The type of the message.
		/// </summary>
		MessageType _msgType = MessageType.Info;
		/// <summary>
		/// Gets the type of the message.
		/// </summary>
		/// <value>The type of the message.</value>
		public MessageType messageType { get { return _msgType; } private set { } }
		/// <summary>
		/// The pipeline element related to the log entry.
		/// </summary>
		PipelineElement _pipelineElement = null;
		/// <summary>
		/// Gets or sets the pipeline element.
		/// </summary>
		/// <value>The pipeline element.</value>
		public PipelineElement pipelineElement { 
			get { return _pipelineElement; } 
			set { this._pipelineElement = value; } 
		}
		/// <summary>
		/// Gets a LogItem object.
		/// </summary>
		/// <returns>LogItem object.</returns>
		/// <param name="message">Message.</param>
		/// <param name="messageType">Message type.</param>
		public static LogItem GetItem (string message, MessageType messageType) {
			LogItem item = new LogItem ();
			item._msg = message;
			item._msgType = messageType;
			return item;
		}
		/// <summary>
		/// Gets a LogItem object of type info.
		/// </summary>
		/// <returns>The info item.</returns>
		/// <param name="message">Message.</param>
		public static LogItem GetInfoItem (string message) {
			LogItem item = GetItem (message, MessageType.Info);
			return item;
		}
		/// <summary>
		/// Gets a LogItem object of type warning.
		/// </summary>
		/// <returns>The warn item.</returns>
		/// <param name="message">Message.</param>
		public static LogItem GetWarnItem (string message) {
			LogItem item = GetItem (message, MessageType.Warning);
			return item;
		}
		/// <summary>
		/// Gets a LogItem object of type error.
		/// </summary>
		/// <returns>The error item.</returns>
		/// <param name="message">Message.</param>
		public static LogItem GetErrorItem (string message) {
			LogItem item = GetItem (message, MessageType.Error);
			return item;
		}
	}
}