internal class ServerHandlerDetails
{
    public ServerHandlerDetails(bool requiresAuthorization)
    {
        this.RequiresAuthorization = requiresAuthorization;
    }

    internal bool RequiresAuthorization { set; get; }
}