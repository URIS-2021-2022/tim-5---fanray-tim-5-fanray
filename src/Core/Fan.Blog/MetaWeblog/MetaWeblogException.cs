using System;
using System.Runtime.Serialization;

namespace Fan.Blog.MetaWeblog
{
    public enum EMetaWeblogCode
    {
        UnknownCause = 1000,
        InvalidRequest,
        UnknownMethod,
        AuthenticationFailed,
        GetUsersBlogs,
        GetPost,
        GetRecentPosts,
        NewPost,
        EditPost,
        DeletePost,
        GetCategories,
        CreateCategory,
        GetKeywords,
        NewMediaObject,
    }

    [Serializable]
    public class MetaWeblogException : Exception
    {
        public MetaWeblogException(EMetaWeblogCode code, string message)
            : base(message)
        {
            Code = code;
        }

        protected MetaWeblogException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
             base.GetObjectData(info, context);
        }

        public EMetaWeblogCode Code { get; private set; }
    }
}
