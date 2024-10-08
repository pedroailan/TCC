using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace TCC.Commons
{
    public class Builder
    {
        public static ConnectionFactory Build(IConfiguration configuration)
        {
            return new ConnectionFactory()
            {
                HostName = configuration["Host"],
                Port = int.Parse(configuration["Port"]),
                UserName = configuration["User"],
                Password = configuration["Password"],
            };
        }
    }
}