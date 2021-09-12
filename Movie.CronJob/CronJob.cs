using Elasticsearch.Net;
using LiteDB;
using Movie.Base;
using Movie.DB;
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
        public async Task CreateJob(ElasticClient esclient, IDBC<MovieEntity> db)
        {
            // create job
            IJobDetail job = JobBuilder.Create<Job>()
                .WithIdentity("job1", "group1")
                .Build();

            // pass the db and elastic client to my job
            job.JobDataMap["client"] = esclient;
            job.JobDataMap["db"] = db;


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
    public class Job : IJob
    {
        Task IJob.Execute(IJobExecutionContext context)
        {
            var client = context.JobDetail.JobDataMap["client"] as ElasticClient;
            var db = context.JobDetail.JobDataMap["db"] as IDBC<MovieEntity>;

            //var results = col.FindAll();
            // Console.WriteLine(results.ToList()[0].MovieName);
            SeedSearch(client, db);
            Console.WriteLine("updating elasticsearch is done");
            return null;
        }

        public static void SeedSearch(ElasticClient client, IDBC<MovieEntity>  db)
        {
            // delete all data retrive the new values from db then bulk insert it
            client.DeleteByQuery<MovieEntity>(del => del
                .Query(q => q.MatchAll())
            );
            client.Bulk(b => b
                .IndexMany<MovieEntity>(db.FindAll())
                .Refresh(Refresh.WaitFor)
            );
        }
    }
}
