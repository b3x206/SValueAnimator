#if UNITY_EDITOR
using UnityEditor;

using System.Collections.Generic;
using System;
using System.Reflection;
using System.Collections;

namespace BX.Editor.Utility
{
    /// <summary>
    /// Contains utility related to serialized property.
    /// </summary>
    internal static class SerializedPropertyUtility
    {
        /// <summary>
        /// This method allows for copying and iterating a given <see cref="SerializedProperty"/>.
        /// <br>Without this (using the <see cref="IEnumerable.GetEnumerator"/>
        /// of <see cref="SerializedProperty"/>) the entire UI errors out, requiring registry of every single property manually.</br>
        /// <br>While somewhat inconvenient for the editor script, it is more convenient for registering properties that were added later.</br>
        /// </summary>
        public static IEnumerable<SerializedProperty> GetVisibleChildren(SerializedProperty property)
        {
            using SerializedProperty iterProperty = property.Copy();
            using SerializedProperty nextSibling = property.Copy();
            {
                nextSibling.NextVisible(false);
            }

            // This is quite necessary, the SerializedProperty.GetEnumerator() doesn't function as I expect.
            if (iterProperty.NextVisible(true))
            {
                yield return iterProperty;

                while (iterProperty.NextVisible(false) && !SerializedProperty.EqualContents(iterProperty, nextSibling))
                {
                    yield return iterProperty;
                }
            }
        }

        /// <summary>
        /// Get the given underlying FieldInfo object property from <paramref name="property"/>'s property path.
        /// <br>Does not support multiple objects, for that create a custom method that retrieves all serializedObject targets.</br>
        /// </summary>
        /// <param name="property">Property to get it's managed object.</param>
        /// <returns>Managed object that the <paramref name="property"/> is pointing to.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static object GetTarget(SerializedProperty property, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            // not very possible as we will get field type of non monobehaviour / scriptableobject
            if (property.serializedObject.isEditingMultipleObjects)
            {
                throw new ArgumentException("Given property is editing multiple objects", nameof(property));
            }
            // static fields are never SerializedProperty
            if ((flags & BindingFlags.Static) == BindingFlags.Static)
            {
                throw new ArgumentException("Search binding flags have 'Static', which is never possible on unity properties", nameof(flags));
            }

            string[] path = property.propertyPath.Split('.');

            object data = property.serializedObject.targetObject;
            for (int i = 0; i < path.Length; i++)
            {
                string fieldName = path[i];
                Type type = data.GetType();
                FieldInfo info = type.GetField(fieldName, flags) ??
                    throw new Exception($"On type {type}, field '{fieldName}' at '{i}' is invalid or flags don't match for path '{property.propertyPath}'.");

                if (info.FieldType.IsArray)
                {
                    if (i >= (path.Length - 1))
                    {
                        // the target is the array
                        data = info.GetValue(data);
                        continue;
                    }

                    // data is contained within array, we should get it from the cell (2 steps)
                    // {current} -> .Array -> .{data[n]}
                    i += 2;
                    fieldName = path[i]; // if this fails, the SerializableObject.propertyPath is faulty. (unless unity changes everything)
                    int cellIndex = int.Parse(fieldName.Substring(fieldName.IndexOf('[') + 1, fieldName.IndexOf(']') - fieldName.IndexOf('[') - 1));

                    // it could be safer to 
                    // 1. Cast object to System.Collections.IEnumerable
                    // 2. Count until we get data
                    //    though unity's serialized special Array types are always IList
                    //    and list wrappers usually use one of those as backing
                    data = ((IList)info.GetValue(data))[cellIndex];
                    continue;
                }

                data = info.GetValue(data);
            }

            return data;
        }
        /// <inheritdoc cref="GetTarget(SerializedProperty, BindingFlags)"/>
        /// <typeparam name="T">Type of the resulting object.</typeparam>
        public static T GetTarget<T>(SerializedProperty property, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return (T)GetTarget(property, flags);
        }
    }
}
#endif
