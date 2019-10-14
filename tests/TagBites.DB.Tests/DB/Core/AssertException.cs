using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.DB.Tests.DB.Core
{
    /// <summary>
    /// Contains assertion types that are not provided with the standard MSTest assertions.
    /// </summary>
    //internal static class AssertException
    //{
    //    /// <summary>
    //    /// Checks to make sure that the input delegate throws a exception of type TException.
    //    /// </summary>
    //    /// <typeparam name="TException">The type of exception expected.</typeparam>
    //    /// <param name="action">The block of code to execute to generate the exception.</param>
    //    public static void Throws<TException>(Action action)
    //        where TException : Exception
    //    {
    //        try
    //        {
    //            action();
    //        }
    //        catch (Exception ex)
    //        {
    //            Assert.True(ex.GetType() == typeof(TException), $"Expected exception of type {typeof(TException)}  but type of {ex.GetType()} was thrown instead.");
    //            return;
    //        }

    //        Assert.Fail($"Expected exception of type {typeof(TException)} but no exception was thrown.");
    //    }
    //    /// <summary>
    //    /// Checks to make sure that the input delegate throws a exception of type TException.
    //    /// </summary>
    //    /// <typeparam name="TException">The type of exception expected.</typeparam>
    //    /// <param name="actions">The block of code to execute to generate the exception.</param>
    //    public static void Throws<TException>(string expectedMessage, Action action) where TException : System.Exception
    //    {
    //        try
    //        {
    //            action();
    //        }
    //        catch (Exception ex)
    //        {
    //            Assert.True(ex.GetType() == typeof(TException), $"Expected exception of type {typeof(TException)} but type of {ex.GetType()} was thrown instead.");
    //            Assert.Equal(expectedMessage, ex.Message, $"Expected exception with a message of '{expectedMessage}' but exception with message of '{ex.Message}' was thrown instead.");
    //            return;
    //        }

    //        Assert.Fail($"Expected exception of type {typeof(TException)} but no exception was thrown.");
    //    }
    //}
}
