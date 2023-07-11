using System;
using System.Collections.Generic;

namespace Construct.Model
{
    [Serializable]
    public sealed class Join
    {
        public int Id;
        public int[] NextJoinIds;
        public int LeftJoinId;
        public List<SingulaJoin> LeftPimples;
        public int RightJoinId;
        public List<SingulaJoin> RightPimples;
    }

    [Serializable]
    public struct SingulaJoin
    {
        public int SingulaId;
        public int PimpleId;
    }
}