using UnityEngine;

using Broccoli.Model;

namespace Broccoli.Pipe {
	/// <summary>
	/// ScriptableObject wrap for the BranchDescriptorCollection class.
	/// </summary>
	public class BranchDescriptorCollectionSO : ScriptableObject, IClonable<BranchDescriptorCollectionSO> {
        public BranchDescriptorCollection branchDescriptorCollection = new BranchDescriptorCollection ();
		public BranchDescriptorCollectionSO Clone () {
			BranchDescriptorCollectionSO clone = ScriptableObject.CreateInstance<BranchDescriptorCollectionSO> ();
			clone.branchDescriptorCollection = branchDescriptorCollection.Clone ();
			return clone;
		}
	}
}