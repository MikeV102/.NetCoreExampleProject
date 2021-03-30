using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tutorial5.Services
{
    public interface IDbService
    {
        bool ExistsIndex(string index);
    }
}
