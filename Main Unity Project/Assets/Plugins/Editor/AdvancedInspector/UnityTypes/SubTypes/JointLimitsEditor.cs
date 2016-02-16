using System;
using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace AdvancedInspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(JointLimits), true)]
    public class JointLimitsEditor : InspectorEditor
    {
        protected override void RefreshFields()
        {
            Type type = typeof(JointLimits);

            fields.Add(new InspectorField(parent, type, Instances, type.GetProperty("min"),
                new DescriptorAttribute("Minimum", "The lower limit of the joint.", "http://docs.unity3d.com/ScriptReference/JointLimits-min.html")));
            fields.Add(new InspectorField(parent, type, Instances, type.GetProperty("max"),
                new DescriptorAttribute("Maximum", "The upper limit of the joint.", "http://docs.unity3d.com/ScriptReference/JointLimits-max.html")));
            fields.Add(new InspectorField(parent, type, Instances, type.GetProperty("bounciness"),
                new DescriptorAttribute("Bounciness", "Determines the size of the bounce when the joint hits it's limit. Also known as restitution.", "http://docs.unity3d.com/ScriptReference/JointLimits-bounciness.html")));
            fields.Add(new InspectorField(parent, type, Instances, type.GetProperty("bounceMinVelocity"),
                new DescriptorAttribute("Bounce Min Velocity", "The minimum impact velocity which will cause the joint to bounce.", "http://docs.unity3d.com/ScriptReference/JointLimits-bounceMinVelocity.html")));
            fields.Add(new InspectorField(parent, type, Instances, type.GetProperty("contactDistance"),
                new DescriptorAttribute("Contact Distance", "Distance inside the limit value at which the limit will be considered to be active by the solver.", "http://docs.unity3d.com/ScriptReference/JointLimits-contactDistance.html")));
        }
    }
}