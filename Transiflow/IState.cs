using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Transiflow
{
    public interface IState<TStateTag>
    {
        public TStateTag Tag { get; }
    }
}