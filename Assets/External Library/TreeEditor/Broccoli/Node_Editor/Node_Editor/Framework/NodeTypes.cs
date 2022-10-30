using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Broccoli.NodeEditorFramework;

namespace Broccoli.NodeEditorFramework 
{
	/// <summary>
	/// Handles fetching and storing of all NodeDeclarations
	/// </summary>
	public static class NodeTypes
	{
		public static Dictionary<Node, NodeData> nodes;

		/// <summary>
		/// Fetches every Node Declaration in the assembly and stores them in the nodes List.
		/// nodes List contains a default instance of each node type in the key and editor specific NodeData in the value
		/// </summary>
		public static void FetchNodes() 
		{
			nodes = new Dictionary<Node, NodeData> ();

			IEnumerable<Assembly> scriptAssemblies = AppDomain.CurrentDomain.GetAssemblies ().Where ((Assembly assembly) => assembly.FullName.Contains ("Assembly"));
			var scriptAssembliesEnumerator = scriptAssemblies.GetEnumerator ();
			while (scriptAssembliesEnumerator.MoveNext ())
			{
				IEnumerable<Type> types = scriptAssembliesEnumerator.Current.GetTypes ().Where (T => T.IsClass && !T.IsAbstract && T.IsSubclassOf (typeof(Node)));
				var typesEnumerator = types.GetEnumerator ();
				while (typesEnumerator.MoveNext ())
				{
					object[] nodeAttributes = typesEnumerator.Current.GetCustomAttributes(typeof(NodeAttribute), false);                    
					NodeAttribute attr = nodeAttributes[0] as NodeAttribute;
					if(attr == null || !attr.hide)
					{
						Node node = (Node)ScriptableObject.CreateInstance (typesEnumerator.Current); // Create a 'raw' instance (not setup using the appropriate Create function)
						node = node.Create (Vector2.zero); // From that, call the appropriate Create Method to init the previously 'raw' instance
						nodes.Add (node, new NodeData (attr == null? node.name : attr.contextText, attr.order, attr.limitToCanvasTypes));
					}
				}
				nodes = nodes.OrderBy (x => x.Value.order).ToDictionary (x => x.Key, x => x.Value);
			}
		}

		/// <summary>
		/// Returns the NodeData for the given Node
		/// </summary>
		public static NodeData getNodeData (string nodeID)
		{
			return nodes [getDefaultNode (nodeID)];
		}

		/// <summary>
		/// Returns the default node from the given nodeID. 
		/// The default node is a dummy used to create other nodes (Due to various limitations creation has to be performed on Node instances)
		/// </summary>
		public static Node getDefaultNode (string nodeID)
		{
			return nodes.Keys.Single<Node> ((Node node) => node.GetID == nodeID);
		}

		/// <summary>
		/// Returns the default node from the node type. 
		/// The default node is a dummy used to create other nodes (Due to various limitations creation has to be performed on Node instances)
		/// </summary>
		public static T getDefaultNode<T> () where T : Node
		{
			return nodes.Keys.Single<Node> ((Node node) => node.GetType () == typeof (T)) as T;
		}

		/// <summary>
		/// Gets the compatible nodes that have atleast one NodeInput that can connect to the given nodeOutput
		/// </summary>
		public static List<Node> getCompatibleNodes (NodeOutput nodeOutput)
		{
			if (nodeOutput == null)
				throw new ArgumentNullException ("nodeOutput");
			List<Node> compatibleNodes = new List<Node> ();
			var nodesEnumerator = NodeTypes.nodes.GetEnumerator ();
			Node node;
			while (nodesEnumerator.MoveNext ())
			{ // Check if any of the NodeInputs is able to connect to the given NodeOutput
				node = nodesEnumerator.Current.Key;
				for (int inputCnt = 0; inputCnt < node.Inputs.Count; inputCnt++)
				{ // Checking for compability, not using CanApplyConnection to leave out unnessecary dependancy checks
					NodeInput input = node.Inputs[inputCnt];
					if (input == null)
						throw new UnityException ("Input " + inputCnt + " is null!");
					if (input.typeData.Type.IsAssignableFrom (nodeOutput.typeData.Type))
					{
						compatibleNodes.Add (node);
						break;
					}
				}
			}
			return compatibleNodes;
		}
	}

	/// <summary>
	/// The NodeData contains the additional, editor specific data of a node type
	/// </summary>
	public struct NodeData 
	{
		public string adress;
		public Type[] limitToCanvasTypes;
		public int order;

		public NodeData (string name, int order, Type[] limitedCanvasTypes)
		{
			adress = name;
			limitToCanvasTypes = limitedCanvasTypes;
			this.order = order;
		}
	}

	/// <summary>
	/// The NodeAttribute is used to specify editor specific data for a node type, later stored using a NodeData
	/// </summary>
	public class NodeAttribute : Attribute 
	{
		public bool hide { get; private set; }
		public string contextText { get; private set; }
		public int order { get; private set; }
		public Type[] limitToCanvasTypes { get; private set; }

		public NodeAttribute (bool HideNode, string ReplacedContextText, int order)
		{
			hide = HideNode;
			contextText = ReplacedContextText;
			limitToCanvasTypes = null;
			this.order = order;
		}

		public NodeAttribute (bool HideNode, string ReplacedContextText, int order, Type[] limitedCanvasTypes)
		{
			hide = HideNode;
			contextText = ReplacedContextText;
			limitToCanvasTypes = limitedCanvasTypes;
			this.order = order;
		}
	}
}