using Moq;
using NotesAndTagsApp.DataAccess.Interfaces;
using NotesAndTagsApp.Domain.Models;
using NotesAndTagsApp.DTOs;
using NotesAndTagsApp.Services.Implementation;
using NotesAndTagsApp.Services.Interfaces;
using NotesAndTagsApp.Shared.CustomExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace NotesAndTagsApp.Tests
{
    [TestClass]
    public class NoteServiceUnitTest
    {
        private readonly INoteService _noteService;
        private readonly Mock<IRepository<Note>> _noteRepository;
        private readonly Mock<IUserRepository> _userRepository;

        public NoteServiceUnitTest()
        {
            _noteRepository = new Mock<IRepository<Note>>();
            _userRepository = new Mock<IUserRepository>();

            _noteService = new NoteService(_noteRepository.Object, _userRepository.Object);
        }

        [TestMethod]
        public void GetAllNotes_Should_Return_NotesDTO()
        {
            //Arrange
            List<Note> notes = new List<Note>()
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
                    UserId = 2,
                    User = new User
                    {
                        Id = 1,
                        Firstname = "Pale",
                        Lastname = "Joldasheva",
                        Username = "PaleJ"
                    }
                }
            };

            _noteRepository.Setup(x => x.GetAll()).Returns(notes);

            //Act
            List<NoteDto> resultNoteDto = _noteService.GetAllNotes();

            //Assert
            Assert.AreEqual(2, resultNoteDto.Count);
            Assert.AreEqual("Do the homework", resultNoteDto.First().Text);
            Assert.AreEqual("Drink water", resultNoteDto.Last().Text);
            Assert.AreEqual("Pale Joldasheva", resultNoteDto.Last().UserFullName);
        }

        [TestMethod]
        public void GetAll_Should_ReturnEmptyList()
        {
            //Arrange
            _noteRepository.Setup(x => x.GetAll()).Returns(new List<Note>());

            //Act
            List<NoteDto> resultNoteDto = _noteService.GetAllNotes();

            //Assert
            Assert.AreEqual(0, resultNoteDto.Count);
        }

        [TestMethod]
        public void GetById_Should_NoteNotFound()
        {
            //Arrange
            int id = 20;
            _noteRepository.Setup(x => x.GetById(id)).Returns(null as Note);

            //Act and assert
            var exception = Assert.ThrowsException<NoteNotFoundException>(() => _noteService.GetById(id));

            //Assert
            Assert.AreEqual($"Note with id {id} was not found!", exception.Message);
        }

        // Da gi dovrsam za Add i Update

    }
}
