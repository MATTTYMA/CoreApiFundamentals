using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [Route("api/camps")]
    [ApiController]
    [ApiVersion("2.0")]
    public class CampsControllerV2 : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsControllerV2(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this._repository = repository;
            this._mapper = mapper;
            this._linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<IActionResult> Get(bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsAsync(includeTalks);
                if (results == null) return NotFound();
                var returnResult = new
                {
                    Count = results.Count(),
                    Result = results
                };
                return Ok(returnResult);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var result = await _repository.GetCampAsync(moniker);
                if (result == null) return NotFound();
                return _mapper.Map<CampModel>(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }

        [HttpGet("search/{theDate}")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsByEventDate(theDate, includeTalks);
                if (results == null) return NotFound();
                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CampModel>> Post([FromBody] CampModel campModel)
        {
            try
            {
                var existCamp = await _repository.GetCampAsync(campModel.Moniker);
                if (existCamp != null) return BadRequest("Moniker already in use");
                var location = _linkGenerator.GetPathByAction("Get", "Camps", new { moniker = campModel.Moniker });
                if (string.IsNullOrWhiteSpace(location)) return BadRequest("Could not use current moniker");
                var camp = _mapper.Map<Camp>(campModel);
                _repository.Add(camp);
                if (await _repository.SaveChangesAsync())
                {
                    return Created(location, _mapper.Map<CampModel>(camp));
                }
                return BadRequest("Operation Failed");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }
        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel camp)
        {
            try
            {
                var existCamp = await _repository.GetCampAsync(moniker);
                if (existCamp == null) return NotFound("Camp with this moniker not found");
                _mapper.Map(camp, existCamp);
                if (await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<CampModel>(existCamp);
                }
                return BadRequest();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var existCamp = await _repository.GetCampAsync(moniker);
                if (existCamp == null) return NotFound("Camp with this moniker not found");
                _repository.Delete(existCamp);
                if (await _repository.SaveChangesAsync())
                {
                    return Ok();
                }
                return BadRequest();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }
    }
}
