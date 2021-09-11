using LiteDB;
using Movie.Base;
using Nest;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;

namespace Movie.CronJobScheduler
{
    public class CronJob
    {
        private readonly StdSchedulerFactory _factory;
        private IScheduler _scheduler;
        public CronJob()
        {
            // Grab the Scheduler instance from the Factory
            _factory = new StdSchedulerFactory();
        }
        public async Task StartSchedulerAsync()
        {
            _scheduler = await _factory.GetScheduler();

            // and start it off
            await _scheduler.Start();
        }
        public async Task CreateJob(ElasticClient esclient, ILiteCollection<MovieEntity> col)
        {
            // create job
            IJobDetail job = JobBuilder.Create<SimpleJob>()
                .WithIdentity("job1", "group1")
                .Build();

            // pass the db and elastic client to my job
            job.JobDataMap["client"] = esclient;
            job.JobDataMap["col"] = col;


            // create trigger
            Quartz.ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                //.WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever())
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(120).RepeatForever())
                .Build();

            // Schedule the job using the job and trigger 
            await _scheduler.ScheduleJob(job, trigger);
        }
    }
    public class SimpleJob : IJob
    {
        Task IJob.Execute(IJobExecutionContext context)
        {
            var client = context.JobDetail.JobDataMap["client"] as ElasticClient;
            var col = context.JobDetail.JobDataMap["col"] as ILiteCollection<MovieEntity>;

            var results = col.FindAll();
            // Console.WriteLine(results.ToList()[0].MovieName);
            MovieEntity.SeedSearch(client, col);
            Console.WriteLine("sseding task done");
            return null;
        }
    }
}
