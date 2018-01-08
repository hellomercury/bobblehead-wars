using UnityEngine;

public class Utility
{
    /// <summary>
    /// Find and return a child with given tag of the given parent game object.
    /// </summary>
    /// <param name="parent">The parent game object</param>
    /// <param name="tag">The tag of the child game object</param>
    /// <returns>A child with given tag</returns>
    public static GameObject FindChildWithTag(GameObject parent, string tag)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.gameObject.CompareTag(tag))
            {
                return child.gameObject;
            }
        }

        return null;
    }

    public static Vector3 ClonePosition(Vector3 original)
    {
        return new Vector3(original.x, original.y, original.z);
    }

    public static Quaternion CloneRotation(Quaternion original)
    {
        return new Quaternion(original.x, original.y, original.z, original.w);
    }
}