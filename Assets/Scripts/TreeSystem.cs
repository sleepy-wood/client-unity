using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Broccoli.Pipe;

public  static class TreeSystem
{
        public static  void Save(Pipeline pipe, string path)
        {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fs = new FileStream(path, FileMode.Create);
                var obj = ScriptableObject.CreateInstance<Pipeline>();

                //var obj = ScriptableObject.CreateInstance<Pipeline>();
                //obj.elements = pipe.elements;
                //obj.idToElement = pipe.idToElement;
                //obj.root = pipe.root;
                //obj.state = pipe.state;
                //obj.validPipelines = pipe.validPipelines;
                //obj.origin = pipe.origin;
                //obj._serializedPipeline = pipe._serializedPipeline;
                //obj._sproutGroups = pipe._sproutGroups;
                //obj.treeFactoryPreferences = pipe.treeFactoryPreferences;
                //obj.isCatalogItem = pipe.isCatalogItem;
                //obj.undoControl = pipe.undoControl;
                //obj.checkedElementsOnValidation = pipe.checkedElementsOnValidation;
                //obj.toDeletePipelineElements = pipe.toDeletePipelineElements;
                //obj.srcElements = pipe.srcElements;
                //obj.seed = pipe.seed;
                //obj.randomState = pipe.randomState;
                //obj.keyNameToPipelineElement = pipe.keyNameToPipelineElement;

                //var json = JsonUtility.ToJson(obj);
                //Debug.Log(json);

                //JsonUtility.FromJsonOverwrite(json, obj);


                PipeWrapper pipeWrapper = new PipeWrapper(pipe);
                formatter.Serialize(fs, pipeWrapper);
                fs.Close();
        }
        public static Pipeline Load(string path)
        {
                if (File.Exists(path))
                {
                        BinaryFormatter formatter = new BinaryFormatter();
                        FileStream fs = new FileStream(path, FileMode.Open);
                        Pipeline pipeline = formatter.Deserialize(fs) as Pipeline;
                        fs.Close();
                        return pipeline;
                }
                else
                {
                        Debug.LogError("Save file is not found!");
                        return null;
                }
        }
}
