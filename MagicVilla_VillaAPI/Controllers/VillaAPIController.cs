using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    [ApiController] // this will help to understand/validate the 'VillaDTO'
    public class VillaAPIController : ControllerBase
    {
        // inject db context
        private readonly ApplicationDbContext _db;
        // inject auto-mapper
        private readonly IMapper _mapper;
        public VillaAPIController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
        {
            IEnumerable<Villa> villasList = await _db.Villas.ToListAsync();
            return Ok(_mapper.Map<List<VillaDTO>>(villasList));
        }

        [HttpGet("{id:int}", Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VillaDTO>> GetVilla(int id)
        {
            if(id==0)
            {
                return BadRequest("Id cannot be 0");
            }

            var villa=await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
            if(villa==null)
            {
                return NotFound($"Villa with Id {id} not found");
            }

            return Ok(_mapper.Map<VillaDTO>(villa));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VillaDTO>> CreateVilla([FromBody]VillaCreateDTO createDTO)
        {
            if(await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower()== createDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", "Villa already Exists!");
                return BadRequest(ModelState);
            }
            if(createDTO == null)
            {
                return BadRequest(createDTO);
            }

            // use mapper rather than manual mapping
            Villa model = _mapper.Map<Villa>(createDTO);
            //Villa model = new()
            //{
            //    Amenity = createDTO.Amenity,
            //    Name = createDTO.Name,
            //    Occupancy = createDTO.Occupancy,
            //    Sqft = createDTO.Sqft,
            //    ImageUrl = createDTO.ImageUrl,
            //    Details = createDTO.Details,
            //    Rate = createDTO.Rate
            //};

            await _db.Villas.AddAsync(model);
            await _db.SaveChangesAsync();

            return CreatedAtRoute("GetVilla", new { id= model.Id}, model);
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVilla(int id)
        {
            if(id==0)
            {
                return BadRequest("Id cannot be 0");
            }
            var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
            if(villa==null)
            {
                return NotFound($"Villa with Id {id} not found");
            }
            _db.Villas.Remove(villa);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            if(id==0)
            {
                return BadRequest("Id cannot be 0");
            }
            if(updateDTO == null || id != updateDTO.Id)
            {
                return BadRequest("id or villa cannot be null");
            }
            var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if(villa==null)
            {
                return NotFound($"Villa with Id {id} not found");
            }

            Villa model = _mapper.Map<Villa>(updateDTO);
            //Villa model = new()
            //{
            //    Amenity = updateDTO.Amenity,
            //    Name = updateDTO.Name,
            //    Occupancy = updateDTO.Occupancy,
            //    Sqft = updateDTO.Sqft,
            //    ImageUrl = updateDTO.ImageUrl,
            //    Details = updateDTO.Details,
            //    Rate = updateDTO.Rate,
            //    Id= updateDTO.Id
            //};

            _db.Villas.Update(model);
            await _db.SaveChangesAsync();
     
            return NoContent();
        }

        [HttpPatch("{id:int}", Name = "PartiallyUpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PartiallyUpdateVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        { 
            if(patchDTO == null || id == 0)
            {
                return BadRequest("patchDTO/id cannot be null");
            }

            ///// no tracking the villa object
            var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (villa==null)
            {
                return NotFound($"Villa with Id {id} not found");
            }

            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);
            //VillaUpdateDTO villaDTO = new()
            //{
            //    Amenity = villa.Amenity,
            //    Name = villa.Name,
            //    Occupancy = villa.Occupancy,
            //    Sqft = villa.Sqft,
            //    ImageUrl = villa.ImageUrl,
            //    Details = villa.Details,
            //    Id = villa.Id,
            //    Rate = villa.Rate
            //};

            // apply to villa object, if any error store in 'ModelState'
            patchDTO.ApplyTo(villaDTO, ModelState);

            Villa model = _mapper.Map<Villa>(villaDTO);
            //Villa model = new()
            //{
            //    Amenity = villaDTO.Amenity,
            //    Name = villaDTO.Name,
            //    Occupancy = villaDTO.Occupancy,
            //    Sqft = villaDTO.Sqft,
            //    ImageUrl = villaDTO.ImageUrl,
            //    Details = villaDTO.Details,
            //    Rate = villaDTO.Rate,
            //    Id= villaDTO.Id
            //};

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _db.Villas.Update(model);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
