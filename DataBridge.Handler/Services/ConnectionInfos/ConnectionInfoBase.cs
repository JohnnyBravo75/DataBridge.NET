namespace DataBridge.ConnectionInfos
{
    using System;
    using System.ServiceModel;
    using Utils;

    [Serializable]
    [ServiceKnownType("GetKnownTypes", typeof(KnownTypesProvider))]
    public abstract class ConnectionInfoBase
    {
    }
}