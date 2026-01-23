using System;
using UnityEngine;

namespace KVSaveSystem
{
    [Serializable]
    public struct ArchiveSetting : IArchiveSetting
    {
        [SerializeField]
        private ArchiveOperationType _archiveOperationMode;
        public ArchiveOperationType ArchiveOperationMode => _archiveOperationMode;

        [SerializeField]
        private bool _isForceSaveSync;
        public bool IsForceSaveSync => _isForceSaveSync;
        
        [SerializeField]
        private bool _isUserArchive;
        public bool IsUserArchive => _isUserArchive;
        
        [SerializeField]
        private bool _isLazyLoad;
        public bool IsLazyLoad => _isLazyLoad;

        public ArchiveSetting(ArchiveOperationType archiveOperationMode, 
            bool isForceSaveSync = false, 
            bool isUserArchive = false, 
            bool isLazyLoad = false)
        {
            _archiveOperationMode = archiveOperationMode;
            _isForceSaveSync = isForceSaveSync;
            _isUserArchive = isUserArchive;
            _isLazyLoad = isLazyLoad;
        }
    }
}