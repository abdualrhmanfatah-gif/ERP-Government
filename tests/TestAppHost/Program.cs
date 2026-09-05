using ERP_Government.Shared;

namespace ERP_Government.TestAppHost;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        builder.AddSqlServer(Services.DatabaseServer)
            .AddDatabase(Services.Database);

        builder.Build().Run();
    }
}