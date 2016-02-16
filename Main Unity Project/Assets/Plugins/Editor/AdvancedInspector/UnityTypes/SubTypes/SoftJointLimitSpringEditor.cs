using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace AdvancedInspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SoftJointLimitSpring), true)]
    public class SoftJointLimitSpringEditor : InspectorEditor
    {
        protected override void RefreshFields()
        {
            Type type = typeof(SoftJointLimitSpring);

            fields.Add(new InspectorField(parent, type, Instances, type.GetProperty("damper"),
                new DescriptorAttribute("Damper", "If spring is greater than zero, the limit is soft.", "http://docs.unity3d.com/ScriptReference/SoftJointLimit-damper.html")));
            fields.Add(new InspectorField(parent, type, Instances, type.GetProperty("limit"),
                new DescriptorAttribute("Spring", "If greater than zero, the limit is soft. The spring will pull the joint back.", "http://docs.unity3d.com/ScriptReference/SoftJointLimit-spring.html")));
        }
    }
}