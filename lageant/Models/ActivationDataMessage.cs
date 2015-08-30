using System;

namespace lageant.Models
{
    /// <summary>
    ///     Object for starting the application per file extension (double click).
    /// </summary>
    [Serializable]
    public class ActivationDataMessage
    {
        /// <summary>
        ///     The whole query string.
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        ///     The file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///     The absolute path to the file.
        /// </summary>
        public string AbsolutePath { get; set; }
    }
}