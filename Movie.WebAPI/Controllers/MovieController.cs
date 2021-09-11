using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Movie.Base;
using Nest;
using System;

namespace Movie.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[EnableCors("http://localhost:8080")]
    public class MoviesController : ControllerBase
    {
        private readonly ILogger<MoviesController> _logger;
        private readonly ElasticClient _client;
        private readonly ILiteCollection<MovieEntity> _col;

        public MoviesController(ILogger<MoviesController> logger, ElasticClient client, ILiteCollection<MovieEntity> col)
        {
            _logger = logger;
            _client = client;
            _col = col;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Get([FromQuery(Name = "term")] string term)
        {
            var response = MovieEntity.Search(_client, term);

            // turn movies into movienames
            string[] movieNames = new string[10];
            var movies = response.Documents;
            int i = 0;

            foreach (var m in movies)
            {
                movieNames[i] = m.MovieName;
                i++;
            }

            // Console.WriteLine(term);
            MovieEntity.AddView(_col, movieNames);
            return Ok(response.Documents);
        }
        [HttpGet]
        [Route("{name}")]
        public IActionResult GetID(string name)
        {
            // Console.WriteLine(name);
            var resp = MovieEntity.AddClick(_col, name);
            if (resp != null)
                return Ok(resp);
            else
                return NotFound("no movie with this name exists");
        }
    }
}
