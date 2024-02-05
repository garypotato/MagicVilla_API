using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    [ApiController] // this will help to understand/validate the 'VillaDTO'
    public class VillaAPIController : ControllerBase
    {
        //// set up logger
        private readonly ILogging _logger;
        public VillaAPIController(ILogging logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
        {
            _logger.Log("GetVillas() called","");
            return Ok(VillaStore.villaList);
        }

        [HttpGet("{id:int}", Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<VillaDTO> GetVilla(int id)
        {
            if(id==0)
            {
                return BadRequest("Id cannot be 0");
            }

            var villa=VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            if(villa==null)
            {
                return NotFound($"Villa with Id {id} not found");
            }

            return Ok(villa);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VillaDTO> CreateVilla([FromBody]VillaDTO villaDTO)
        {
            if(VillaStore.villaList.FirstOrDefault(u => u.Name.ToLower()==villaDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", "Villa already Exists!");
                return BadRequest(ModelState);
            }
            if(villaDTO == null)
            {
                return BadRequest("Villa cannot be null");
            }
            if(villaDTO.Id>0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Id cannot be greater than 0");
            }

            villaDTO.Id = VillaStore.villaList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
            VillaStore.villaList.Add(villaDTO);

            return CreatedAtRoute("GetVilla", new { id= villaDTO.Id}, villaDTO);
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteVilla(int id)
        {
            if(id==0)
            {
                return BadRequest("Id cannot be 0");
            }
            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            if(villa==null)
            {
                return NotFound($"Villa with Id {id} not found");
            }
            VillaStore.villaList.Remove(villa);
            return NoContent();
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateVilla(int id, [FromBody] VillaDTO villaDTO)
        {
            if(id==0)
            {
                return BadRequest("Id cannot be 0");
            }
            if(villaDTO==null || id != villaDTO.Id)
            {
                return BadRequest("id or villa cannot be null");
            }
            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            if(villa==null)
            {
                return NotFound($"Villa with Id {id} not found");
            }
            villa.Name = villaDTO.Name;
            villa.Occupancy = villaDTO.Occupancy;
            villa.Sqft = villaDTO.Sqft;

            return NoContent();
        }

        [HttpPatch("{id:int}", Name = "PartiallyUpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult PartiallyUpdateVilla(int id, JsonPatchDocument<VillaDTO> patchDTO)
        { 
            if(patchDTO == null || id == 0)
            {
                return BadRequest("patchDTO/id cannot be null");
            }

            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            if(villa==null)
            {
                return NotFound($"Villa with Id {id} not found");
            }

            // apply to villa object, if any error store in 'ModelState'
            patchDTO.ApplyTo(villa, ModelState);
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();
        }
    }
}
