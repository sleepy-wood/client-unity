using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Broccoli.Pipe;
using Broccoli.Factory;

[System.Serializable]
public class PipeWrapper
{
        /// <summary>
        /// Elements used to create a tree.
        /// </summary>
        public List<PipelineElement> elements = new List<PipelineElement>(); // TODO: 05/04/2017 make it private
        /// <summary>
        /// Dictionary for the relationship between ids and elements.
        /// </summary>
        //[System.NonSerialized]
        public Dictionary<int, PipelineElement> idToElement = new Dictionary<int, PipelineElement>();
        /// <summary>
        /// On a single valid sequence of connected elements the first element of them.
        /// </summary>
        public PipelineElement root = null;
        /// <summary>
        /// The state of validation for this pipeline.
        /// </summary>
        public Pipeline.State state = Pipeline.State.Empty;
        /// <summary>
        /// Number of valid connected series of elements.
        /// </summary>
        public int validPipelines = 0;
        /// <summary>
        /// Point of origin to create the tree.
        /// </summary>
        public Vector3 origin = Vector3.zero;
        /// <summary>
        /// Object used to serialize the pipeline.
        /// </summary>
        [SerializeField]
        public PipelineSerializable _serializedPipeline = new PipelineSerializable();
        /// <summary>
        /// Sprout groups on the pipeline, used to create leafs and other offspring from the branches.
        /// </summary>
        [SerializeField]
        public SproutGroups _sproutGroups = new SproutGroups();
        /// <summary>
        /// Accessor for sprout groups.
        /// </summary>
        /// <value>The sprout groups.</value>
        public SproutGroups sproutGroups { get { return _sproutGroups; } private set { } }
        /// <summary>
        /// The tree factory preferences.
        /// </summary>
        public TreeFactoryPreferences treeFactoryPreferences = new TreeFactoryPreferences();
        /// <summary>
        /// This pipeline is a catalog item.
        /// </summary>
        public bool isCatalogItem = false;
        /// <summary>
        /// The undo control.
        /// </summary>
        public Pipeline.UndoControl undoControl = new Pipeline.UndoControl();
        /// <summary>
        /// The checked elements already checked when validating the pipeline.
        /// </summary>
        public List<int> checkedElementsOnValidation = new List<int>();
        /// <summary>
        /// To delete pipeline elements.
        /// </summary>
        public List<PipelineElement> toDeletePipelineElements = new List<PipelineElement>();
        /// <summary>
        /// The source elements.
        /// </summary>
        public List<PipelineElement> srcElements = new List<PipelineElement>();
        /// <summary>
        /// The seed used to process the pipeline.
        /// </summary>
        public int seed = -1;
        /// <summary>
        /// The random state used to process the pipeline.
        /// </summary>
        public Random.State randomState;
        /// <summary>
        /// Maintains a relationship between pipeline elements and their keynames.
        /// </summary>
        /// <typeparam name="string">Pipeline element keyname.</typeparam>
        /// <typeparam name="PipelineElement">Pipeline element.</typeparam>
        /// <returns>Pipeline element.</returns>
        public Dictionary<string, PipelineElement> keyNameToPipelineElement = new Dictionary<string, PipelineElement>();

        public PipeWrapper(Pipeline pipe)
        {
                this.elements = pipe.elements;
                this.idToElement = pipe.idToElement;
                this.root = pipe.root;
                this.state = pipe.state;
                this.validPipelines = pipe.validPipelines;
                this.origin = pipe.origin;
                this._serializedPipeline = pipe._serializedPipeline;
                this._sproutGroups = pipe._sproutGroups;
                this.sproutGroups = pipe.sproutGroups;
                this.treeFactoryPreferences = pipe.treeFactoryPreferences;
                this.isCatalogItem = pipe.isCatalogItem;
                this.undoControl = pipe.undoControl;
                this.checkedElementsOnValidation = pipe.checkedElementsOnValidation;
                this.toDeletePipelineElements = pipe.toDeletePipelineElements;
                this.srcElements = pipe.srcElements;
                this.seed = pipe.seed;
                this.randomState = pipe.randomState;
                this.keyNameToPipelineElement = pipe.keyNameToPipelineElement;
        }
}
