using System;

namespace Autodash.Core
{
    public class GridConsoleScraperException : Exception
    {
        public GridConsoleScraperException(Uri uri, Exception ex) : 
            base("Cannot reach Grid Console at: " + uri.ToString(), ex)
        {
        }
    }
}