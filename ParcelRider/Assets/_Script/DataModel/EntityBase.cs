using System;

namespace DataModel
{
    public class EntityBase
    {
        public static T Instance<T>() where T : EntityBase, new()
        {
            var t = new T();
            return t;
        }

        public int Version { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
        public long DeletedAt { get; set; }

        public EntityBase()
        {
            Version = 0;
            CreatedAt = GetEpochTime();
            UpdatedAt = CreatedAt;
            DeletedAt = 0;
        }

        public long GetEpochTime()
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(DateTime.UtcNow - epoch).TotalSeconds;
        }

        public void UpdateFileTimeStamp()
        {
            UpdatedAt = GetEpochTime();
            Version++;
        }

        public void DeleteFileTimeStamp()
        {
            DeletedAt = GetEpochTime();
        }
    }
    public class EntityBase<TId> : EntityBase
    {
        public TId Id { get; set; }
    }
}