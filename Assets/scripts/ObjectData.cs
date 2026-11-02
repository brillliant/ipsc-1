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
    public class ShootingZoneData {
        public List<Vector3> points;   // локальные относительно stageRoot
        public bool closed;
    }

    /// <summary>Описание готового стейджа для панели Stage info (из официального брифинга USPSA); у пользовательских стейджей отсутствует.</summary>
    [System.Serializable]
    public class StageInfoData {
        public string code;        // номер классифаера, например "CM 99-11"
        public string procedure;   // START POSITION + PROCEDURE
        public string scoring;     // SCORING, TARGETS, SCORED HITS, START-STOP
        public string briefing;    // Written Stage Briefing целиком
        public string image;       // путь к картинке в Resources без расширения
    }

    [System.Serializable]
    public class ObjectDataList
    {
        public List<ObjectData> objectDataList;
        public List<ShootingZoneData> shootingZones;
        public List<Vector3> startMarks;   // крестики под ноги на старте, локальные относительно stageRoot (может отсутствовать)
        public StageInfoData info;         // описание стейджа (может отсутствовать)
    }
}