using UnityEngine;

public class AIPath : MonoBehaviour
{
    public Color pathColor = new Color(1, 0.5f, 0);

    void OnDrawGizmos()
    {
        Gizmos.color = pathColor;

        int count = 1;

        foreach (Transform node in transform)
        {
            AINode nodeComponent = node.GetComponent<AINode>();
            if (nodeComponent == null)
            {
                nodeComponent = node.gameObject.AddComponent<AINode>();
            }

            nodeComponent.GizmosColor = pathColor;

            if (node.name != count.ToString())
                node.name = count.ToString();

            Transform NextNode = transform.Find((count + 1).ToString());
            Transform PreviousNode = transform.Find((count - 1).ToString());

            if (NextNode)
            {
                Gizmos.DrawLine(node.position, NextNode.position);
                nodeComponent.NextAINode = NextNode; 
            }

            if (PreviousNode)
            {
                nodeComponent.PreviousNode = PreviousNode; 
            }

            count++;
        }
    }
}

