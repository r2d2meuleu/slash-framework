﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AttributeTableExtensions.cs" company="Slash Games">
//   Copyright (c) Slash Games. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Slash.Collections.AttributeTables
{
    using System;

    /// <summary>
    ///   Extension methods for IAttributeTable.
    ///   Mainly utility methods, e.g. to get values of a specific type.
    /// </summary>
    public static class AttributeTableExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        ///   Tries to get an boolean value from the specified attribute table with the specified key.
        /// </summary>
        /// <param name="attributeTable">Attribute table to work on.</param>
        /// <param name="key">Key of value to get.</param>
        /// <param name="value">Variable to write value to.</param>
        /// <returns>True if the value was found; otherwise, false.</returns>
        public static bool TryGetBool(this IAttributeTable attributeTable, object key, out bool value)
        {
            object objectValue;
            if (!attributeTable.TryGetValue(key, out objectValue))
            {
                value = default(bool);
                return false;
            }

            value = Convert.ToBoolean(objectValue);
            return true;
        }

        /// <summary>
        ///   Tries to get a float value from the specified attribute table with the specified key.
        /// </summary>
        /// <param name="attributeTable">Attribute table to work on.</param>
        /// <param name="key">Key of value to get.</param>
        /// <param name="value">Variable to write value to.</param>
        /// <returns>True if the value was found; otherwise, false.</returns>
        public static bool TryGetFloat(this IAttributeTable attributeTable, object key, out float value)
        {
            object objectValue;
            if (!attributeTable.TryGetValue(key, out objectValue))
            {
                value = default(float);
                return false;
            }

            value = Convert.ToSingle(objectValue);
            return true;
        }

        /// <summary>
        ///   Tries to get an int value from the specified attribute table with the specified key.
        /// </summary>
        /// <param name="attributeTable">Attribute table to work on.</param>
        /// <param name="key">Key of value to get.</param>
        /// <param name="value">Variable to write value to.</param>
        /// <returns>True if the value was found; otherwise, false.</returns>
        public static bool TryGetInt(this IAttributeTable attributeTable, object key, out int value)
        {
            object objectValue;
            if (!attributeTable.TryGetValue(key, out objectValue))
            {
                value = default(int);
                return false;
            }

            value = Convert.ToInt32(objectValue);
            return true;
        }

        /// <summary>
        ///   Tries to retrieve the value the passed key is mapped to within this
        ///   attribute table and expects the specified type.
        /// </summary>
        /// <param name="attributeTable">Attribute table to work on.</param>
        /// <param name="key"> Key to retrieve the value of. </param>
        /// <param name="type">Expected value type.</param>
        /// <param name="value"> Retrieved value. </param>
        /// <returns> true if a value was found, and false otherwise. </returns>
        public static bool TryGetValue(this IAttributeTable attributeTable, object key, Type type, out object value)
        {
            if (!attributeTable.TryGetValue(key, out value))
            {
                return false;
            }

            // Convert value.
            value = Convert.ChangeType(value, type);

            return true;
        }

        /// <summary>
        ///   Tries to retrieve the value the specified key is mapped to within this
        ///   attribute table. Additionally checks that the type of the value is correct.
        ///   Only classes are allowed as the method wouldn't be AOT compatible otherwise.
        /// </summary>
        /// <typeparam name="T">Expected type of value.</typeparam>
        /// <param name="attributeTable">Attribute table to work on.</param>
        /// <param name="key"> Key to retrieve the value of. </param>
        /// <param name="value"> Retrieved value. </param>
        /// <returns> true if a value was found, and false otherwise. </returns>
        public static bool TryGetValue<T>(this IAttributeTable attributeTable, object key, out T value)
        {
            // Try get value.
            object objectValue;
            if (!attributeTable.TryGetValue(key, out objectValue))
            {
                value = default(T);
                return false;
            }

            // Check if of expected type.
            if (!(objectValue is T))
            {
                value = default(T);
                return false;
            }

            value = (T)objectValue;
            return true;
        }

        #endregion
    }
}