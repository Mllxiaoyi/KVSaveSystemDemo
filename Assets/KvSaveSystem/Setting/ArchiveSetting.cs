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
        private bool _isLoadAfterLogin;
        public bool IsLoadAfterLogin => _isLoadAfterLogin;

        public ArchiveSetting(ArchiveOperationType archiveOperationMode, bool isForceSaveSync)
        {
            _archiveOperationMode = archiveOperationMode;
            _isForceSaveSync = isForceSaveSync;
            _isLoadAfterLogin = false;
        }
    }
}