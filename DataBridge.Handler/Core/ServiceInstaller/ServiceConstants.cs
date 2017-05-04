namespace DataBridge.Services
{
    ///<summary>
    /// How the service starts. Example would be at boot or automatic.
    ///</summary>
    public enum ServiceStartMode
    {
        Automatic,
        Boot,
        System,
        Manual,
        Disabled,
    }

    /// <summary>
    /// The return code from the WMI Class Win32_Service
    /// </summary>
    public enum ServiceReturnCode
    {
        Success = 0,
        NotSupported = 1,
        AccessDenied = 2,
        DependentServicesRunning = 3,
        InvalidServiceControl = 4,
        ServiceCannotAcceptControl = 5,
        ServiceNotActive = 6,
        ServiceRequestTimeout = 7,
        UnknownFailure = 8,
        PathNotFound = 9,
        ServiceAlreadyRunning = 10,
        ServiceDatabaseLocked = 11,
        ServiceDependencyDeleted = 12,
        ServiceDependencyFailure = 13,
        ServiceDisabled = 14,
        ServiceLogonFailure = 15,
        ServiceMarkedForDeletion = 16,
        ServiceNoThread = 17,
        StatusCircularDependency = 18,
        StatusDuplicateName = 19,
        StatusInvalidName = 20,
        StatusInvalidParameter = 21,
        StatusInvalidServiceAccount = 22,
        StatusServiceExists = 23,
        ServiceAlreadyPaused = 24
    }

    /// <summary>
    /// What type of service is it? Most of the time it will be OwnProcess
    /// </summary>
    public enum ServiceType
    {
        KernalDriver = 1,
        FileSystemDriver = 2,
        Adapter = 4,
        RecognizerDriver = 8,
        OwnProcess = 16,
        ShareProcess = 32,
        InteractiveProcess = 256,
    }

    internal enum ServiceErrorControl
    {
        UserNotNotified = 0,
        UserNotified = 1,
        SystemRestartedWithLastKnownGoodConfiguration = 2,
        SystemAttemptsToStartWithAGoodConfiguration = 3
    }
}