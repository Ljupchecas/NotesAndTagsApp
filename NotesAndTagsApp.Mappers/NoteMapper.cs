using NotesAndTagsApp.Domain.Models;
using NotesAndTagsApp.DTOs;

namespace NotesAndTagsApp.Mappers
{
    public static class NoteMapper
    {
        public static NoteDto ToNoteDto(this Note note)
        {
            return new NoteDto
            {
                Tag = note.Tag,
                Priority = note.Priority,
                Text = note.Text,
                //UserFullName = $"{note.User.Firstname} {note.User.Lastname}"
                UserFullName = "UserFullName"
            };
        }

        public static Note ToNote(this AddNoteDto addNoteDto)
        {
            return new Note()
            {
                Tag = addNoteDto.Tag,
                Priority = addNoteDto.Priority,
                Text = addNoteDto.Text,
                UserId = addNoteDto.UserId
            };
        }
    }
}
