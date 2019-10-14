namespace TagBites.Utils
{
    internal static class ErrorMessages
    {
        public static string UnexpectedFinalizerCalled(string objectName)
        {
            return $"Unexpected finalizer called on IDisposable object {objectName}.";
        }
    }
}
