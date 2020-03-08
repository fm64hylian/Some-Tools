using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StageActionType {
    Add,
    Remove
}

public class StageAction 
{
    public StageActionType ActionType;
    public List<Vector3> positions= new List<Vector3>();
    // Start is called before the first frame update

    public StageAction(StageActionType latestAction, List<Vector3> positionaffectedPositions) {
        ActionType = latestAction;
        positions = positionaffectedPositions;
    }
}
