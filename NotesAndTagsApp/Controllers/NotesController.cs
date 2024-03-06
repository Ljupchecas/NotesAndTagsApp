using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotesAndTagsApp.Domain.Enums;
using NotesAndTagsApp.DTOs;
using NotesAndTagsApp.Services.Interfaces;
using NotesAndTagsApp.Shared.CustomExceptions;
using Serilog;
using System.Security.Claims;

namespace Exercise10.NotesAndTagsApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NotesController(INoteService noteService)
        {
            _noteService = noteService;
        }

        private int GetAuthorizedUserId()
        {
            if(!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                string name = User.FindFirst(ClaimTypes.Name)?.Value;
                Log.Error($"{name} identifier claim does not exist!");
                throw new Exception($"{name} identifier claim does not exist!");
            }
            return userId;
        }

        [HttpGet]
        public ActionResult<List<NoteDto>> GetAll()
        {
            try
            {
                var claims = User.Claims;
                string userId = claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                string username = claims.First(x => x.Type == ClaimTypes.Name).Value;
                string userRole = claims.First(x => x.Type == ClaimTypes.Role).Value;

                if(username != "superAdmin" || userRole != "SuperAdmin")
                {
                    Log.Error("The User is not with username superAdmin");
                    return StatusCode(StatusCodes.Status403Forbidden);
                }

                Log.Information("Succesfully retrived notes informations.");
                return Ok(_noteService.GetAllNotes());
            }
            catch (Exception ex)
            {
                Log.Error($"Get All {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("getAllUserNotes")]
        public ActionResult<List<NoteDto>> GetAllUserNotes() 
        {
            try
            {
                var userId = GetAuthorizedUserId();

                Log.Information("Succesfully retrived all user notes informations.");
                return _noteService.GetAllUserNotes(userId);
            }
            catch (Exception ex)
            {
                Log.Error($"Get All {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet("{id}")]
        public ActionResult<NoteDto> GetById(int id)
        {
            try
            {
                var noteDto = _noteService.GetById(id);
                Log.Information($"Succesfully retrived note with id {id}.");
                return Ok(noteDto);
            }
            catch (NoteNotFoundException ex)
            {
                Log.Error($"Get by id NotFound: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error($"Get by id: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("addNote")]
        public IActionResult AddNote([FromBody] AddNoteDto addNoteDto)
        {
            try
            {
                _noteService.AddNote(addNoteDto);
                Log.Information($"Succesfully add note.");
                return StatusCode(StatusCodes.Status201Created, "Note added");
            }
            catch (NoteNotFoundException ex)
            {
                Log.Error($"AddNote BadRequest: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error($"AddNote: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("updateNote")]
        public IActionResult UpdateNote([FromBody] UpdateNoteDto updateNoteDto)
        {
            try
            {
                _noteService.UpdateNote(updateNoteDto);
                Log.Information($"Succesfully update note.");
                return NoContent();
            }
            catch (NoteNotFoundException ex)
            {
                Log.Error($"updateNote NotFound: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (NoteDataException ex)
            {
                Log.Error($"updateNote BadRequest: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error($"updateNote: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteNote(int id)
        {
            try
            {
                _noteService.DeleteNote(id);
                Log.Information($"Succesfully delete note.");
                return Ok($"Note with id {id} was successfully deleted!");
            }
            catch (NoteNotFoundException ex)
            {
                Log.Error($"delete NotFound: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error($"delete: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
