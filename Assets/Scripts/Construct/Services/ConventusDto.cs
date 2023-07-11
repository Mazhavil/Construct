using System;

namespace Construct.Services
{
    [Serializable]
    public sealed class ConventusDto
    {
        public int conventus_id;
        public string conventus_name;
        public SingulaDto[] singulas;
        public JoinDto[] joins;
    }
}