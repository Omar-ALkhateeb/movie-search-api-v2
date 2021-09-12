using LiteDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Movie.Base;
using Nest;
using System;

namespace Movie.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly ILogger<MoviesController> _logger;
        private readonly ElasticClient _client;
        private readonly IMovieServices _movieServices;

        public MoviesController(ILogger<MoviesController> logger, IMovieServices movieServices)
        {
            _logger = logger;
            _movieServices = movieServices;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Get([FromQuery(Name = "term")] string term)
        {
            try
            {
                var response = _movieServices.Search(term);

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
                _movieServices.AddView(movieNames);
                return Ok(response.Documents);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }
        [HttpGet]
        [Route("{name}")]
        public IActionResult GetID(string name)
        {
            try
            {
                // Console.WriteLine(name);
                var resp = _movieServices.AddClick(name);
                if (resp != null)
                    return Ok(resp);
                else
                    return NotFound("no movie with this name exists");
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }
    }
}
