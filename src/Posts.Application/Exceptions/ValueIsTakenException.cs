namespace Posts.Application.Exceptions
{
    public class ValueIsTakenException : Exception
    {
        public Type EntityType { get; }
        public object? EntityKey { get; }
        public string ValueName { get; set; }

        public ValueIsTakenException(Type entityType, string valueName, object? entityKey = null)
            : base(CreateMessage(entityType, valueName, entityKey))
        {
            EntityType = entityType;
            EntityKey = entityKey;
            ValueName = valueName;
        }

        private static string CreateMessage(Type type, string valueName, object? key)
        {
            var name = type.Name;
            return $"{valueName} is already taken by another {name}.";
        }
    }
}
