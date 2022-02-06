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
    [Route("api/camps/{moniker}/Talks")]
    [ApiController]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public TalksController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker, bool includeSpeakers = false)
        {
            try
            {
                var talks = await _repository.GetTalksByMonikerAsync(moniker, includeSpeakers);
                return _mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }

        [HttpGet("{talkId:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int talkId, bool includeSpeakers = false)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, talkId, includeSpeakers);
                if (talk == null)
                {
                    return NotFound();
                }
                return _mapper.Map<TalkModel>(talk);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post([FromBody] TalkModel talkModel, string moniker)
        {
            try
            {
                var existCamp = await _repository.GetCampAsync(moniker);
                if (existCamp == null) return BadRequest("Camp does not exist");
                var talk = _mapper.Map<Talk>(talkModel);
                talk.Camp = existCamp;
                if (talkModel.Speaker == null) return BadRequest("Speaker ID is required");
                var speaker = await _repository.GetSpeakerAsync(talkModel.Speaker.SpeakerId);
                if (speaker == null) return BadRequest("Speaker not found");
                talk.Speaker = speaker;
                _repository.Add(talk);
                if (await _repository.SaveChangesAsync())
                {
                    var location = _linkGenerator.GetPathByAction(HttpContext, "Get", values : new { moniker, id = talk.TalkId});
                    return Created(location, _mapper.Map<TalkModel>(talk));
                }
                return BadRequest("Operation Failed");
            }   
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }

        [HttpPut("{talkId:int}")]
        public async Task<ActionResult<TalkModel>> Put(TalkModel talkModel, int talkId, string moniker)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, talkId, true);
                if (talk == null) return NotFound("Talk not found");
                if(talkModel.Speaker != null)
                {
                    var speaker = await _repository.GetSpeakerAsync(talkModel.Speaker.SpeakerId);
                    if (speaker != null)
                    {
                        talk.Speaker = speaker;
                    }
                }
                _mapper.Map(talkModel,talk);
                if (await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<TalkModel>(talk);
                }
                else
                {
                    return BadRequest("Failed to update database");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }

        [HttpDelete("{talkId}")]
        public async Task<IActionResult> Delete(int talkId, string moniker)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, talkId);
                if (talk == null) return NotFound("Failed to find the talk to delete");
                _repository.Delete(talk);
                if (await _repository.SaveChangesAsync())
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Failed to delete talk");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }
    }
}
