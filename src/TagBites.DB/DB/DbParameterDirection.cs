﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.DB
{
    [Flags]
    public enum DbParameterDirection
    {
        Input = 1,
        Output = 2,
        InputOutput = 3,
        ReturnValue = 6,
    }
}
