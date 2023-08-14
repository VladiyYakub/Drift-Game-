using UnityEngine;

public class AINode : MonoBehaviour
{
    public Transform NextAINode;
    public Transform PreviousNode;

    public NodeSetting NodeAISetting;

    [HideInInspector]
    public Color GizmosColor = new Color(1, 1, 1);

    [System.Serializable]
    public class NodeSetting
    {
        public bool braking; 
    }
    void OnDrawGizmos()
    {
        transform.LookAt(NextAINode);

        Gizmos.color = NodeAISetting.braking ? Color.red : GizmosColor;

        Gizmos.DrawSphere(transform.position, 1.0f);
    }
}

