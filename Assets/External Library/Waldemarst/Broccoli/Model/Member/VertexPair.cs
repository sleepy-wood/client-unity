namespace Broccoli.Model {
	/// <summary>
	/// Pair class to use on vertex operations.
	/// First and second position represent either a vertex position, UV or color channels.
	/// </summary>
	/// <typeparam name="T">First value in the pair.</typeparam>
	/// <typeparam name="U">Second value in the pair.</typeparam>
	public class VertexPair<T, U> {
		#region Constructors
		public VertexPair() {	}
		public VertexPair(T first, U second) {
			this.First = first;
			this.Second = second;
		}
		#endregion
		#region Accessors
		public T First { get; set; }
		public U Second { get; set; }
		#endregion
	}
}