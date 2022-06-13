using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;

namespace RestAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
               CreateHostBuilder(args).Build().Run();
        }

        public static void StartCadmanJob(IConfiguration config)
        {
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            var scheduler = schedulerFactory.GetScheduler().Result;
            var configKey = $"Quartz:CadmanJob";
            var cronSchedule = config[configKey];

            ITrigger trigger = TriggerBuilder.Create()
            .WithCronSchedule(cronSchedule)
            .WithPriority(1)
            .Build();

            IJobDetail job = JobBuilder.Create<CadmanJob>()
                        .WithIdentity("Cadman Job")
                        .Build();
            job.JobDataMap.Put("ConnectionString", config.GetConnectionString("DBContextConnectionString"));
            job.JobDataMap.Put("CadmanConnectionString", config.GetConnectionString("CadmanConnectionString"));

            job.JobDataMap.Put("Vault", config.GetSection("Vault").Value);
            job.JobDataMap.Put("CadmanQueueFolder", config.GetSection("CadmanQueueFolder").Value);
            job.JobDataMap.Put("CadmanWatchFolder", config.GetSection("CadmanWatchFolder").Value);
            job.JobDataMap.Put("CadmanSdiWatchFolder", config.GetSection("CadmanSdiWatchFolder").Value);
            job.JobDataMap.Put("LogFolder", config.GetSection("LogFolder").Value);
            job.JobDataMap.Put("ProjectName", config.GetSection("ProjectName").Value);

            job.JobDataMap.Put("PartDownloadFolder", config.GetSection("PartDownloadFolder").Value);

            scheduler.ScheduleJob(job, trigger);
            scheduler.Start();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                    })
                    .UseStartup<Startup>();
                })
                .ConfigureServices((hostContext, services) =>
                {                   
                    StartCadmanJob(hostContext.Configuration);
                }
            );
    }
}
