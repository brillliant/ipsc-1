using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace {
    [System.Serializable]
    public class ObjectData {
        public string prefabName;
        public Vector3 position;
        public Quaternion rotation;

        public ObjectData(string prefabName, Vector3 position, Quaternion rotation) {
            this.prefabName = prefabName;
            this.position = position;
            this.rotation = rotation;
        }
    }
    
    [System.Serializable]
    public class ObjectDataList
    {
        public List<ObjectData> objectDataList;
    }
}