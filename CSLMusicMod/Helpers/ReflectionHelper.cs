using System;
using System.Reflection;
using UnityEngine;

namespace CSLMusicMod
{
    /// <summary>
    /// Helpers to improve QOL with reflection
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// Gets a private field of an object
        /// </summary>
        /// <returns>The private field.</returns>
        /// <param name="instance">The object</param>
        /// <param name="name">Name of the private field</param>
        /// <typeparam name="T">Type of the private field</typeparam>
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

        /// <summary>
        /// Gets a private static field of a class
        /// </summary>
        /// <returns>The private static field.</returns>
        /// <param name="type">Class with the private field</param>
        /// <param name="name">Name of the field</param>
        /// <typeparam name="T">Type of the field</typeparam>
        public static T GetPrivateStaticField<T>(Type type, String name)
        {
            object obj = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static).
                GetValue(null);

            if (obj == null)
                return default(T);

            return (T)obj;
        }

        /// <summary>
        /// Sets a private field.
        /// </summary>
        /// <param name="instance">The object</param>
        /// <param name="name">Name of the private field</param>
        /// <param name="value">Value to be set</param>
        public static void SetPrivateField(object instance, String name, object value)
        {
            instance.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).
                SetValue(instance, value);
        }

        /// <summary>
        /// Sets a private static field of a class.
        /// </summary>
        /// <param name="type">The class</param>
        /// <param name="name">Name of the private static field</param>
        /// <param name="value">Value to be set</param>
        public static void SetPrivateStaticField(Type type, String name, object value)
        {
            type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static).
                SetValue(null, value);
        }

		/// <summary>
		/// Invokes a private function.
		/// </summary>
		/// <returns>Return value of the procedure</returns>
		/// <param name="instance">Object with the private method</param>
		/// <param name="method">Name of the method</param>
		/// <param name="parameters">Parameters of the method</param>
		/// <typeparam name="T">Return type of the method</typeparam>
		public static T InvokePrivateMethod<T>(object instance, String method, params object[] parameters)
        {
            return (T)instance.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, parameters);
        }

		/// <summary>
		/// Invokes a private procedure.
		/// </summary>
		/// <param name="instance">Object with the private method.</param>
		/// <param name="method">Name of the method.</param>
		/// <param name="parameters">Parameters of the method.</param>
		public static void InvokePrivateVoidMethod(object instance, String method, params object[] parameters)
        {
            instance.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, parameters);
        }
    }
}

