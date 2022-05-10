using Moq;
using NordTaskApi.Common.Exceptions;
using NordTaskApi.Common.Models;
using NordTaskApi.Common.Repositories;
using NordTaskApi.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NordTaskApi.Tests
{
    public class NoteServiceTests
    {
        [Fact]
        public async Task NewNoteShouldBeAddedToDBWithoutID()
        {
            var repoMock = new Mock<INotesRepository>();
            repoMock.Setup(r => r.CreateNote(It.IsAny<Note>())).Returns<Note>(note => Task.FromResult(note));

            var notesService = new NotesService(repoMock.Object);
            var result = await notesService.CreateNote(new Note(), string.Empty);

            Assert.Equal(Guid.Empty, result.Id);
        }

        [Fact]
        public async Task NewNoteShouldBeOwned()
        {
            var repoMock = new Mock<INotesRepository>();
            repoMock.Setup(r => r.CreateNote(It.IsAny<Note>())).Returns<Note>(note => Task.FromResult(note));

            var userId = "userId";
            var notesService = new NotesService(repoMock.Object);
            var result = await notesService.CreateNote(new Note(), userId);

            Assert.Equal(userId, result.OwnedBy);
        }

        [Fact]
        public async Task NewNoteShouldHaveCreationTimeInUTC()
        {
            var repoMock = new Mock<INotesRepository>();
            repoMock.Setup(r => r.CreateNote(It.IsAny<Note>())).Returns<Note>(note => Task.FromResult(note));

            var notesService = new NotesService(repoMock.Object);
            var result = await notesService.CreateNote(new Note(), string.Empty);

            Assert.NotEqual(DateTime.MinValue, result.CreatedAt);
            Assert.Equal(DateTimeKind.Utc, result.CreatedAt.Kind);
        }

        [Theory]
        [InlineData("password")]
        [InlineData("")]
        [InlineData(null)]
        public async Task NewNoteShouldBePasswordSecured(string password)
        {
            var repoMock = new Mock<INotesRepository>();
            repoMock.Setup(r => r.CreateNote(It.IsAny<Note>())).Returns<Note>(note => Task.FromResult(note));

            var notesService = new NotesService(repoMock.Object);
            var newNote = new Note
            {
                UserPassword = password
            };
            var result = await notesService.CreateNote(newNote, string.Empty);

            Assert.Equal(!string.IsNullOrEmpty(password), result.IsPasswordProtected);
        }

        [Theory]
        [InlineData("user1", "user2", "user3")]
        [InlineData("user1")]
        [InlineData(null)]
        public async Task NewNoteShouldhaveShares(params string[]? sharedEmails)
        {
            var repoMock = new Mock<INotesRepository>();
            repoMock.Setup(r => r.CreateNote(It.IsAny<Note>())).Returns<Note>(note => Task.FromResult(note));

            var notesService = new NotesService(repoMock.Object);
            var sharesCount = sharedEmails?.Length;
            var newNote = new Note
            {
                SharedWithEmails = sharedEmails?.ToList()
            };
            var result = await notesService.CreateNote(newNote, string.Empty);

            Assert.Equal(sharesCount, result.SharedWith?.Count);
        }

        [Fact]
        public async Task GetNotesShouldNotReturnContentIfPasswordSecured()
        {
            var repoMock = new Mock<INotesRepository>();
            var notes = (new List<Note>
            {
                new Note(){Password = string.Empty, Content = "Content"},
                new Note(){Password = null, Content = "Content"},
                new Note(){Password = "xxxxx", Content = "Content"},
                new Note(){Password = "\r\n", Content = "Content"}
            }).AsEnumerable();
            repoMock.Setup(r => r.GetNotes(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(notes));

            var notesService = new NotesService(repoMock.Object);
            var result = await notesService.GetNotes(string.Empty, new CancellationToken());

            Assert.DoesNotContain(result, note => !string.IsNullOrEmpty(note.Password) && !string.IsNullOrEmpty(note.Content));
        }

        [Fact]
        public async Task GetNoteShouldReturnSharedWithAsEmailList()
        {
            var repoMock = new Mock<INotesRepository>();
            var notes = (new List<Note>
            {
                new Note(){SharedWith = new List<NoteShare>
                    {
                        new NoteShare { UserEmail = "user1"},
                        new NoteShare { UserEmail = "user3"},
                        new NoteShare { UserEmail = "user2"}
                    }
                }
            }).AsEnumerable();
            repoMock.Setup(r => r.GetNotes(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(notes));

            var notesService = new NotesService(repoMock.Object);
            var result = (await notesService.GetNotes(string.Empty, new CancellationToken())).First().SharedWithEmails;

            Assert.NotNull(result);
            Assert.Equal(3, result!.Count);
            Assert.Equal(notes!.First()!.SharedWith!.Select(s => s.UserEmail!).ToList(), result);
        }

        [Fact]
        public async Task UpdateNoteShouldHaveSharedEmails()
        {
            var noteOwner = "personA";
            var repoMock = new Mock<INotesRepository>();
            var updatedNote = new Note
            {
                SharedWithEmails = new List<string> { "user1", "user2", "user3" }
            };
            repoMock.Setup(r => r.GetNotes(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<Note>() { new Note { OwnedBy = noteOwner, Content = "" } }.AsEnumerable()));
            repoMock.Setup(r => r.GetNoteShares(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Enumerable.Empty<NoteShare>()));
            repoMock.Setup(r => r.UpdateNote(It.IsAny<Note>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var notesService = new NotesService(repoMock.Object);
            await notesService.UpdateNote(updatedNote, noteOwner);

            Assert.Equal(3, updatedNote!.SharedWith!.Count);
            Assert.Equal(updatedNote.SharedWithEmails, updatedNote.SharedWith.Select(sw => sw.UserEmail));
        }

        [Theory()]
        [InlineData("", "")]
        [InlineData(null, null)]
        [InlineData("cGFzc3dvcmQ=", "password")]
        public async Task UpdateNoteShouldBePasswordProtected(string passwordBase64, string passwordDecoded)
        {
            var repoMock = new Mock<INotesRepository>();
            var noteOwner = "personA";
            var updatedNote = new Note
            {
                OwnedBy = noteOwner,
                UserPassword = passwordBase64
            };
            repoMock.Setup(r => r.GetNotes(It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .Returns(Task.FromResult(new List<Note>()
               {
                   new Note { OwnedBy = noteOwner, Content = "", Password = passwordDecoded } 
               }.AsEnumerable()));
            repoMock.Setup(r => r.GetNoteShares(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Enumerable.Empty<NoteShare>()));
            repoMock.Setup(r => r.UpdateNote(It.IsAny<Note>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var notesService = new NotesService(repoMock.Object);
            await notesService.UpdateNote(updatedNote, noteOwner);

            Assert.Equal(!string.IsNullOrEmpty(passwordBase64), updatedNote.IsPasswordProtected);
        }

        [Fact]
        public async Task CannotRemoveNotOwnedNote()
        {
            var owner = "personA";
            var requestor = "personB";
            var noteId = Guid.NewGuid();
            var note = new Note
            {
                OwnedBy = owner,
                Id = noteId
            };
            var repoMock = new Mock<INotesRepository>();
            repoMock.Setup(r => r.GetNotes(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<Note>() { note }.AsEnumerable()));

            var notesService = new NotesService(repoMock.Object);

            await Assert.ThrowsAsync<UnauthorizedException>(() => notesService.DeleteNote(noteId, requestor));
        }

        [Fact]
        public async Task CannotRemoveNotExistingNote()
        {
            var repoMock = new Mock<INotesRepository>();
            repoMock.Setup(r => r.GetNotes(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Enumerable.Empty<Note>()));

            var notesService = new NotesService(repoMock.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => notesService.DeleteNote(Guid.NewGuid(), "personA"));
        }

        [Fact]
        public async Task CannotDeleteNotExisitingNoteShare()
        {
            var noteId = Guid.NewGuid();
            var shareOwner = "personA";
            var repoMock = new Mock<INotesRepository>();
            repoMock.Setup(r => r.GetNoteShares(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<NoteShare>() { new NoteShare { NoteId = noteId, UserEmail = shareOwner } }.AsEnumerable()));

            var notesService = new NotesService(repoMock.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => notesService.DeleteNoteShare(Guid.NewGuid(), shareOwner));
        }

        [Fact]
        public async Task CannotDeleteNotOwnedNoteShare()
        {
            var repoMock = new Mock<INotesRepository>();
            repoMock.Setup(r => r.GetNoteShares(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Enumerable.Empty<NoteShare>()));

            var notesService = new NotesService(repoMock.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => notesService.DeleteNoteShare(Guid.NewGuid(), "personA"));
        }
    }
}