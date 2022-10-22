using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.Model;

namespace Broccoli.Pipe {
    /// <summary>
    /// Composite variation container class.
    /// </summary>
    [System.Serializable]
    public class VariationDescriptor {
        #region Composite Variant Unit Descriptor
        [System.Serializable]
        public class VariationUnitDescriptor {
            #region Vars
            public int unitId = 0;
            #endregion

            #region Clone
            public VariationUnitDescriptor Clone () {
                VariationUnitDescriptor clone = new VariationUnitDescriptor ();
                clone.unitId = unitId;
                return clone;
            }
            #endregion
        }
        #endregion

        #region Structure Vars
        public int id = 0;
        public int seed = 0;
        public List<VariationUnitDescriptor> variantUnitDescriptors = new List<VariationUnitDescriptor> ();
        #endregion

        #region Constructor
        public VariationDescriptor () {}
        #endregion

        #region Clone
        /// <summary>
        /// Clone this instance.
        /// </summary>
        public VariationDescriptor Clone () {
            VariationDescriptor clone = new VariationDescriptor ();
            clone.id = id;
            clone.seed = seed;
            for (int i = 0; i < variantUnitDescriptors.Count; i++) {
                clone.variantUnitDescriptors.Add (variantUnitDescriptors [i].Clone ());
            }
            return clone;
        }
        #endregion
    }
}