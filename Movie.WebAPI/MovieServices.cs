using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Movie.Base;
using Movie.DB;
using Nest;

namespace Movie.WebAPI
{
    public class MovieServices : IMovieServices
    {
        private readonly IDBC<MovieEntity> _db;
        private readonly ElasticClient _elasticClient;
        public MovieServices(IDBC<MovieEntity> db, ElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
            _db = db;
        }
        public MovieEntity AddClick(string movieName)
        {
            var movie = _db.FindOne(movieName);
            if (movie != null)
            {
                movie.Clicks += 1;
                _db.UpdateOne(movie);
            }
            return movie;
        }

        public void AddView(string[] movieNames)
        {
            foreach (string movieName in movieNames)
            {
                var movie = _db.FindOne(movieName);
                if (movie != null)
                {
                    movie.Views += 1;
                    _db.UpdateOne(movie);
                }
            }
 
        }

        public bool IsEmpty()
        {
            return _db.FindAll().Count() == 0;
        }

        public ISearchResponse<MovieEntity> Search(string term)
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


            var response = _elasticClient.Search<MovieEntity>(s => s.Query(q => q.Raw(json)));

            Console.WriteLine(response.OriginalException);
            // Console.WriteLine(response.IsValid);
            return response;
        }

        public void SeedDB()
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

            _db.BulkInsert(movies.ToArray());
        }
    }
}
