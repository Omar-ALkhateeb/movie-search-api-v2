using Elasticsearch.Net;
using LiteDB;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Movie.Base
{
    [ElasticsearchType(RelationName = "employee")]
    public class MovieEntity
    {
        public ObjectId Id { get; set; }
        [Text(Name = "movieName")]
        public string MovieName { get; set; }
        [Text(Name = "year")]
        public string Year { get; set; }
        [Text(Name = "genre")]
        public string[] Genre { get; set; }
        public int Clicks { get; set; }
        public int Views { get; set; }

        public static bool IsEmpty(ILiteCollection<MovieEntity> col)
        {
            return col.FindAll().Count() == 0;
        }

        public static void SeedDB(ILiteCollection<MovieEntity> col)
        {
            // read data from dat file
            var lines = File.ReadAllLines("./movies.dat");
            // Console.WriteLine(lines);
            List<MovieEntity> movies = new List<MovieEntity>();

            // serialize the data into Movie objs
            foreach (string line in lines)
            {
                var movieData = line.Split("::").ToList();
                // Console.WriteLine(movieData[1].Split('(')[1].Trim(')'));
                movies.Add(new MovieEntity
                {
                    Clicks = 0,
                    Views = 0,
                    Year = movieData[1].Split('(')[1].Trim(')'),
                    Genre = movieData[2].Split('|'),
                    MovieName = movieData[1].Split('(')[0]
                });
            }

            col.InsertBulk(movies);
        }
        public static MovieEntity AddClick(ILiteCollection<MovieEntity> col, string movieName)
        {
            var movie = col.FindOne(x => x.MovieName == movieName);
            if (movie != null)
            {
                movie.Clicks += 1;
                col.Update(movie);
            }
            return movie;
        }
        public static void AddView(ILiteCollection<MovieEntity> col, string[] movieNames)
        {
            foreach (string movieName in movieNames)
            {
                var movie = col.FindOne(x => x.MovieName == movieName);
                if (movie != null)
                {
                    movie.Views += 1;
                    col.Update(movie);
                }
            }
        }
        public static void SeedSearch(ElasticClient client, ILiteCollection<MovieEntity> col)
        {
            // delete all data retrive the new values from db then bulk insert it
            client.DeleteByQuery<MovieEntity>(del => del
                .Query(q => q.MatchAll())
            );
            client.Bulk(b => b
                .IndexMany<MovieEntity>(col.FindAll().ToArray())
                .Refresh(Refresh.WaitFor)
            );
        }
        public static ISearchResponse<MovieEntity> Search(ElasticClient client, string term)
        {
            // TODO optimize these parameters
            var json = @"{
            ""function_score"": {
                ""query"": {
                ""query_string"": {
                    ""query"": """ + term + @""",
                    ""fields"": [""movieName^5"", ""genre^2""]
                }
                },
                ""script_score"": {
                ""script"": {
                    ""lang"": ""painless"",
                    ""inline"": ""_score + 20*doc['clicks'].value + 40 * doc['views'].value""
                }
                },
                ""score_mode"": ""max"",
                ""boost_mode"": ""multiply""
            }
        }";


            var response = client.Search<MovieEntity>(s => s.Query(q => q.Raw(json)));

            Console.WriteLine(response.OriginalException);
            // Console.WriteLine(response.IsValid);
            return response;
        }
    }
}

