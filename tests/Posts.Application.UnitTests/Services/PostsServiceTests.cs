using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Posts.Application.Core;
using Posts.Application.Exceptions;
using Posts.Application.Repositories;
using Posts.Application.Repositories.Models;
using Posts.Application.Services;
using Posts.Contract.Models;
using Posts.Contract.Models.Posts;
using Posts.Domain.Entities;

namespace Posts.Application.UnitTests.Services
{
    public class PostsServiceTests
    {
        protected readonly Mock<IPostsRepository> _postsRepoMock = new();
        protected readonly Mock<ITagsRepository> _tagsRepoMock = new();
        protected readonly Mock<IPostMediaRepository> _mediaRepoMock = new();
        protected readonly Mock<IUnitOfWork> _uowMock = new();
        protected readonly Mock<IS3Client> _s3ClientMock = new();
        protected readonly Mock<ICurrentUser> _currentUserMock = new();
        protected readonly Mock<ILogger<PostsService>> _loggerMock = new();

        protected readonly PostsService _service;

        public PostsServiceTests()
        {
            _service = new PostsService(
                _postsRepoMock.Object,
                _tagsRepoMock.Object,
                _mediaRepoMock.Object,
                _uowMock.Object,
                _s3ClientMock.Object,
                _currentUserMock.Object,
                _loggerMock.Object
            );
        }

        protected void SetupHappyPath(Guid? createdId = null)
        {
            _s3ClientMock.Setup(x => x.PersistFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("s3-key");
            _s3ClientMock.Setup(x => x.GetPublicUrl(It.IsAny<string>()))
                .Returns((string x) => $"http://store/{x}");
            _s3ClientMock.Setup(x => x.GetPresignedUrl(It.IsAny<string>(), It.IsAny<int>()))
                .Returns((string x, int? _exp) => $"http://store/{x}?t=token");

            _postsRepoMock.Setup(x => x.GetReadModelsByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(new List<PostReadModel>
                {
                new PostReadModel
                {
                    Id = createdId ?? Guid.NewGuid(),
                    Content = "Returned Content",
                    CreatorId = Guid.NewGuid(),
                    CreatorUsername = "test_user",
                    CreatorFirstName = "Test",
                    CreatorLastName = "User",
                    MediaKey = null,
                    Tags = Array.Empty<string>()
                }
                });

            _uowMock.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
        }

        #region Private
        [Fact]
        public async Task Create_Should_ExtractAndSaveTags_Correctly()
        {
            SetupHappyPath();

            var content = "Привет #world, это #тест и еще раз #WORLD";

            var dto = new CreatePostDto
            {
                Content = content,
                Media = []
            };

            await _service.Create(dto);

            _tagsRepoMock.Verify(x => x.UpsertTagsStatsAsync(
                It.Is<string[]>(tags =>
                    tags.Length == 2 &&
                    tags.Contains("world") &&
                    tags.Contains("тест")
                )
            ), Times.Once);
        }


        [Fact]
        public async Task Create_Should_Return_MappedDto()
        {
            // Arrange
            SetupHappyPath();
            var expectedId = Guid.NewGuid();
            var expectedContent = "Final Content";

            var dto = new CreatePostDto { Content = "Input" };

            _postsRepoMock.Setup(x => x.GetReadModelsByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(new List<PostReadModel>
                {
                new PostReadModel
                {
                    Id = expectedId,
                    Content = expectedContent,
                    CreatorId = Guid.NewGuid(),
                }
                });

            // Act
            var result = await _service.Create(dto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(expectedId);
            result.Content.Should().Be(expectedContent);
        }

        [Fact]
        public async Task Create_Should_Return_MappedDto_WithManyMedia()
        {
            // Arrange
            SetupHappyPath();
            var expectedId = Guid.NewGuid();
            var expectedContent = "Final Content";

            var dto = new CreatePostDto { Content = "Input" };
            var creatorId = Guid.NewGuid();

            _postsRepoMock.Setup(x => x.GetReadModelsByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(new List<PostReadModel>
                {
                new PostReadModel
                {
                    Id = expectedId,
                    Content = expectedContent,
                    CreatorId = creatorId,
                    MediaKey = "media1.jpg",
                    MediaId = Guid.NewGuid(),
                    MediaOrder = 0
                },
                new PostReadModel
                {
                    Id = expectedId,
                    Content = expectedContent,
                    CreatorId = creatorId,
                    MediaKey = "media2.jpg",
                    MediaId = Guid.NewGuid(),
                    MediaOrder = 1
                },
                new PostReadModel
                {
                    Id = expectedId,
                    Content = expectedContent,
                    CreatorId = creatorId,
                    MediaKey = "media3.jpg",
                    MediaId = Guid.NewGuid(),
                    MediaOrder = 2
                },
                new PostReadModel
                {
                    Id = expectedId,
                    Content = expectedContent,
                    CreatorId = creatorId,
                    MediaKey = "media4.jpg",
                    MediaId = Guid.NewGuid(),
                    MediaOrder = 3
                }
                });

            // Act
            var result = await _service.Create(dto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(expectedId);
            result.Content.Should().Be(expectedContent);
        }

        #endregion

        #region Create
        [Fact]
        public async Task Create_Should_ThrowValidationException_When_ReplyAndRepost_AreSet()
        {
            var dto = new CreatePostDto
            {
                ReplyForId = Guid.NewGuid(),
                RepostId = Guid.NewGuid(),
                Content = "Fail"
            };

            // Act
            Func<Task> act = async () => await _service.Create(dto);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*same time*");
        }

        [Fact]
        public async Task Create_Should_ThrowEntityNotFound_When_RepostId_DoesNotExist()
        {
            var repostId = Guid.NewGuid();
            var dto = new CreatePostDto { RepostId = repostId, Content = "Test" };

            _postsRepoMock.Setup(x => x.GetByIdAsync(repostId))
                .ReturnsAsync((Post?)null);

            // Act
            Func<Task> act = async () => await _service.Create(dto);

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task Create_Should_SavePost_WithTags_And_Media()
        {
            // Arrange
            SetupHappyPath();
            var content = "Hello #world and #csharp";
            var mediaDto = new FileDto { Key = "img.jpg" };

            var dto = new CreatePostDto
            {
                Content = content,
                Media = new[] { mediaDto }
            };

            // Act
            await _service.Create(dto);

            // Assert
            _postsRepoMock.Verify(x => x.AddAsync(It.Is<Post>(p =>
                p.Content == content &&
                p.Tags.Contains("world") &&
                p.Tags.Length == 2
            )), Times.Once);

            _tagsRepoMock.Verify(x => x.UpsertTagsStatsAsync(It.Is<string[]>(tags =>
                tags.Length == 2 && tags.Contains("world") && tags.Contains("csharp")
            )), Times.Once);

            _mediaRepoMock.Verify(x => x.AddManyAsync(It.Is<IEnumerable<PostMedia>>(media =>
                media.Count() == 1 &&
                media.First().Key == "s3-key" &&
                media.First().SortOrder == 0
            )), Times.Once);

            _uowMock.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Create_Should_IncrementCounters_And_SetDepth_When_Replying()
        {
            SetupHappyPath();
            var parentId = Guid.NewGuid();
            var parentPost = new Post { Id = parentId, Depth = 5 };

            var dto = new CreatePostDto { ReplyForId = parentId, Content = "Reply" };

            _postsRepoMock.Setup(x => x.GetByIdAsync(parentId)).ReturnsAsync(parentPost);

            // Act
            await _service.Create(dto);

            // Assert
            _postsRepoMock.Verify(x => x.AddAsync(It.Is<Post>(p =>
                p.ReplyForId == parentId &&
                p.Depth == 6
            )), Times.Once);

            _postsRepoMock.Verify(x => x.IncrementRepliesCountAsync(parentId), Times.Once);
        }

        [Fact]
        public async Task Create_Should_IncrementRepostsCount_When_Reposting()
        {
            // Arrange
            SetupHappyPath();
            var originId = Guid.NewGuid();
            var dto = new CreatePostDto { RepostId = originId, Content = "Repost" };

            _postsRepoMock.Setup(x => x.GetByIdAsync(originId)).ReturnsAsync(new Post());

            // Act
            await _service.Create(dto);

            // Assert
            _postsRepoMock.Verify(x => x.IncrementRepostsCountAsync(originId), Times.Once);
        }
        #endregion
        #region Update
        [Fact]
        public async Task Update_Should_Throw_When_PostNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _postsRepoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Post?)null);

            var dto = new UpdatePostDto { Id = id };

            // Act
            Func<Task> act = async () => await _service.Update(dto);

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task Update_Should_CalculateDiff_And_CleanupNewFilesOnly_OnFailure()
        {
            var postId = Guid.NewGuid();
            var existingPost = new Post { Id = postId, Content = "Old #tag", Tags = ["tag"] };

            var existingMedia = new List<PostMedia>
        {
            new PostMedia { Id = Guid.NewGuid(), Key = "old.jpg", SortOrder = 0 }
        };

            var updateDto = new UpdatePostDto
            {
                Id = postId,
                Content = "New #tag #wow",
                Media = [
                    new FileDto { Key = "old.jpg" },
                new FileDto { Key = "temp_new.jpg" }
                ]
            };

            _postsRepoMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(existingPost);
            _mediaRepoMock.Setup(x => x.GetByPostIdAsync(postId)).ReturnsAsync(existingMedia);

            _s3ClientMock.Setup(x => x.PersistFileAsync(It.Is<string>(f => f == "old.jpg"), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("old.jpg");
            _s3ClientMock.Setup(x => x.PersistFileAsync(It.Is<string>(f => f == "temp_new.jpg"), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync("permanent/new.jpg");

            _uowMock.Setup(x => x.CommitAsync()).ThrowsAsync(new Exception("DB Crash"));

            _postsRepoMock.Setup(x => x.GetReadModelsByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(new List<PostReadModel>
                {
                new PostReadModel
                {
                    Id = updateDto.Id,
                    CreatorId = Guid.NewGuid(),
                    Content = updateDto.Content,
                    CreatorUsername = "testuser",
                    MediaKey = null
                }
                });

            Func<Task> act = async () => await _service.Update(updateDto);

            await act.Should().ThrowAsync<Exception>();

            _tagsRepoMock.Verify(x => x.UpsertTagsStatsAsync(It.Is<string[]>(t => t.Contains("wow") && t.Length == 1)), Times.Once);
            _mediaRepoMock.Verify(x => x.AddManyAsync(It.Is<IEnumerable<PostMedia>>(m => m.Count() == 1)), Times.Once);

            _s3ClientMock.Verify(x => x.DeleteFileAsync(It.Is<string>(x => x == "permanent/new.jpg")), Times.Once);
            _s3ClientMock.Verify(x => x.DeleteFileAsync(It.Is<string>(x => x == "old.jpg")), Times.Never);
        }

        [Fact]
        public async Task Update_Should_Calculate_Tags_Diff_Correctly()
        {

            var postId = Guid.NewGuid();
            var oldTags = new[] { "old", "keep" };
            var newContent = "Some text #keep #new";

            var existingPost = new Post { Id = postId, Tags = oldTags, Content = "Old content" };
            _postsRepoMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(existingPost);

            _mediaRepoMock.Setup(x => x.GetByPostIdAsync(postId)).ReturnsAsync(new List<PostMedia>());
            _s3ClientMock.Setup(x => x.PersistFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync("key");

            _uowMock.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
            _postsRepoMock.Setup(x => x.GetReadModelsByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(new List<PostReadModel> { new PostReadModel() });

            var dto = new UpdatePostDto { Id = postId, Content = newContent, Media = Array.Empty<FileDto>() };

            // Act
            await _service.Update(dto);

            // Assert
            _postsRepoMock.Verify(x => x.UpdateAsync(It.Is<Post>(p =>
                p.Tags.Length == 2 && p.Tags.Contains("keep") && p.Tags.Contains("new")
            )), Times.Once);

            _tagsRepoMock.Verify(x => x.UpsertTagsStatsAsync(It.Is<string[]>(tags =>
                tags.Length == 1 && tags.First() == "new"
            )), Times.Once);

            _tagsRepoMock.Verify(x => x.DecrementTagsUsageAsync(It.Is<string[]>(tags =>
                tags.Length == 1 && tags.First() == "old"
            )), Times.Once);
        }

        [Fact]
        public async Task Update_Should_Handle_Media_Add_Update_Delete_Correctly()
        {
            // Arrange
            var postId = Guid.NewGuid();

            var existingMedia = new List<PostMedia>
        {
            new PostMedia { Id = Guid.NewGuid(), Key = "img_delete.jpg", SortOrder = 0 },
            new PostMedia { Id = Guid.NewGuid(), Key = "img_swap.jpg", SortOrder = 1 }
        };

            var dtoMedia = new[]
            {
            new FileDto { Key = "img_swap.jpg" },
            new FileDto { Key = "img_new.jpg" }
        };

            _postsRepoMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(new Post { Id = postId, Tags = Array.Empty<string>() });
            _mediaRepoMock.Setup(x => x.GetByPostIdAsync(postId)).ReturnsAsync(existingMedia);

            _s3ClientMock.Setup(x => x.PersistFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync((string f, string folder, bool? _p) => f);

            _uowMock.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            _postsRepoMock.Setup(x => x.GetReadModelsByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(new List<PostReadModel> { new PostReadModel() });

            var dto = new UpdatePostDto
            {
                Id = postId,
                Content = "content",
                Media = dtoMedia
            };

            // Act
            await _service.Update(dto);

            // Assert

            _mediaRepoMock.Verify(x => x.AddManyAsync(It.Is<IEnumerable<PostMedia>>(list =>
                list.Count() == 1 &&
                list.First().Key == "img_new.jpg" &&
                list.First().SortOrder == 1
            )), Times.Once);

            _mediaRepoMock.Verify(x => x.UpdateAsync(It.Is<PostMedia>(m =>
                m.Key == "img_swap.jpg" &&
                m.SortOrder == 0
            )), Times.Once);

            var deletedId = existingMedia.First(m => m.Key == "img_delete.jpg").Id;

            _mediaRepoMock.Verify(x => x.DeleteManyAsync(It.Is<IEnumerable<Guid>>(ids =>
                ids.Contains(deletedId) && ids.Count() == 1
            )), Times.Once);
        }
        #endregion
        #region Delete
        [Fact]
        public async Task Delete_Should_Throw_When_PostNotFound()
        {
            // Arrange
            var postId = Guid.NewGuid();
            _postsRepoMock.Setup(x => x.GetByIdAsync(postId))
                .ReturnsAsync((Post?)null); // Явно возвращаем null

            // Act
            Func<Task> act = async () => await _service.Delete(postId);

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>();

            _uowMock.Verify(x => x.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task Delete_Should_DecrementCounters_And_RemovePost()
        {
            // Arrange
            SetupHappyPath();
            var postId = Guid.NewGuid();
            var replyId = Guid.NewGuid();
            var repostId = Guid.NewGuid();
            var tags = new[] { "tag1", "tag2" };

            var postToDelete = new Post
            {
                Id = postId,
                ReplyForId = replyId,
                RepostId = repostId,
                Tags = tags
            };

            _postsRepoMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(postToDelete);

            // Act
            await _service.Delete(postId);

            // Assert
            _postsRepoMock.Verify(x => x.DecrementRepliesCountAsync(replyId), Times.Once);
            _postsRepoMock.Verify(x => x.DecrementRepostsCountAsync(repostId), Times.Once);
            _tagsRepoMock.Verify(x => x.DecrementTagsUsageAsync(tags), Times.Once);
            _postsRepoMock.Verify(x => x.DeleteAsync(postId), Times.Once);
            _uowMock.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Delete_Should_Rollback_On_DatabaseError()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var post = new Post { Id = postId, Tags = new[] { "tag1" } };

            _postsRepoMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(post);

            _postsRepoMock.Setup(x => x.DeleteAsync(postId))
                .ThrowsAsync(new Exception("DB Connection Failed"));

            // Act
            Func<Task> act = async () => await _service.Delete(postId);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("DB Connection Failed");

            _uowMock.Verify(x => x.RollbackAsync(), Times.Once);
            _uowMock.Verify(x => x.CommitAsync(), Times.Never);
        }
        #endregion
        #region Read
        [Fact]
        public async Task GetById_Should_ReturnDto_When_PostExists()
        {
            var postId = Guid.NewGuid();
            SetupHappyPath(postId);
            _postsRepoMock.Setup(x => x.GetByIdAsync(postId))
                .ReturnsAsync(new Post { Id = postId });

            // Act
            var result = await _service.GetById(postId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(postId);
        }

        [Fact]
        public async Task GetById_Should_Throw_When_PostNotFound()
        {
            // Arrange
            _postsRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Post?)null);

            // Act
            Func<Task> act = async () => await _service.GetById(Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task GetUserPosts_Should_ReturnPagedData_And_CallRepoWith_IsReply_False()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var pagination = new PaginationRequestDto { From = 0, Limit = 10 };
            var postId = Guid.NewGuid();
            SetupHappyPath(postId);

            _postsRepoMock.Setup(x => x.GetPostsByCreatorAsync(userId, pagination, false))
                .ReturnsAsync(new List<Post> { new Post { Id = postId } });

            _postsRepoMock.Setup(x => x.GetPostsByCreatorCountAsync(userId, false))
                .ReturnsAsync(42);

            // Act
            var result = await _service.GetUserPosts(userId, pagination);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Id.Should().Be(postId);
            result.Total.Should().Be(42);

            _postsRepoMock.Verify(x => x.GetPostsByCreatorAsync(userId, pagination, false), Times.Once);
        }

        [Fact]
        public async Task GetUserReplies_Should_CallRepoWith_IsReply_True()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var pagination = new PaginationRequestDto { From = 0, Limit = 10 };
            var postId = Guid.NewGuid();
            SetupHappyPath(postId);

            _postsRepoMock.Setup(x => x.GetPostsByCreatorAsync(userId, pagination, true))
                .ReturnsAsync(new List<Post> { new Post { Id = postId } });

            _postsRepoMock.Setup(x => x.GetPostsByCreatorCountAsync(userId, true))
                .ReturnsAsync(42);

            // Act
            var result = await _service.GetUserReplies(userId, pagination);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Id.Should().Be(postId);
            result.Total.Should().Be(42);

            _postsRepoMock.Verify(x => x.GetPostsByCreatorAsync(userId, pagination, true), Times.Once);
        }

        [Fact]
        public async Task GetPostReplies_Should_ReturnPagedData()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var replyId = Guid.NewGuid();
            var pagination = new PaginationRequestDto();
            SetupHappyPath(replyId);

            _postsRepoMock.Setup(x => x.GetPostRepliesAsync(postId, pagination))
                .ReturnsAsync(new List<Post> { new Post { Id = replyId } });

            _postsRepoMock.Setup(x => x.GetPostRepliesCountAsync(postId))
                .ReturnsAsync(5);

            // Act
            var result = await _service.GetPostReplies(postId, pagination);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Id.Should().Be(replyId);
            result.Total.Should().Be(5);
        }
        #endregion
    }
}
