using LiteDB;
using Nest;

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

    }
}

