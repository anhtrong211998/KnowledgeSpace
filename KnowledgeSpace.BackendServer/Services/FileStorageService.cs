using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KnowledgeSpace.BackendServer.Services
{
    public class FileStorageService : IStorageService
    {
        private readonly string _userContentFolder;
        private const string USER_CONTENT_FOLDER_NAME = "user-attachments";

        public FileStorageService(IWebHostEnvironment webHostEnvironment)
        {
            _userContentFolder = Path.Combine(webHostEnvironment.WebRootPath, USER_CONTENT_FOLDER_NAME);
        }

        /// <summary>
        /// GET URL OF FILE
        /// </summary>
        /// <param name="fileName">NAME OF FILE</param>
        /// <returns></returns>
        public string GetFileUrl(string fileName)
        {
            return $"/{USER_CONTENT_FOLDER_NAME}/{fileName}";
        }

        /// <summary>
        /// SAVE FILE
        /// </summary>
        /// <param name="mediaBinaryStream">Stream</param>
        /// <param name="fileName">NAME OF FILE</param>
        /// <returns></returns>
        public async Task SaveFileAsync(Stream mediaBinaryStream, string fileName)
        {
            if (!Directory.Exists(_userContentFolder))
                Directory.CreateDirectory(_userContentFolder);

            var filePath = Path.Combine(_userContentFolder, fileName);
            using var output = new FileStream(filePath, FileMode.Create);
            await mediaBinaryStream.CopyToAsync(output);
        }

        /// <summary>
        /// DELETE FILE 
        /// </summary>
        /// <param name="fileName">NAME OF FILE</param>
        /// <returns></returns>
        public async Task DeleteFileAsync(string fileName)
        {
            var filePath = Path.Combine(_userContentFolder, fileName);
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }
    }
}
