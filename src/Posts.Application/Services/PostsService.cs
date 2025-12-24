using Posts.Application.Core;
using Posts.Application.Exceptions;
using Posts.Application.Extensions;
using Posts.Application.Repositories;
using Posts.Application.Repositories.Models;
using Posts.Contract.Models;
using Posts.Contract.Models.Posts;
using Posts.Domain.Entities;
using Posts.Domain.Utils;
using System.Text.RegularExpressions;

namespace Posts.Application.Services
{
    public class PostsService
    {
        private static readonly Regex _hashtagRegex = new Regex(@"#(\w+)", RegexOptions.Compiled);
        private static readonly string _uploadMediaFolder = "posts/media/";

        private readonly IPostsRepository _postsRepository;
        private readonly ITagsRepository _tagsRepository;
        private readonly IPostMediaRepository _postMediaRepository;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IS3Client _s3Client;

        private readonly ICurrentUser _currentUser;

        public PostsService(
            IPostsRepository postsRepository,
            ITagsRepository tagsRepository,
            IPostMediaRepository postMediaRepository,
            IUnitOfWork unitOfWork,
            IS3Client s3Client,
            ICurrentUser currentUser
        )
        {
            _postsRepository = postsRepository;
            _tagsRepository = tagsRepository;
            _postMediaRepository = postMediaRepository;

            _unitOfWork = unitOfWork;
            _s3Client = s3Client;

            _currentUser = currentUser;
        }

        public async Task<PostDto> GetById(Guid id)
        {
            var post = await _postsRepository.GetByIdAsync(id);
            if (post is null)
            {
                throw new EntityNotFoundException(typeof(Post), id);
            }

            return await GetPostDtoByPostId(post.Id);
        }

        public async Task<PaginationResponseDto<PostDto>> GetUserPosts(Guid userId, PaginationRequestDto dto)
        {
            var userPosts = await _postsRepository.GetPostsByCreatorAsync(userId, dto, false);
            var userPostsCount = await _postsRepository.GetPostsByCreatorCountAsync(userId, false);
            var dtos = await GetPostsDtosByPostIds(userPosts.Select(x => x.Id));
            return new PaginationResponseDto<PostDto>
            {
                Items = dtos,
                Total = userPostsCount
            };
        }

        public async Task<PaginationResponseDto<PostDto>> GetUserReplies(Guid userId, PaginationRequestDto dto)
        {
            var userPosts = await _postsRepository.GetPostsByCreatorAsync(userId, dto, true);
            var userPostsCount = await _postsRepository.GetPostsByCreatorCountAsync(userId, true);
            var dtos = await GetPostsDtosByPostIds(userPosts.Select(x => x.Id));
            return new PaginationResponseDto<PostDto>
            {
                Items = dtos,
                Total = userPostsCount
            };
        }

        public async Task<PaginationResponseDto<PostDto>> GetPostReplies(Guid postId, PaginationRequestDto dto)
        {
            var postReplies = await _postsRepository.GetPostRepliesAsync(postId, dto);
            var postRepliesCount = await _postsRepository.GetPostRepliesCountAsync(postId);
            var postRepliesIds = postReplies.Select(x => x.Id);
            var dtos = await GetPostsDtosByPostIds(postRepliesIds);
            return new PaginationResponseDto<PostDto>
            {
                Items = dtos,
                Total = postRepliesCount
            };
        }

        public async Task<PostDto> Create(CreatePostDto dto)
        {
            Post? repostedPost = null;
            if (dto.RepostId is not null)
            {
                repostedPost = await _postsRepository.GetByIdAsync(dto.RepostId.Value);
                if (repostedPost is null)
                {
                    throw new EntityNotFoundException(typeof(Post), dto.RepostId);
                }
            }

            Post? repliyedPost = null;
            if (dto.ReplyForId is not null)
            {
                repliyedPost = await _postsRepository.GetByIdAsync(dto.ReplyForId.Value);
                if (repliyedPost is null)
                {
                    throw new EntityNotFoundException(typeof(Post), dto.ReplyForId);
                }
            }

            var uploads = await Task.WhenAll(
                dto
                  .Media
                  .Select((m) => _s3Client.PersistFileDtoAsync(m, _uploadMediaFolder))
            );


            var tags = CalculateTagsFromContent(dto.Content);

            var post = new Post
            {
                Content = dto.Content,
                Tags = tags,
                ReplyForId = dto.ReplyForId,
                RepostId = dto.RepostId,
                Depth = repliyedPost is null ? 0 : repliyedPost.Depth + 1,
            };

            var media = uploads
                .Select((u, idx) => new PostMedia
                {
                    PostId = post.Id,
                    Key = u!,
                    SortOrder = idx
                })
                .ToArray();

            try
            {

                await _unitOfWork.BeginTransactionAsync();

                await _postsRepository.AddAsync(post);
                await _tagsRepository.UpsertTagsStatsAsync(tags);
                await _postMediaRepository.AddManyAsync(media);

                if (dto.ReplyForId is not null)
                {
                    await _postsRepository.IncrementRepliesCountAsync(dto.ReplyForId.Value);
                }

                if (dto.RepostId is not null)
                {
                    await _postsRepository.IncrementRepostsCountAsync(dto.RepostId.Value);
                }

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return await GetPostDtoByPostId(post.Id);
        }

        public async Task<PostDto> Update(UpdatePostDto dto)
        {
            var currentPost = await _postsRepository.GetByIdAsync(dto.Id);
            if (currentPost is null)
            {
                throw new EntityNotFoundException(typeof(Post), dto.Id);
            }

            var currentMedia = await _postMediaRepository.GetByPostIdAsync(dto.Id);
            var uploads = await Task.WhenAll(
                dto.Media
                    .Select((m) => _s3Client.PersistFileDtoAsync(m, _uploadMediaFolder))
            );

            var tags = CalculateTagsFromContent(dto.Content);
            var oldTags = currentPost.Tags;

            currentPost.Content = dto.Content;
            currentPost.Tags = tags;

            var tagsToAdd = tags.Except(oldTags).ToArray();
            var tagsToRemove = oldTags.Except(tags).ToArray();

            var mediaToAdd = new List<PostMedia>();
            var mediaToUpdate = new List<PostMedia>();
            var mediaToRemove = currentMedia.Where(m => !uploads.Contains(m.Key));

            for (var i = 0; i < uploads.Length; i++)
            {
                var order = i;
                var uploadKey = uploads[i]!;
                var mediaByKey = currentMedia.FirstOrDefault(m => m.Key == uploadKey);

                if (mediaByKey is null)
                {
                    var mediaEntity = new PostMedia
                    {
                        PostId = currentPost.Id,
                        Key = uploadKey,
                        SortOrder = order
                    };
                    mediaToAdd.Add(mediaEntity);
                    continue;
                }
                if (mediaByKey.SortOrder != order)
                {
                    mediaByKey.SortOrder = order;
                    mediaToUpdate.Add(mediaByKey);
                }
            }


            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _postsRepository.UpdateAsync(currentPost);

                if (tagsToAdd.Any())
                {
                    await _tagsRepository.UpsertTagsStatsAsync(tagsToAdd);
                }

                if (tagsToRemove.Any())
                {
                    await _tagsRepository.DecrementTagsUsageAsync(tagsToRemove);
                }

                if (mediaToAdd.Any())
                {
                    await _postMediaRepository.AddManyAsync(mediaToAdd);
                }

                foreach(var update in mediaToUpdate)
                {
                    // TODO: implement bulk update
                    await _postMediaRepository.UpdateAsync(update);
                }

                if (mediaToRemove.Any())
                {
                    var mediaToRemoveIds = mediaToRemove.Select(m => m.Id);
                    await _postMediaRepository.DeleteManyAsync(mediaToRemoveIds);
                }
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return await GetPostDtoByPostId(currentPost.Id);
        }

        public async Task Delete(Guid postId)
        {
            var post = await _postsRepository.GetByIdAsync(postId);
            if (post is null)
            {
                throw new EntityNotFoundException(typeof(Post), postId);
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                if(post.ReplyForId is not null)
                {
                    await _postsRepository.DecrementRepliesCountAsync(post.ReplyForId.Value);
                }

                if(post.RepostId is not null)
                {
                    await _postsRepository.DecrementRepostsCountAsync(post.RepostId.Value);
                }

                if(post.Tags.Any())
                {
                    await _tagsRepository.DecrementTagsUsageAsync(post.Tags);
                }

                await _postsRepository.DeleteAsync(postId);

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> IsAllowToRepost(Guid postId)
        {
            return true;
        }

        public async Task<bool> IsAllowToReply(Guid postId)
        {
            return true;
        }

        private async Task<IEnumerable<PostDto>> GetPostsDtosByPostIds(IEnumerable<Guid> ids)
        {
            var models = await _postsRepository.GetReadModelsByIdsAsync(ids);
            var repostIds = models
                .Where(x => x.RepostId is not null)
                .Select(x => x.RepostId!.Value);
            var reposts = await _postsRepository.GetReadModelsByIdsAsync(repostIds);

            return MapToDtos(models, reposts);
        }

        private async Task<PostDto> GetPostDtoByPostId(Guid id)
        {
            var dtos = await GetPostsDtosByPostIds([id]);
            return dtos.First();
        }

        private List<PostDto> MapToDtos(IEnumerable<PostReadModel> rows, IEnumerable<PostReadModel>? reposts = null)
        {
            var repostsDtos = reposts is null || !reposts.Any() ?
                new Dictionary<Guid, PostDto>() :
                MapToDtos(reposts).
                    ToDictionary(x => x.Id, x => x);

            return rows
                .GroupBy(r => r.Id)
                .Select(g =>
                {
                    var post = g.First();

                    return new PostDto
                    {
                        Id = post.Id,
                        Author = new PostAuthorDto
                        {
                            Id = post.CreatorId,
                            Username = post.CreatorUsername,
                            FirstName = post.CreatorFirstName,
                            LastName = post.CreatorLastName,
                            ProfileImage = _s3Client.GetPublicFileDto(post.CreatorProfileImageKey)
                        },
                        Content = post.Content,
                        LikesCount = post.LikesCount,
                        RepostsCount = post.RepostsCount,
                        ViewsCount = post.ViewsCount,
                        Depth = post.Depth,
                        Media = g
                            .Where(r => !string.IsNullOrEmpty(r.MediaKey))
                            .OrderBy(r => r.MediaOrder)
                            .Select(r => _s3Client.GetPresignedFileDto(r.MediaKey)!)
                            .ToArray(),
                        Repost = post.RepostId is not null ?
                            repostsDtos.GetValueOrDefault(post.RepostId.Value) :
                            null,

                    };
                })
                .ToList();
        }

        private string[] CalculateTagsFromContent(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return [];
            }

            return _hashtagRegex.Matches(content)
                .Select(match => match.Groups[1].Value)
                .Select(Formatting.Tag)
                .Distinct()
                .ToArray();
        }
    }
}
