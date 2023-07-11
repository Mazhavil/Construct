using System;

namespace Construct.Services
{
    [Serializable]
    public sealed class JoinDto
    {
        public int join_id;
        public int[] next_join_ids;
        public int left_join_id;
        public int right_join_id;
    }
}