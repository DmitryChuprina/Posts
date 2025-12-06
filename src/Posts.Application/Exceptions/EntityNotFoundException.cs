namespace Posts.Application.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public Type EntityType { get; }
        public object? Key { get; }

        public EntityNotFoundException(Type entityType, object? key = null)
            : base(CreateMessage(entityType, key))
        {
            EntityType = entityType;
            Key = key;
        }

        private static string CreateMessage(Type type, object? key)
        {
            var name = type.Name;

            return key is null
                ? $"{name} was not found."
                : $"{name} with key '{key}' was not found.";
        }
    }
}
