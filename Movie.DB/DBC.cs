using LiteDB;
using Movie.Base;
using System;
using System.Collections.Generic;

namespace Movie.DB
{
    public interface IDBC<T>
    {
        public void BulkInsert(T[] ts);
        public T UpdateOne(T t);
        public T FindOne(string id);
        public T[] FindAll();
    }
    public class LiteDBC:IDBC<MovieEntity>
    {
        private readonly ILiteCollection<MovieEntity> _col;
        public LiteDBC()
        {
            var db = new LiteDatabase(@"Movies.db");
            _col = db.GetCollection<MovieEntity>("movies");
        }

        public void BulkInsert(MovieEntity[] movies)
        {
            _col.InsertBulk(movies);
        }

        public MovieEntity[] FindAll()
        {
            var movies = new List<MovieEntity>();
            var res = _col.FindAll();
            foreach(var m in res)
            {
                movies.Add(m);
            }

            return movies.ToArray();
        }

        public MovieEntity FindOne(string movieName)
        {
            return _col.FindOne(x => x.MovieName == movieName);
        }

        public MovieEntity UpdateOne(MovieEntity movie)
        {
            _col.Update(movie);
            return movie;
        }
    }
}
