using LiteDB;
using Movie.Base;
using System;
using System.Collections.Generic;

namespace Movie.DB
{
    public interface IDBC<T>
    {
        public T getDatabse();
    }
    public class LiteDBC : IDBC<LiteDatabase>
    {
        private readonly LiteDatabase _db;
        public LiteDBC()
        {
            _db = new LiteDatabase(@"Movies.db");
        }

        public LiteDatabase getDatabse()
        {
            return _db;
        }
    }
}
