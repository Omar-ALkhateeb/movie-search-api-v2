using LiteDB;
using Movie.Base;
using System;

namespace Movie.DB
{
    public class DBC
    {
        private readonly ILiteCollection<MovieEntity> _col;
        public DBC()
        {
            var db = new LiteDatabase(@"Movies.db");
            _col = db.GetCollection<MovieEntity>("movies");
        }
        public ILiteCollection<MovieEntity> Create()
        {
            return _col;
        }
    }
}
