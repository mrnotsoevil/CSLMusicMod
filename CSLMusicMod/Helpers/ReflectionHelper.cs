using System;
using System.Reflection;
using UnityEngine;

namespace CSLMusicMod.Helpers
{
    public static class ReflectionHelper
    {
        public static T GetPrivateField<T>(object instance, String name)
        {
            if (instance == null)
            {
                Debug.LogError("GetPrivateField: Instance is null!");
                return default(T);
            }

            FieldInfo field = instance.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                Debug.LogError("GetPrivateField: Field is null!");
                return default(T);
            }

            object obj = field.GetValue(instance);

            if (obj == null)
                return default(T);

            return (T)obj;
        }

        public static T GetPrivateStaticField<T>(Type type, String name)
        {
            object obj = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static).
                GetValue(null);

            if (obj == null)
                return default(T);

            return (T)obj;
        }

        public static void SetPrivateField(object instance, String name, object value)
        {
            instance.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).
                SetValue(instance, value);
        }

        public static void SetPrivateStaticField(Type type, String name, object value)
        {
            type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static).
                SetValue(null, value);
        }
    }
}

