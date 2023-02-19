namespace PNLauncher.Core
{
    using System;

    public enum UpdateStatus
    {
        StartUpdate,
        GettingInfo,
        CheckFiles,
        DownloadFiles,
        CheckSuccess,
        UpdatePaused,
        UpdateEnd,
        UpdateSuccess,
        LastVersion,
        CopyFiles
    }
}

