﻿using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    [ApiController] // this will help to understand/validate the 'VillaDTO'
    public class VillaAPIController : ControllerBase
    {
        protected APIResponse _response;
        // inject db context
        private readonly IVillaRepository _dbVilla;
        // inject auto-mapper
        private readonly IMapper _mapper;
        public VillaAPIController(IVillaRepository dbVilla, IMapper mapper)
        {
            _dbVilla = dbVilla;
            _mapper = mapper;
            this._response = new();
        }

        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetVillas()
        {
            try
            {
                IEnumerable<Villa> villasList = await _dbVilla.GetAllAsync();
                _response.Result = _mapper.Map<List<VillaDTO>>(villasList);
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _response.Result = _mapper.Map<VillaDTO>(villa);
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {
            try
            {
                if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("CustomError", "Villa already Exists!");
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string>() { "Villa already Exists!" };
                    return BadRequest(_response);
                }
                if (createDTO == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string>() { "Null Villa Object" };
                    return BadRequest(_response);
                }

                // use mapper rather than manual mapping
                Villa villa = _mapper.Map<Villa>(createDTO);
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

                await _dbVilla.CreateAsync(villa);

                _response.Result = _mapper.Map<VillaDTO>(villa);
                _response.StatusCode = HttpStatusCode.Created;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string>() { "Id cannot be 0" };
                    return BadRequest(_response);
                }
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string>() { $"Villa with Id {id} not found" };
                    return NotFound(_response);
                }

                await _dbVilla.RemoveAsync(villa);

                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.Created;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            try
            {
                if (id == 0)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string>() { "Id cannot be 0" };
                    return BadRequest(_response);
                }
                if (updateDTO == null || id != updateDTO.Id)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string>() { "id or villa cannot be null" };
                    return BadRequest(_response);
                }
                var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);
                if (villa == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string>() { $"Villa with Id {id} not found" };
                    return NotFound(_response);
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

                await _dbVilla.UpdateAsync(model);

                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpPatch("{id:int}", Name = "PartiallyUpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PartiallyUpdateVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            if (patchDTO == null || id == 0)
            {
                return BadRequest("patchDTO/id cannot be null");
            }

            ///// no tracking the villa object
            var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);
            if (villa == null)
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

            await _dbVilla.UpdateAsync(model);
            return NoContent();
        }
    }
}
