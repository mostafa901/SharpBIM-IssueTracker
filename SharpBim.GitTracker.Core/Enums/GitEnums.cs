using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBim.GitTracker.Core.Enums
{
    [Flags]
    public enum IssueState
    {
        open = 0, closed = 1, all = 2
    }
}