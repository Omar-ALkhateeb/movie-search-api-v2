using LiteDB;
using Movie.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movie.DB
{
    public interface IEntityDataAcess<T>
    {
        public void BulkInsert(T[] ts);
        public T UpdateOne(T t);
        public T FindOne(string id);
        public T[] FindAll();
    }


    public class MovieEntityAcessLayer : IEntityDataAcess<MovieEntity>
    {
        private readonly ILiteCollection<MovieEntity> _col;
        public MovieEntityAcessLayer(LiteDatabase db)
        {
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
            foreach (var m in res)
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
