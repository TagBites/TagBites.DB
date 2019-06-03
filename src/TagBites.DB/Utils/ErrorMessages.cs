// ReSharper disable once CheckNamespace
namespace TBS.Resources
{
    internal static class ErrorMessages
    {
        public static string UnexpectedFinalizerCalled(string objectName)
        {
            return $"Unexpected finalizer called on IDisposable object {objectName}.";
        }
    }
}
