namespace FastCdReplace.Core
{
    public sealed class FileProgressInfo
    {
        /// <summary>
        /// Ім'я файла, який зараз обробляється.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Скільки байтів уже прочитано/оброблено для поточного файла.
        /// </summary>
        public long BytesProcessed { get; set; }

        /// <summary>
        /// Загальний розмір поточного файла в байтах.
        /// </summary>
        public long TotalBytes { get; set; }

        /// <summary>
        /// Загальний оброблений обсяг по всіх файлах.
        /// Зручно для одного загального progress bar на формі.
        /// </summary>
        public long TotalProcessedBytesAllFiles { get; set; }

        /// <summary>
        /// Загальний обсяг усіх файлів.
        /// </summary>
        public long TotalBytesAllFiles { get; set; }

        /// <summary>
        /// Відсоток по поточному файлу.
        /// </summary>
        public int FilePercent =>
            TotalBytes <= 0 ? 0 : (int)(BytesProcessed * 100L / TotalBytes);

        /// <summary>
        /// Загальний відсоток по всіх файлах.
        /// </summary>
        public int OverallPercent =>
            TotalBytesAllFiles <= 0 ? 0 : (int)(TotalProcessedBytesAllFiles * 100L / TotalBytesAllFiles);
    }
}