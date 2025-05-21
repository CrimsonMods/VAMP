using System;

namespace VAMP.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class FileReloadAttribute : Attribute
    {
        public string CallbackMethodName { get; }

        /// <summary>
        /// Marks a file path for automatic reload detection.
        /// </summary>
        /// <param name="callbackMethodName">Static method to call when file changes (should be void Method()).
        /// If null, will use "Reload" + the field/property name (e.g., [FileReload] MyConfig → ReloadMyConfig)</param>
        public FileReloadAttribute(string callbackMethodName = null)
        {
            CallbackMethodName = callbackMethodName;
        }
    }
}