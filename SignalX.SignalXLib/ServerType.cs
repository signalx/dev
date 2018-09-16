namespace SignalXLib.Lib
{
    public enum ServerType
    {
        Default = 0,

        /// <summary>
        ///     Sets up a server where requests should be queued with only one allowed in at a time
        /// </summary>
        SingleAccess,

        /// <summary>
        ///     Sets up a server where authorization is checked before access
        /// </summary>
        Authorized,

        /// <summary>
        ///     Sets up a server where authorization is checked before access and requests should be queued with only one allowed
        ///     in at a time
        /// </summary>
        AuthorizedSingleAccess,
        Dynamic,
        DynamicAuthorized
    }
}