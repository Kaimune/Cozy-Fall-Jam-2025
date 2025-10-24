using UnityEngine;

public static class PlayAreaUtils
{
    /// <summary>
    /// Clamps a position inside the play area defined by FruitManager.
    /// Optional: push out of obstacles.
    /// </summary>
    public static Vector3 ClampPosition(Vector3 pos, GameObject[] obstacles = null, float buffer = 0.1f)
    {
        if (FruitManager.Instance == null)
            return pos;

        // Clamp to play area
        Vector3 min = FruitManager.Instance.transform.position + new Vector3(buffer, 0, buffer);
        Vector3 max = min + new Vector3(FruitManager.Instance.playAreaLength - buffer * 2, 0, FruitManager.Instance.playAreaWidth - buffer * 2);


        pos.x = Mathf.Clamp(pos.x, min.x, max.x);
        pos.z = Mathf.Clamp(pos.z, min.z, max.z);

        // Push out of obstacles
        if (obstacles != null)
        {
            foreach (GameObject obs in obstacles)
            {
                BoxCollider box = obs.GetComponent<BoxCollider>();
                if (box != null && box.bounds.Contains(pos))
                {
                    Vector3 closest = box.bounds.ClosestPoint(pos);
                    Vector3 delta = pos - closest;

                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.z))
                        pos.x = closest.x + Mathf.Sign(delta.x) * buffer;
                    else
                        pos.z = closest.z + Mathf.Sign(delta.z) * buffer;
                }
            }
        }

        return pos;
    }
}