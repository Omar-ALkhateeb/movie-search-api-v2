using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movie.Base
{
    public interface IMovieServices
    {
        public abstract bool IsEmpty();
        public abstract void SeedDB();
        public abstract MovieEntity AddClick(string movieName);
        public abstract void AddView(string[] movieNames);
        public abstract ISearchResponse<MovieEntity> Search(string term);
    }
}
