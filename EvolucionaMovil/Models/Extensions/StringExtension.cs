﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EvolucionaMovil.Models.Extensions
{
    public static class StringExtension
    {
        public static bool ContainsInvariant(this string mainText, string queryText)
        {
            if (string.IsNullOrEmpty(mainText))
            {
                return false;
            }
            return mainText.ToUpperInvariant().Contains(queryText.ToUpperInvariant());
        }
    }
}