using NotesAndTagsApp.DataAccess.Interfaces;
using NotesAndTagsApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesAndTagsApp.Tests.FakeRepositories
{
    public class FakeNoteRepository : IRepository<Note>
    {
        private List<Note> notes;
        public FakeNoteRepository()
        {
            notes = new List<Note>()
            {
                new Note()
                {
                    Id = 1,
                    Tag = Domain.Enums.TagEnum.Exercise,
                    Text = "Do the homework",
                    Priority = Domain.Enums.PriorityEnum.High,
                    UserId = 1,
                    User = new User
                    {
                        Id = 1,
                        Firstname = "Kire",
                        Lastname = "Joldashev",
                        Username = "KireJ"
                    }
                },
                new Note()
                {
                    Id = 2,
                    Tag = Domain.Enums.TagEnum.Health,
                    Text = "Drink water",
                    Priority = Domain.Enums.PriorityEnum.Medium,
                    UserId = 2
                }
            };
        }

        public void Add(Note entity)
        {
            notes.Add(entity);
        }

        public void Delete(Note entity)
        {
            notes.Remove(entity);
        }

        public List<Note> GetAll()
        {
            return notes;
        }

        public Note GetById(int id)
        {
            return notes.FirstOrDefault(note => note.Id == id);
        }

        public void Update(Note entity)
        {
            notes[notes.IndexOf(entity)] = entity;
        }





    }
}
