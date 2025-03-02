﻿using System;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;

namespace SpecFlow.VisualStudio.Analytics
{
    public interface IUserUniqueIdStore
    {
        string GetUserId();
    }

    [Export(typeof(IUserUniqueIdStore))]
    public class FileUserIdStore : IUserUniqueIdStore
    {
        public static readonly string UserIdFilePath = Environment.ExpandEnvironmentVariables(@"%APPDATA%\SpecFlow\userid");

        private readonly Lazy<string> _lazyUniqueUserId;
        private readonly IFileSystem _fileSystem;

        [ImportingConstructor]
        public FileUserIdStore(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _lazyUniqueUserId = new Lazy<string>(FetchAndPersistUserId);
        }

        public string GetUserId()
        {
            return _lazyUniqueUserId.Value;
        }
        
        private string FetchAndPersistUserId()
        {
            if (_fileSystem.File.Exists(UserIdFilePath))
            {
                var userIdStringFromFile = _fileSystem.File.ReadAllText(UserIdFilePath);
                if (IsValidGuid(userIdStringFromFile))
                {
                    return userIdStringFromFile;
                }
            }

            return GenerateAndPersistUserId();
        }

        private void PersistUserId(string userId)
        {
            var directoryName = Path.GetDirectoryName(UserIdFilePath);
            if (!_fileSystem.Directory.Exists(directoryName))            
            {
                _fileSystem.Directory.CreateDirectory(directoryName);
            }

            _fileSystem.File.WriteAllText(UserIdFilePath, userId);
        }

        private bool IsValidGuid(string guid)
        {
            return Guid.TryParse(guid, out var parsedGuid);
        }

        private string GenerateAndPersistUserId()
        {
            var newUserId = Guid.NewGuid().ToString();

            PersistUserId(newUserId);

            return newUserId;
        }
    }
}
