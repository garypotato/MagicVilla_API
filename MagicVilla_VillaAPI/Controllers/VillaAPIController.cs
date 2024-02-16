using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    [ApiController] // this will help to understand/validate the 'VillaDTO'
    public class VillaAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public VillaAPIController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
        {
            return Ok(_db.Villas.ToList());
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

            var villa=_db.Villas.FirstOrDefault(u => u.Id == id);
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
            if(_db.Villas.FirstOrDefault(u => u.Name.ToLower()==villaDTO.Name.ToLower()) != null)
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
            Villa model = new()
            {
                Amenity = villaDTO.Amenity,
                Name = villaDTO.Name,
                Occupancy = villaDTO.Occupancy,
                Sqft = villaDTO.Sqft,
                ImageUrl = villaDTO.ImageUrl,
                Details = villaDTO.Details,
                Id = villaDTO.Id,
                Rate = villaDTO.Rate
            };
            _db.Villas.Add(model);
            _db.SaveChanges();

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
            var villa = _db.Villas.FirstOrDefault(u => u.Id == id);
            if(villa==null)
            {
                return NotFound($"Villa with Id {id} not found");
            }
            _db.Villas.Remove(villa);
            _db.SaveChanges();

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
            var villa = _db.Villas.FirstOrDefault(u => u.Id == id);
            if(villa==null)
            {
                return NotFound($"Villa with Id {id} not found");
            }
            Villa model = new()
            {
                Amenity = villaDTO.Amenity,
                Name = villaDTO.Name,
                Occupancy = villaDTO.Occupancy,
                Sqft = villaDTO.Sqft,
                ImageUrl = villaDTO.ImageUrl,
                Details = villaDTO.Details,
                Id = villaDTO.Id,
                Rate = villaDTO.Rate
            };
            _db.Villas.Update(model);
            _db.SaveChanges();
     
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

            var villa = _db.Villas.FirstOrDefault(u => u.Id == id);
            if (villa==null)
            {
                return NotFound($"Villa with Id {id} not found");
            }
            VillaDTO villaDTO = new()
            {
                Amenity = villa.Amenity,
                Name = villa.Name,
                Occupancy = villa.Occupancy,
                Sqft = villa.Sqft,
                ImageUrl = villa.ImageUrl,
                Details = villa.Details,
                Id = villa.Id,
                Rate = villa.Rate
            };

            // apply to villa object, if any error store in 'ModelState'
            patchDTO.ApplyTo(villaDTO, ModelState);
            Villa model = new()
            {
                Amenity = villa.Amenity,
                Name = villa.Name,
                Occupancy = villa.Occupancy,
                Sqft = villa.Sqft,
                ImageUrl = villa.ImageUrl,
                Details = villa.Details,
                Id = villa.Id,
                Rate = villa.Rate
            };
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _db.Villas.Update(model);
            _db.SaveChanges();
            return NoContent();
        }
    }
}
