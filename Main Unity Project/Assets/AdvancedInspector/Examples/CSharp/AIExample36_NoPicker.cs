using UnityEngine;

using AdvancedInspector;

[AdvancedInspector]
public class AIExample36_NoPicker : MonoBehaviour
{
    [Inspect, NoPicker]
    public Camera Camera
    {
        get { return Camera.main; }
    }
}
