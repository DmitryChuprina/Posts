namespace Posts.Application.Exceptions
{
    public class ValueIsTakenException : Exception
    {
        public Type EntityType { get; }
        public object? EntityKey { get; }
        public string PropertyName { get; }

        public ValueIsTakenException(Type entityType, string propertyName, object? entityKey = null)
            : base(CreateMessage(entityType, propertyName, entityKey))
        {
            EntityType = entityType;
            EntityKey = entityKey;
            PropertyName = propertyName;
        }

        private static string CreateMessage(Type type, string propertyName, object? key)
        {
            var name = type.Name;
            return $"{propertyName} is already taken by another {name}.";
        }
    }
}
