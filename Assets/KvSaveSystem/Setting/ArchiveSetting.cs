using System;
using Sirenix.OdinInspector;
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

        public ArchiveSetting(ArchiveOperationType archiveOperationMode, bool isForceSaveSync)
        {
            _archiveOperationMode = archiveOperationMode;
            _isForceSaveSync = isForceSaveSync;
            _isUserArchive = false;
        }
    }
}