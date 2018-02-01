namespace Swashbuckle.AspNetCore.Chm
{
    public class BasicAuthScheme : SecurityScheme
    {
        public BasicAuthScheme()
        {
            Type = "basic";
        }
    }
}
