/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

// This file contains general utilities to aid in development.
// Classes here generally shouldn't be exposed publicly since
// they're not particular to any library functionality.
// Because the classes here are internal, it's likely this file
// might be included in multiple assemblies.
namespace Standard
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    /// A static class for retail validated assertions.
    /// Instead of breaking into the debugger an exception is thrown.
    /// </summary>
    internal static class Verify
    {
        /// <summary>
        /// Ensure that the current thread's apartment state is what's expected.
        /// </summary>
        /// <param name="requiredState">
        /// The required apartment state for the current thread.
        /// </param>
        /// <param name="message">
        /// The message string for the exception to be thrown if the state is invalid.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the calling thread's apartment state is not the same as the requiredState.
        /// </exception>
        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file."),
            DebuggerStepThrough
        ]
        public static void IsApartmentState(ApartmentState requiredState, string message)
        {
            if (Thread.CurrentThread.GetApartmentState() != requiredState)
            {
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Ensure that an argument is neither null nor empty.
        /// </summary>
        /// <param name="value">The string to validate.</param>
        /// <param name="name">The name of the parameter that will be presented if an exception is thrown.</param>
        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file."),
            SuppressMessage(
                "Microsoft.Performance",
                "CA1820:TestForEmptyStringsUsingStringLength",
                Justification = "The point of this function is to give a better error message."),
            DebuggerStepThrough
        ]
        public static void IsNeitherNullNorEmpty(string value, string name)
        {
            // catch caller errors, mixing up the parameters.  Name should never be empty.
            Assert.IsNeitherNullNorEmpty(name);

            // Notice that ArgumentNullException and ArgumentException take the parameters in opposite order :P
            const string errorMessage = "The parameter can not be either null or empty.";
            if (null == value)
            {
                throw new ArgumentNullException(name, errorMessage);
            }
            if ("" == value)
            {
                throw new ArgumentException(errorMessage, name);
            }
        }

        /// <summary>Verifies that an argument is not null.</summary>
        /// <typeparam name="T">Type of the object to validate.  Must be a class.</typeparam>
        /// <param name="obj">The object to validate.</param>
        /// <param name="name">The name of the parameter that will be presented if an exception is thrown.</param>
        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file."),
            DebuggerStepThrough
        ]
        public static void IsNotNull<T>(T obj, string name) where T : class
        {
            if (null == obj)
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Verifies the specified statement is true.  Throws an ArgumentException if it's not.
        /// </summary>
        /// <param name="statement">The statement to be verified as true.</param>
        /// <param name="name">Name of the parameter to include in the ArgumentException.</param>
        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification="Shared code file."),
            DebuggerStepThrough
        ]
        public static void IsTrue(bool statement, string name)
        {
            if (!statement)
            {
                throw new ArgumentException("", name);
            }
        }

        /// <summary>
        /// Verifies the specified statement is true.  Throws an ArgumentException if it's not.
        /// </summary>
        /// <param name="statement">The statement to be verified as true.</param>
        /// <param name="name">Name of the parameter to include in the ArgumentException.</param>
        /// <param name="message">The message to include in the ArgumentException.</param>
        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file."),
            DebuggerStepThrough
        ]
        public static void IsTrue(bool statement, string name, string message)
        {
            if (!statement)
            {
                throw new ArgumentException(message, name);
            }
        }

        public static void AreNotEqual<T>(T notExpected, T actual, string parameterName, string message)
        {
            if (null == notExpected)
            {
                // Two nulls are considered equal, regardless of type semantics.
                if (null == actual || actual.Equals(notExpected))
                {
                    throw new ArgumentException(message, parameterName);
                }
            }
            else if (notExpected.Equals(actual))
            {
                throw new ArgumentException(message, parameterName);
            }
        }
    }
}
